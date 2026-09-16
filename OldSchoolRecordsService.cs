using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web;

namespace DigitalSchoolManager
{
    internal static class OldSchoolRecordsService
    {
        private const int MaximumPdfBytes = 5 * 1024 * 1024;
        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString; } }

        internal static void EnsureSchema()
        {
            string path = HttpContext.Current.Server.MapPath("~/OldSchoolRecords_Database_Update.sql");
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(File.ReadAllText(path), connection))
            { command.CommandTimeout = 120; connection.Open(); command.ExecuteNonQuery(); }
        }

        internal static DataTable Search(string search, string category)
        {
            return Fill(@"SELECT r.RecordID,r.ArchiveNumber,r.Category,r.RecordTitle,r.RecordReference,r.RegisterYear,
 r.PhysicalLocation,r.ConfidentialityLevel,r.RecordStartDate,r.RecordEndDate,r.Description,r.Keywords,
 COUNT(d.DocumentID) AS DocumentCount,ISNULL(SUM(CONVERT(bigint,d.FileSizeBytes)),0) AS TotalBytes
FROM dbo.OldSchoolRecords r LEFT JOIN dbo.OldSchoolRecordDocuments d ON d.RecordID=r.RecordID
WHERE r.IsActive=1 AND (@Category=N'' OR r.Category=@Category) AND
 (@Search=N'' OR r.ArchiveNumber LIKE @Pattern OR r.RecordTitle LIKE @Pattern OR ISNULL(r.RecordReference,N'') LIKE @Pattern OR ISNULL(r.Keywords,N'') LIKE @Pattern)
GROUP BY r.RecordID,r.ArchiveNumber,r.Category,r.RecordTitle,r.RecordReference,r.RegisterYear,r.PhysicalLocation,r.ConfidentialityLevel,r.RecordStartDate,r.RecordEndDate,r.Description,r.Keywords,r.UpdatedAtUtc
ORDER BY r.UpdatedAtUtc DESC,r.RecordID DESC;", Text("@Category", 80, category), Text("@Search", 150, search), Text("@Pattern", 170, "%" + search + "%"));
        }

        internal static DataRow GetRecord(int id)
        {
            DataTable table = Fill("SELECT * FROM dbo.OldSchoolRecords WHERE RecordID=@ID AND IsActive=1;", Int("@ID", id));
            if (table.Rows.Count == 0) throw new InvalidOperationException("The archive record was not found.");
            return table.Rows[0];
        }

        internal static string SaveRecord(OldSchoolRecordInput input)
        {
            input.Category = Required(input.Category, "record category", 80);
            input.RecordTitle = Required(input.RecordTitle, "record title", 250);
            if (input.StartDate.HasValue && input.EndDate.HasValue && input.StartDate.Value > input.EndDate.Value)
                throw new InvalidOperationException("The record start date cannot be later than the end date.");
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                if (input.RecordID > 0)
                {
                    using (SqlCommand update = new SqlCommand(@"UPDATE dbo.OldSchoolRecords SET Category=@Category,RecordTitle=@Title,RecordReference=@Reference,
 RecordStartDate=@StartDate,RecordEndDate=@EndDate,RegisterYear=@Year,PhysicalLocation=@Location,ConfidentialityLevel=@Confidentiality,
 Description=@Description,Keywords=@Keywords,UpdatedAtUtc=SYSUTCDATETIME() WHERE RecordID=@ID AND IsActive=1;SELECT ArchiveNumber FROM dbo.OldSchoolRecords WHERE RecordID=@ID;", connection))
                    { AddRecordParameters(update, input); update.Parameters.Add("@ID", SqlDbType.Int).Value = input.RecordID; return Convert.ToString(update.ExecuteScalar(), CultureInfo.InvariantCulture); }
                }
                int id;
                using (SqlCommand insert = new SqlCommand(@"INSERT dbo.OldSchoolRecords(ArchiveNumber,Category,RecordTitle,RecordReference,RecordStartDate,RecordEndDate,RegisterYear,PhysicalLocation,ConfidentialityLevel,Description,Keywords,CreatedByUserID)
VALUES(N'PENDING-'+CONVERT(nvarchar(36),NEWID()),@Category,@Title,@Reference,@StartDate,@EndDate,@Year,@Location,@Confidentiality,@Description,@Keywords,@UserID);SELECT CAST(SCOPE_IDENTITY() AS int);", connection))
                { AddRecordParameters(insert, input); id = Convert.ToInt32(insert.ExecuteScalar(), CultureInfo.InvariantCulture); }
                string number = string.Format(CultureInfo.InvariantCulture, "GHSS-ARC-{0:yyyyMMdd}-{1:D4}", DateTime.Today, id);
                using (SqlCommand update = new SqlCommand("UPDATE dbo.OldSchoolRecords SET ArchiveNumber=@Number WHERE RecordID=@ID;", connection))
                { update.Parameters.Add("@Number", SqlDbType.NVarChar, 60).Value = number; update.Parameters.Add("@ID", SqlDbType.Int).Value = id; update.ExecuteNonQuery(); }
                input.RecordID = id; return number;
            }
        }

        internal static int AddDocuments(int recordId, IEnumerable<HttpPostedFile> uploads, int? userId)
        {
            List<ArchiveDocument> documents = new List<ArchiveDocument>();
            foreach (HttpPostedFile upload in uploads)
            {
                if (upload == null || upload.ContentLength <= 0) continue;
                string extension = Path.GetExtension(upload.FileName ?? string.Empty).ToLowerInvariant();
                if (extension == ".pdf") documents.Add(ReadPdf(upload));
                else
                {
                    ProcessedImage image = ImageUploadProcessor.ReadAndOptimize(upload, 2200, 3000);
                    documents.Add(new ArchiveDocument { FileName = image.FileName, ContentType = image.ContentType, Data = image.Data });
                }
            }
            if (documents.Count == 0) throw new InvalidOperationException("Select at least one scanned image or PDF file.");
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    int next;
                    using (SqlCommand read = new SqlCommand("SELECT ISNULL(MAX(PageNumber),0)+1 FROM dbo.OldSchoolRecordDocuments WHERE RecordID=@ID;", connection, transaction))
                    { read.Parameters.Add("@ID", SqlDbType.Int).Value = recordId; next = Convert.ToInt32(read.ExecuteScalar(), CultureInfo.InvariantCulture); }
                    foreach (ArchiveDocument document in documents)
                    {
                        using (SqlCommand insert = new SqlCommand(@"INSERT dbo.OldSchoolRecordDocuments(RecordID,PageNumber,OriginalFileName,ContentType,FileSizeBytes,DocumentData,UploadedByUserID)
VALUES(@RecordID,@Page,@Name,@Type,@Size,@Data,@UserID);", connection, transaction))
                        {
                            insert.Parameters.Add("@RecordID", SqlDbType.Int).Value = recordId; insert.Parameters.Add("@Page", SqlDbType.Int).Value = next++;
                            insert.Parameters.Add("@Name", SqlDbType.NVarChar, 260).Value = document.FileName; insert.Parameters.Add("@Type", SqlDbType.NVarChar, 100).Value = document.ContentType;
                            insert.Parameters.Add("@Size", SqlDbType.Int).Value = document.Data.Length; insert.Parameters.Add("@Data", SqlDbType.VarBinary, -1).Value = document.Data;
                            insert.Parameters.Add("@UserID", SqlDbType.Int).Value = userId.HasValue ? (object)userId.Value : DBNull.Value; insert.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
            return documents.Count;
        }

        internal static DataTable GetDocuments(int recordId) { return Fill("SELECT DocumentID,PageNumber,OriginalFileName,ContentType,FileSizeBytes,UploadedAtUtc FROM dbo.OldSchoolRecordDocuments WHERE RecordID=@ID ORDER BY PageNumber;", Int("@ID", recordId)); }
        internal static ArchiveDocument GetDocument(int id)
        {
            DataTable table = Fill("SELECT OriginalFileName,ContentType,DocumentData FROM dbo.OldSchoolRecordDocuments WHERE DocumentID=@ID;", Int("@ID", id));
            if (table.Rows.Count == 0) return null; DataRow row = table.Rows[0];
            return new ArchiveDocument { FileName = Convert.ToString(row["OriginalFileName"], CultureInfo.InvariantCulture), ContentType = Convert.ToString(row["ContentType"], CultureInfo.InvariantCulture), Data = (byte[])row["DocumentData"] };
        }

        private static ArchiveDocument ReadPdf(HttpPostedFile upload)
        {
            if (upload.ContentLength > MaximumPdfBytes) throw new InvalidOperationException("Each PDF archive document must be no larger than 5 MB.");
            byte[] data; using (BinaryReader reader = new BinaryReader(upload.InputStream)) data = reader.ReadBytes(upload.ContentLength);
            if (data.Length < 5 || data[0] != 0x25 || data[1] != 0x50 || data[2] != 0x44 || data[3] != 0x46 || data[4] != 0x2D) throw new InvalidOperationException("A selected PDF file is not valid.");
            return new ArchiveDocument { FileName = Path.GetFileName(upload.FileName), ContentType = "application/pdf", Data = data };
        }
        private static void AddRecordParameters(SqlCommand command, OldSchoolRecordInput input)
        {
            command.Parameters.Add("@Category", SqlDbType.NVarChar, 80).Value = input.Category; command.Parameters.Add("@Title", SqlDbType.NVarChar, 250).Value = input.RecordTitle;
            NullableText(command, "@Reference", 120, input.RecordReference); NullableDate(command, "@StartDate", input.StartDate); NullableDate(command, "@EndDate", input.EndDate);
            NullableText(command, "@Year", 30, input.RegisterYear); NullableText(command, "@Location", 250, input.PhysicalLocation);
            command.Parameters.Add("@Confidentiality", SqlDbType.NVarChar, 30).Value = string.IsNullOrWhiteSpace(input.ConfidentialityLevel) ? "Official" : input.ConfidentialityLevel.Trim();
            NullableText(command, "@Description", 1500, input.Description); NullableText(command, "@Keywords", 500, input.Keywords);
            command.Parameters.Add("@UserID", SqlDbType.Int).Value = input.UserID.HasValue ? (object)input.UserID.Value : DBNull.Value;
        }
        private static DataTable Fill(string sql, params SqlParameter[] parameters) { DataTable table = new DataTable(); using (SqlConnection c = new SqlConnection(ConnectionString)) using (SqlCommand q = new SqlCommand(sql, c)) using (SqlDataAdapter a = new SqlDataAdapter(q)) { if (parameters != null) q.Parameters.AddRange(parameters); a.Fill(table); } return table; }
        private static SqlParameter Text(string n, int s, string v) { return new SqlParameter(n, SqlDbType.NVarChar, s) { Value = (v ?? string.Empty).Trim() }; }
        private static SqlParameter Int(string n, int v) { return new SqlParameter(n, SqlDbType.Int) { Value = v }; }
        private static string Required(string v, string label, int max) { v = (v ?? string.Empty).Trim(); if (v.Length == 0) throw new InvalidOperationException("Enter the " + label + "."); if (v.Length > max) throw new InvalidOperationException("The " + label + " is too long."); return v; }
        private static void NullableText(SqlCommand q, string n, int s, string v) { v = (v ?? string.Empty).Trim(); if (v.Length > s) v = v.Substring(0, s); q.Parameters.Add(n, SqlDbType.NVarChar, s).Value = v.Length == 0 ? (object)DBNull.Value : v; }
        private static void NullableDate(SqlCommand q, string n, DateTime? v) { q.Parameters.Add(n, SqlDbType.Date).Value = v.HasValue ? (object)v.Value.Date : DBNull.Value; }
    }

    internal sealed class OldSchoolRecordInput
    {
        internal int RecordID { get; set; } internal string Category { get; set; } internal string RecordTitle { get; set; }
        internal string RecordReference { get; set; } internal DateTime? StartDate { get; set; } internal DateTime? EndDate { get; set; }
        internal string RegisterYear { get; set; } internal string PhysicalLocation { get; set; } internal string ConfidentialityLevel { get; set; }
        internal string Description { get; set; } internal string Keywords { get; set; } internal int? UserID { get; set; }
    }
    internal sealed class ArchiveDocument { internal string FileName { get; set; } internal string ContentType { get; set; } internal byte[] Data { get; set; } }
}
