using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    internal static class OfficeDaakAttachmentService
    {
        private const int MaximumPdfBytes = 5 * 1024 * 1024;
        private const int MaximumFilesPerUpload = 20;
        private static string ConnectionString { get { ConnectionStringSettings s = ConfigurationManager.ConnectionStrings["SchoolDB"]; if (s == null || string.IsNullOrWhiteSpace(s.ConnectionString)) throw new ConfigurationErrorsException("The SchoolDB connection string is missing."); return s.ConnectionString; } }

        internal static void EnsureSchema()
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.OfficeDaakDocuments',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.OfficeDaakDocuments(
  DocumentID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OfficeDaakDocuments PRIMARY KEY,
  RecordType NVARCHAR(10) NOT NULL, RecordID INT NOT NULL, SequenceNo INT NOT NULL,
  OriginalFileName NVARCHAR(260) NOT NULL, FileContentType NVARCHAR(100) NOT NULL,
  FileSizeBytes INT NOT NULL, DocumentData VARBINARY(MAX) NOT NULL, DocumentSha256 CHAR(64) NOT NULL,
  UploadedByUserID INT NULL, UploadedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_OfficeDaakDocuments_Uploaded DEFAULT(SYSUTCDATETIME()),
  CONSTRAINT CK_OfficeDaakDocuments_Type CHECK(RecordType IN(N'Incoming',N'Outgoing')),
  CONSTRAINT CK_OfficeDaakDocuments_Size CHECK(FileSizeBytes>0 AND FileSizeBytes<=5242880),
  CONSTRAINT UX_OfficeDaakDocuments_Sequence UNIQUE(RecordType,RecordID,SequenceNo));
 CREATE INDEX IX_OfficeDaakDocuments_Record ON dbo.OfficeDaakDocuments(RecordType,RecordID,DocumentID);
END;
INSERT dbo.OfficeDaakDocuments(RecordType,RecordID,SequenceNo,OriginalFileName,FileContentType,FileSizeBytes,DocumentData,DocumentSha256,UploadedAtUtc)
SELECT N'Incoming',d.DiaryID,1,ISNULL(NULLIF(d.OriginalFileName,N''),N'diary-scan'),ISNULL(NULLIF(d.FileContentType,N''),N'application/octet-stream'),CONVERT(int,DATALENGTH(d.DocumentData)),d.DocumentData,ISNULL(NULLIF(d.DocumentSha256,''),CONVERT(char(64),HASHBYTES('SHA2_256',d.DocumentData),2)),d.CreatedAtUtc
FROM dbo.OfficeDaakDiary d WHERE d.DocumentData IS NOT NULL AND DATALENGTH(d.DocumentData)<=5242880 AND NOT EXISTS(SELECT 1 FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Incoming' AND x.RecordID=d.DiaryID);
INSERT dbo.OfficeDaakDocuments(RecordType,RecordID,SequenceNo,OriginalFileName,FileContentType,FileSizeBytes,DocumentData,DocumentSha256,UploadedAtUtc)
SELECT N'Outgoing',d.DispatchID,1,ISNULL(NULLIF(d.OriginalFileName,N''),N'dispatch-scan'),ISNULL(NULLIF(d.FileContentType,N''),N'application/octet-stream'),CONVERT(int,DATALENGTH(d.DocumentData)),d.DocumentData,ISNULL(NULLIF(d.DocumentSha256,''),CONVERT(char(64),HASHBYTES('SHA2_256',d.DocumentData),2)),d.CreatedAtUtc
FROM dbo.OfficeDaakDispatch d WHERE d.DocumentData IS NOT NULL AND DATALENGTH(d.DocumentData)<=5242880 AND NOT EXISTS(SELECT 1 FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Outgoing' AND x.RecordID=d.DispatchID);
UPDATE d SET DocumentData=NULL,DocumentSha256=NULL,StoredFileName=NULL,OriginalFileName=NULL,FileContentType=NULL,FileSizeBytes=NULL FROM dbo.OfficeDaakDiary d WHERE EXISTS(SELECT 1 FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Incoming' AND x.RecordID=d.DiaryID);
UPDATE d SET DocumentData=NULL,DocumentSha256=NULL,StoredFileName=NULL,OriginalFileName=NULL,FileContentType=NULL,FileSizeBytes=NULL FROM dbo.OfficeDaakDispatch d WHERE EXISTS(SELECT 1 FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Outgoing' AND x.RecordID=d.DispatchID);";
            using (SqlConnection c = new SqlConnection(ConnectionString)) using (SqlCommand q = new SqlCommand(sql, c)) { q.CommandTimeout = 120; c.Open(); q.ExecuteNonQuery(); }
        }

        internal static IList<DaakAttachmentUpload> ReadUploads(FileUpload upload)
        {
            List<DaakAttachmentUpload> files = new List<DaakAttachmentUpload>(); if (upload == null || !upload.HasFiles) return files;
            if (upload.PostedFiles.Count > MaximumFilesPerUpload) throw new InvalidOperationException("Upload no more than 20 scanned pages at one time.");
            foreach (HttpPostedFile posted in upload.PostedFiles)
            {
                if (posted == null || posted.ContentLength <= 0) continue;
                string name = Path.GetFileName(posted.FileName ?? string.Empty); string ext = Path.GetExtension(name).ToLowerInvariant(); byte[] data; string contentType;
                if (ImageUploadProcessor.IsCompressibleImageExtension(ext)) { ProcessedImage image = ImageUploadProcessor.ReadAndOptimize(posted, 2000, 2600); data = image.Data; name = image.FileName; contentType = image.ContentType; }
                else if (ext == ".pdf") { if (posted.ContentLength > MaximumPdfBytes) throw new InvalidOperationException("Each PDF Daak copy must be no larger than 5 MB."); if (posted.InputStream.CanSeek) posted.InputStream.Position = 0; using (BinaryReader r = new BinaryReader(posted.InputStream)) data = r.ReadBytes(posted.ContentLength); if (data.Length < 5 || data[0] != 0x25 || data[1] != 0x50 || data[2] != 0x44 || data[3] != 0x46 || data[4] != 0x2D) throw new InvalidOperationException("The file " + name + " is not a valid PDF."); contentType = "application/pdf"; }
                else throw new InvalidOperationException("Daak pages must be JPG, JPEG, PNG, GIF, BMP, or PDF files.");
                if (name.Length > 260) name = name.Substring(name.Length - 260); files.Add(new DaakAttachmentUpload { FileName = name, ContentType = contentType, Data = data, Sha256 = Hash(data) });
            }
            return files;
        }

        internal static void AddDocuments(string recordType, int recordId, IList<DaakAttachmentUpload> files, int? userId)
        {
            recordType = Type(recordType); if (recordId <= 0 || files == null || files.Count == 0) return;
            using (SqlConnection c = new SqlConnection(ConnectionString)) { c.Open(); using (SqlTransaction t = c.BeginTransaction(IsolationLevel.Serializable)) { int sequence; using (SqlCommand n = new SqlCommand("SELECT ISNULL(MAX(SequenceNo),0) FROM dbo.OfficeDaakDocuments WITH(UPDLOCK,HOLDLOCK) WHERE RecordType=@Type AND RecordID=@ID;", c, t)) { n.Parameters.Add("@Type", SqlDbType.NVarChar, 10).Value = recordType; n.Parameters.Add("@ID", SqlDbType.Int).Value = recordId; sequence = Convert.ToInt32(n.ExecuteScalar(), CultureInfo.InvariantCulture); } foreach (DaakAttachmentUpload file in files) { sequence++; using (SqlCommand q = new SqlCommand(@"INSERT dbo.OfficeDaakDocuments(RecordType,RecordID,SequenceNo,OriginalFileName,FileContentType,FileSizeBytes,DocumentData,DocumentSha256,UploadedByUserID) VALUES(@Type,@ID,@Sequence,@Name,@ContentType,@Size,@Data,@Hash,@UserID);", c, t)) { q.Parameters.Add("@Type", SqlDbType.NVarChar, 10).Value = recordType; q.Parameters.Add("@ID", SqlDbType.Int).Value = recordId; q.Parameters.Add("@Sequence", SqlDbType.Int).Value = sequence; q.Parameters.Add("@Name", SqlDbType.NVarChar, 260).Value = file.FileName; q.Parameters.Add("@ContentType", SqlDbType.NVarChar, 100).Value = file.ContentType; q.Parameters.Add("@Size", SqlDbType.Int).Value = file.Data.Length; q.Parameters.Add("@Data", SqlDbType.VarBinary, -1).Value = file.Data; q.Parameters.Add("@Hash", SqlDbType.Char, 64).Value = file.Sha256; q.Parameters.Add("@UserID", SqlDbType.Int).Value = userId.HasValue ? (object)userId.Value : DBNull.Value; q.ExecuteNonQuery(); } } t.Commit(); } }
        }

        internal static DataTable GetDocuments(string type, int id) { return Fill("SELECT DocumentID,SequenceNo,OriginalFileName,FileContentType,FileSizeBytes,UploadedAtUtc FROM dbo.OfficeDaakDocuments WHERE RecordType=@Type AND RecordID=@ID ORDER BY SequenceNo,DocumentID;", P("@Type", SqlDbType.NVarChar, 10, Type(type)), P("@ID", SqlDbType.Int, 0, id)); }
        internal static DataRow GetRecord(string type, int id) { string sql = Type(type) == "Incoming" ? "SELECT *,DiaryNo AS RegisterNo,ReceivedDate AS RegisterDate,SenderOffice AS Correspondent FROM dbo.OfficeDaakDiary WHERE DiaryID=@ID;" : "SELECT *,DispatchNo AS RegisterNo,DispatchDate AS RegisterDate,RecipientOffice AS Correspondent FROM dbo.OfficeDaakDispatch WHERE DispatchID=@ID;"; DataTable t = Fill(sql, P("@ID", SqlDbType.Int, 0, id)); if (t.Rows.Count == 0) throw new InvalidOperationException("The selected Daak record was not found."); return t.Rows[0]; }
        internal static DaakAttachmentUpload GetDocument(int documentId) { DataTable t = Fill("SELECT OriginalFileName,FileContentType,DocumentData,DocumentSha256 FROM dbo.OfficeDaakDocuments WHERE DocumentID=@ID;", P("@ID", SqlDbType.Int, 0, documentId)); if (t.Rows.Count == 0) return null; DataRow r = t.Rows[0]; return new DaakAttachmentUpload { FileName = Convert.ToString(r["OriginalFileName"], CultureInfo.InvariantCulture), ContentType = Convert.ToString(r["FileContentType"], CultureInfo.InvariantCulture), Data = (byte[])r["DocumentData"], Sha256 = Convert.ToString(r["DocumentSha256"], CultureInfo.InvariantCulture) }; }
        internal static void DeleteDocument(int documentId) { using (SqlConnection c = new SqlConnection(ConnectionString)) using (SqlCommand q = new SqlCommand("DELETE dbo.OfficeDaakDocuments WHERE DocumentID=@ID;", c)) { q.Parameters.Add("@ID", SqlDbType.Int).Value = documentId; c.Open(); q.ExecuteNonQuery(); } }
        internal static int GetRecordId(string type, string number) { string sql = Type(type) == "Incoming" ? "SELECT DiaryID FROM dbo.OfficeDaakDiary WHERE DiaryNo=@No;" : "SELECT DispatchID FROM dbo.OfficeDaakDispatch WHERE DispatchNo=@No;"; DataTable t = Fill(sql, P("@No", SqlDbType.NVarChar, 30, number)); return t.Rows.Count == 0 ? 0 : Convert.ToInt32(t.Rows[0][0], CultureInfo.InvariantCulture); }

        internal static string UpdateDiary(int id, DaakDiaryRecord r) { const string sql = @"UPDATE dbo.OfficeDaakDiary SET ReceivedDate=@Date,LetterDate=@LetterDate,SenderOffice=@Office,SenderReferenceNo=@Reference,Subject=@Subject,Description=@Description,Category=@Category,Priority=@Priority,DeliveryMode=@Mode,AssignedTo=@Assigned,ActionDueDate=@Due,Status=@Status,Remarks=@Remarks,UpdatedAtUtc=SYSUTCDATETIME() WHERE DiaryID=@ID;SELECT DiaryNo FROM dbo.OfficeDaakDiary WHERE DiaryID=@ID;"; using (SqlConnection c = new SqlConnection(ConnectionString)) using (SqlCommand q = new SqlCommand(sql, c)) { q.Parameters.Add("@ID", SqlDbType.Int).Value = id; q.Parameters.Add("@Date", SqlDbType.Date).Value = r.ReceivedDate.Date; Date(q, "@LetterDate", r.LetterDate); Text(q, "@Office", 200, r.SenderOffice); Text(q, "@Reference", 100, r.SenderReferenceNo); Text(q, "@Subject", 300, r.Subject); Text(q, "@Description", -1, r.Description); Text(q, "@Category", 50, r.Category); Text(q, "@Priority", 20, r.Priority); Text(q, "@Mode", 50, r.DeliveryMode); Text(q, "@Assigned", 150, r.AssignedTo); Date(q, "@Due", r.ActionDueDate); Text(q, "@Status", 30, r.Status); Text(q, "@Remarks", 500, r.Remarks); c.Open(); object value = q.ExecuteScalar(); if (value == null) throw new InvalidOperationException("The Daak diary entry was not found."); return Convert.ToString(value, CultureInfo.InvariantCulture); } }
        internal static string UpdateDispatch(int id, DaakDispatchRecord r) { const string sql = @"UPDATE dbo.OfficeDaakDispatch SET DispatchDate=@Date,LetterDate=@LetterDate,RecipientOffice=@Office,RecipientAddress=@Address,RecipientContact=@Contact,Subject=@Subject,ReferenceNo=@Reference,Description=@Description,Category=@Category,Priority=@Priority,DispatchMode=@Mode,TrackingNo=@Tracking,SignedBy=@Signed,PreparedBy=@Prepared,Status=@Status,Remarks=@Remarks,UpdatedAtUtc=SYSUTCDATETIME() WHERE DispatchID=@ID;SELECT DispatchNo FROM dbo.OfficeDaakDispatch WHERE DispatchID=@ID;"; using (SqlConnection c = new SqlConnection(ConnectionString)) using (SqlCommand q = new SqlCommand(sql, c)) { q.Parameters.Add("@ID", SqlDbType.Int).Value = id; q.Parameters.Add("@Date", SqlDbType.Date).Value = r.DispatchDate.Date; Date(q, "@LetterDate", r.LetterDate); Text(q, "@Office", 200, r.RecipientOffice); Text(q, "@Address", 350, r.RecipientAddress); Text(q, "@Contact", 100, r.RecipientContact); Text(q, "@Subject", 300, r.Subject); Text(q, "@Reference", 100, r.ReferenceNo); Text(q, "@Description", -1, r.Description); Text(q, "@Category", 50, r.Category); Text(q, "@Priority", 20, r.Priority); Text(q, "@Mode", 50, r.DispatchMode); Text(q, "@Tracking", 100, r.TrackingNo); Text(q, "@Signed", 150, r.SignedBy); Text(q, "@Prepared", 150, r.PreparedBy); Text(q, "@Status", 30, r.Status); Text(q, "@Remarks", 500, r.Remarks); c.Open(); object value = q.ExecuteScalar(); if (value == null) throw new InvalidOperationException("The Daak dispatch entry was not found."); return Convert.ToString(value, CultureInfo.InvariantCulture); } }

        private static DataTable Fill(string sql, params SqlParameter[] parameters) { DataTable t = new DataTable(); using (SqlConnection c = new SqlConnection(ConnectionString)) using (SqlCommand q = new SqlCommand(sql, c)) using (SqlDataAdapter a = new SqlDataAdapter(q)) { if (parameters != null) q.Parameters.AddRange(parameters); a.Fill(t); } return t; }
        private static SqlParameter P(string name, SqlDbType type, int size, object value) { SqlParameter p = size > 0 ? new SqlParameter(name, type, size) : new SqlParameter(name, type); p.Value = value ?? DBNull.Value; return p; }
        private static void Text(SqlCommand q, string name, int size, string value) { value = (value ?? string.Empty).Trim(); q.Parameters.Add(name, SqlDbType.NVarChar, size).Value = value.Length == 0 ? (object)DBNull.Value : value; }
        private static void Date(SqlCommand q, string name, DateTime? value) { q.Parameters.Add(name, SqlDbType.Date).Value = value.HasValue ? (object)value.Value.Date : DBNull.Value; }
        private static string Type(string value) { value = (value ?? string.Empty).Trim(); if (value.Equals("incoming", StringComparison.OrdinalIgnoreCase)) return "Incoming"; if (value.Equals("outgoing", StringComparison.OrdinalIgnoreCase)) return "Outgoing"; throw new InvalidOperationException("Select an incoming or outgoing Daak register."); }
        private static string Hash(byte[] data) { using (SHA256 sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", string.Empty).ToLowerInvariant(); }
    }

    internal sealed class DaakAttachmentUpload { internal string FileName { get; set; } internal string ContentType { get; set; } internal byte[] Data { get; set; } internal string Sha256 { get; set; } }
}
