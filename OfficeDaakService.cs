using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Web.Hosting;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    internal static class OfficeDaakService
    {
        private const int MaximumDocumentBytes = 5 * 1024 * 1024;
        private static readonly object LegacyMigrationLock = new object();
        private static bool _legacyMigrationAttempted;

        private static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString; }
        }

        internal static void EnsureSchema()
        {
            const string sql = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.OfficeDaakCounters', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OfficeDaakCounters
    (
        RegisterType NVARCHAR(20) NOT NULL,
        CounterYear SMALLINT NOT NULL,
        LastNumber INT NOT NULL,
        CONSTRAINT PK_OfficeDaakCounters PRIMARY KEY (RegisterType, CounterYear),
        CONSTRAINT CK_OfficeDaakCounters_LastNumber CHECK (LastNumber > 0)
    );
END;

IF OBJECT_ID(N'dbo.OfficeDaakDiary', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OfficeDaakDiary
    (
        DiaryID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_OfficeDaakDiary PRIMARY KEY,
        DiaryNo NVARCHAR(30) NOT NULL,
        ReceivedDate DATE NOT NULL,
        LetterDate DATE NULL,
        SenderOffice NVARCHAR(200) NOT NULL,
        SenderReferenceNo NVARCHAR(100) NULL,
        Subject NVARCHAR(300) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Category NVARCHAR(50) NOT NULL,
        Priority NVARCHAR(20) NOT NULL,
        DeliveryMode NVARCHAR(50) NULL,
        AssignedTo NVARCHAR(150) NULL,
        ActionDueDate DATE NULL,
        Status NVARCHAR(30) NOT NULL,
        Remarks NVARCHAR(500) NULL,
        StoredFileName NVARCHAR(260) NULL,
        OriginalFileName NVARCHAR(260) NULL,
        FileContentType NVARCHAR(100) NULL,
        FileSizeBytes BIGINT NULL,
        DocumentData VARBINARY(MAX) NULL,
        DocumentSha256 CHAR(64) NULL,
        CreatedByUserID INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_OfficeDaakDiary_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_OfficeDaakDiary_UpdatedAtUtc DEFAULT (SYSUTCDATETIME())
    );
END;

IF OBJECT_ID(N'dbo.OfficeDaakDispatch', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OfficeDaakDispatch
    (
        DispatchID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_OfficeDaakDispatch PRIMARY KEY,
        DispatchNo NVARCHAR(30) NOT NULL,
        DispatchDate DATE NOT NULL,
        LetterDate DATE NULL,
        RecipientOffice NVARCHAR(200) NOT NULL,
        RecipientAddress NVARCHAR(350) NULL,
        RecipientContact NVARCHAR(100) NULL,
        Subject NVARCHAR(300) NOT NULL,
        ReferenceNo NVARCHAR(100) NULL,
        Description NVARCHAR(MAX) NULL,
        Category NVARCHAR(50) NOT NULL,
        Priority NVARCHAR(20) NOT NULL,
        DispatchMode NVARCHAR(50) NULL,
        TrackingNo NVARCHAR(100) NULL,
        SignedBy NVARCHAR(150) NULL,
        PreparedBy NVARCHAR(150) NULL,
        Status NVARCHAR(30) NOT NULL,
        Remarks NVARCHAR(500) NULL,
        StoredFileName NVARCHAR(260) NULL,
        OriginalFileName NVARCHAR(260) NULL,
        FileContentType NVARCHAR(100) NULL,
        FileSizeBytes BIGINT NULL,
        DocumentData VARBINARY(MAX) NULL,
        DocumentSha256 CHAR(64) NULL,
        CreatedByUserID INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_OfficeDaakDispatch_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_OfficeDaakDispatch_UpdatedAtUtc DEFAULT (SYSUTCDATETIME())
    );
END;

IF COL_LENGTH(N'dbo.OfficeDaakDiary', N'DocumentData') IS NULL
    ALTER TABLE dbo.OfficeDaakDiary ADD DocumentData VARBINARY(MAX) NULL;
IF COL_LENGTH(N'dbo.OfficeDaakDiary', N'DocumentSha256') IS NULL
    ALTER TABLE dbo.OfficeDaakDiary ADD DocumentSha256 CHAR(64) NULL;
IF COL_LENGTH(N'dbo.OfficeDaakDispatch', N'DocumentData') IS NULL
    ALTER TABLE dbo.OfficeDaakDispatch ADD DocumentData VARBINARY(MAX) NULL;
IF COL_LENGTH(N'dbo.OfficeDaakDispatch', N'DocumentSha256') IS NULL
    ALTER TABLE dbo.OfficeDaakDispatch ADD DocumentSha256 CHAR(64) NULL;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.OfficeDaakDiary')
      AND name = N'UX_OfficeDaakDiary_DiaryNo'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_OfficeDaakDiary_DiaryNo
        ON dbo.OfficeDaakDiary (DiaryNo);
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.OfficeDaakDiary')
      AND name = N'IX_OfficeDaakDiary_Register'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_OfficeDaakDiary_Register
        ON dbo.OfficeDaakDiary (ReceivedDate DESC, Status)
        INCLUDE (DiaryNo, SenderOffice, Subject, Priority);
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.OfficeDaakDispatch')
      AND name = N'UX_OfficeDaakDispatch_DispatchNo'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_OfficeDaakDispatch_DispatchNo
        ON dbo.OfficeDaakDispatch (DispatchNo);
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.OfficeDaakDispatch')
      AND name = N'IX_OfficeDaakDispatch_Register'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_OfficeDaakDispatch_Register
        ON dbo.OfficeDaakDispatch (DispatchDate DESC, Status)
        INCLUDE (DispatchNo, RecipientOffice, Subject, Priority);
END;

COMMIT TRANSACTION;";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }

            if (!_legacyMigrationAttempted)
            {
                lock (LegacyMigrationLock)
                {
                    if (!_legacyMigrationAttempted)
                    {
                        MigrateLegacyDocuments();
                        _legacyMigrationAttempted = true;
                    }
                }
            }
        }

        internal static string CreateDiary(DaakDiaryRecord record)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    string diaryNo = GetNextRegisterNumber(connection, transaction, "DIARY", "DY", record.ReceivedDate);
                    const string sql = @"
INSERT INTO dbo.OfficeDaakDiary
       (DiaryNo, ReceivedDate, LetterDate, SenderOffice, SenderReferenceNo,
        Subject, Description, Category, Priority, DeliveryMode, AssignedTo,
        ActionDueDate, Status, Remarks, StoredFileName, OriginalFileName,
        FileContentType, FileSizeBytes, DocumentData, DocumentSha256,
        CreatedByUserID, CreatedAtUtc, UpdatedAtUtc)
VALUES (@DiaryNo, @ReceivedDate, @LetterDate, @SenderOffice, @SenderReferenceNo,
        @Subject, @Description, @Category, @Priority, @DeliveryMode, @AssignedTo,
        @ActionDueDate, @Status, @Remarks, @StoredFileName, @OriginalFileName,
        @FileContentType, @FileSizeBytes, @DocumentData, @DocumentSha256,
        @CreatedByUserID, SYSUTCDATETIME(), SYSUTCDATETIME());";

                    using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                    {
                        command.Parameters.Add("@DiaryNo", SqlDbType.NVarChar, 30).Value = diaryNo;
                        command.Parameters.Add("@ReceivedDate", SqlDbType.Date).Value = record.ReceivedDate.Date;
                        AddNullableDate(command, "@LetterDate", record.LetterDate);
                        AddRequiredText(command, "@SenderOffice", 200, record.SenderOffice);
                        AddNullableText(command, "@SenderReferenceNo", 100, record.SenderReferenceNo);
                        AddRequiredText(command, "@Subject", 300, record.Subject);
                        AddNullableText(command, "@Description", -1, record.Description);
                        AddRequiredText(command, "@Category", 50, record.Category);
                        AddRequiredText(command, "@Priority", 20, record.Priority);
                        AddNullableText(command, "@DeliveryMode", 50, record.DeliveryMode);
                        AddNullableText(command, "@AssignedTo", 150, record.AssignedTo);
                        AddNullableDate(command, "@ActionDueDate", record.ActionDueDate);
                        AddRequiredText(command, "@Status", 30, record.Status);
                        AddNullableText(command, "@Remarks", 500, record.Remarks);
                        AddDocumentParameters(command, record.Document);
                        AddNullableInt(command, "@CreatedByUserID", record.CreatedByUserID);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return diaryNo;
                }
            }
        }

        internal static string CreateDispatch(DaakDispatchRecord record)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    string dispatchNo = GetNextRegisterNumber(connection, transaction, "DISPATCH", "DSP", record.DispatchDate);
                    const string sql = @"
INSERT INTO dbo.OfficeDaakDispatch
       (DispatchNo, DispatchDate, LetterDate, RecipientOffice, RecipientAddress,
        RecipientContact, Subject, ReferenceNo, Description, Category, Priority,
        DispatchMode, TrackingNo, SignedBy, PreparedBy, Status, Remarks,
        StoredFileName, OriginalFileName, FileContentType, FileSizeBytes,
        DocumentData, DocumentSha256, CreatedByUserID, CreatedAtUtc, UpdatedAtUtc)
VALUES (@DispatchNo, @DispatchDate, @LetterDate, @RecipientOffice, @RecipientAddress,
        @RecipientContact, @Subject, @ReferenceNo, @Description, @Category, @Priority,
        @DispatchMode, @TrackingNo, @SignedBy, @PreparedBy, @Status, @Remarks,
        @StoredFileName, @OriginalFileName, @FileContentType, @FileSizeBytes,
        @DocumentData, @DocumentSha256, @CreatedByUserID, SYSUTCDATETIME(), SYSUTCDATETIME());";

                    using (SqlCommand command = new SqlCommand(sql, connection, transaction))
                    {
                        command.Parameters.Add("@DispatchNo", SqlDbType.NVarChar, 30).Value = dispatchNo;
                        command.Parameters.Add("@DispatchDate", SqlDbType.Date).Value = record.DispatchDate.Date;
                        AddNullableDate(command, "@LetterDate", record.LetterDate);
                        AddRequiredText(command, "@RecipientOffice", 200, record.RecipientOffice);
                        AddNullableText(command, "@RecipientAddress", 350, record.RecipientAddress);
                        AddNullableText(command, "@RecipientContact", 100, record.RecipientContact);
                        AddRequiredText(command, "@Subject", 300, record.Subject);
                        AddNullableText(command, "@ReferenceNo", 100, record.ReferenceNo);
                        AddNullableText(command, "@Description", -1, record.Description);
                        AddRequiredText(command, "@Category", 50, record.Category);
                        AddRequiredText(command, "@Priority", 20, record.Priority);
                        AddNullableText(command, "@DispatchMode", 50, record.DispatchMode);
                        AddNullableText(command, "@TrackingNo", 100, record.TrackingNo);
                        AddNullableText(command, "@SignedBy", 150, record.SignedBy);
                        AddNullableText(command, "@PreparedBy", 150, record.PreparedBy);
                        AddRequiredText(command, "@Status", 30, record.Status);
                        AddNullableText(command, "@Remarks", 500, record.Remarks);
                        AddDocumentParameters(command, record.Document);
                        AddNullableInt(command, "@CreatedByUserID", record.CreatedByUserID);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return dispatchNo;
                }
            }
        }

        internal static DataTable GetDiaryRecords(string search, string status, DateTime? fromDate, DateTime? toDate)
        {
            const string sql = @"
SELECT DiaryID, DiaryNo, ReceivedDate, LetterDate, SenderOffice,
       SenderReferenceNo, Subject, Category, Priority, AssignedTo,
       ActionDueDate, Status, OriginalFileName, FileSizeBytes,
       (SELECT COUNT(*) FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Incoming' AND x.RecordID=OfficeDaakDiary.DiaryID) AS DocumentCount,
       CASE WHEN EXISTS(SELECT 1 FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Incoming' AND x.RecordID=OfficeDaakDiary.DiaryID) OR DocumentData IS NOT NULL OR StoredFileName IS NOT NULL THEN 1 ELSE 0 END AS HasDocument
FROM dbo.OfficeDaakDiary
WHERE (@Search = N''
       OR DiaryNo LIKE @Pattern
       OR SenderOffice LIKE @Pattern
       OR ISNULL(SenderReferenceNo, N'') LIKE @Pattern
       OR Subject LIKE @Pattern)
  AND (@Status = N'' OR Status = @Status)
  AND (@FromDate IS NULL OR ReceivedDate >= @FromDate)
  AND (@ToDate IS NULL OR ReceivedDate <= @ToDate)
ORDER BY ReceivedDate DESC, DiaryID DESC;";

            return GetRegisterData(sql, search, status, fromDate, toDate);
        }

        internal static DataTable GetDispatchRecords(string search, string status, DateTime? fromDate, DateTime? toDate)
        {
            const string sql = @"
SELECT DispatchID, DispatchNo, DispatchDate, LetterDate, RecipientOffice,
       ReferenceNo, Subject, Category, Priority, DispatchMode, TrackingNo,
       Status, OriginalFileName, FileSizeBytes,
       (SELECT COUNT(*) FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Outgoing' AND x.RecordID=OfficeDaakDispatch.DispatchID) AS DocumentCount,
       CASE WHEN EXISTS(SELECT 1 FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Outgoing' AND x.RecordID=OfficeDaakDispatch.DispatchID) OR DocumentData IS NOT NULL OR StoredFileName IS NOT NULL THEN 1 ELSE 0 END AS HasDocument
FROM dbo.OfficeDaakDispatch
WHERE (@Search = N''
       OR DispatchNo LIKE @Pattern
       OR RecipientOffice LIKE @Pattern
       OR ISNULL(ReferenceNo, N'') LIKE @Pattern
       OR ISNULL(TrackingNo, N'') LIKE @Pattern
       OR Subject LIKE @Pattern)
  AND (@Status = N'' OR Status = @Status)
  AND (@FromDate IS NULL OR DispatchDate >= @FromDate)
  AND (@ToDate IS NULL OR DispatchDate <= @ToDate)
ORDER BY DispatchDate DESC, DispatchID DESC;";

            return GetRegisterData(sql, search, status, fromDate, toDate);
        }

        internal static DaakRegisterStats GetDiaryStats()
        {
            const string sql = @"
SELECT COUNT(*) AS TotalRecords,
       SUM(CASE WHEN ReceivedDate = CONVERT(DATE, GETDATE()) THEN 1 ELSE 0 END) AS TodayRecords,
       SUM(CASE WHEN Status IN (N'Received', N'Under Review', N'Forwarded') THEN 1 ELSE 0 END) AS ActiveRecords,
       (SELECT COUNT(*) FROM dbo.OfficeDaakDocuments WHERE RecordType=N'Incoming') AS FiledCopies
FROM dbo.OfficeDaakDiary;";
            return GetStats(sql);
        }

        internal static DaakRegisterStats GetDispatchStats()
        {
            const string sql = @"
SELECT COUNT(*) AS TotalRecords,
       SUM(CASE WHEN DispatchDate = CONVERT(DATE, GETDATE()) THEN 1 ELSE 0 END) AS TodayRecords,
       SUM(CASE WHEN Status IN (N'Prepared', N'Dispatched') THEN 1 ELSE 0 END) AS ActiveRecords,
       (SELECT COUNT(*) FROM dbo.OfficeDaakDocuments WHERE RecordType=N'Outgoing') AS FiledCopies
FROM dbo.OfficeDaakDispatch;";
            return GetStats(sql);
        }

        internal static DaakDocument GetDiaryDocument(int diaryId)
        {
            return GetDocument(
                "SELECT DocumentData, DocumentSha256, StoredFileName, OriginalFileName, FileContentType, FileSizeBytes FROM dbo.OfficeDaakDiary WHERE DiaryID = @RecordID;",
                diaryId,
                "Incoming");
        }

        internal static DaakDocument GetDispatchDocument(int dispatchId)
        {
            return GetDocument(
                "SELECT DocumentData, DocumentSha256, StoredFileName, OriginalFileName, FileContentType, FileSizeBytes FROM dbo.OfficeDaakDispatch WHERE DispatchID = @RecordID;",
                dispatchId,
                "Outgoing");
        }

        internal static DaakDocument SaveUploadedDocument(FileUpload upload, string direction)
        {
            if (upload == null || !upload.HasFile)
            {
                return null;
            }

            ValidateDirection(direction);

            string originalFileName = Path.GetFileName(upload.FileName ?? string.Empty);
            string extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            if (!ImageUploadProcessor.IsCompressibleImageExtension(extension) && extension != ".pdf")
            {
                throw new InvalidOperationException("Upload a JPG, JPEG, PNG, GIF, BMP, or PDF document only.");
            }

            originalFileName = LimitFileName(originalFileName, extension, 260);

            if (extension != ".pdf")
            {
                ProcessedImage image = ImageUploadProcessor.ReadAndOptimize(upload, 2000, 2600);
                return new DaakDocument
                {
                    OriginalFileName = image.FileName,
                    ContentType = image.ContentType,
                    SizeBytes = image.Data.LongLength,
                    Data = image.Data,
                    Sha256 = ComputeSha256(image.Data)
                };
            }

            if (upload.PostedFile.ContentLength <= 0 || upload.PostedFile.ContentLength > MaximumDocumentBytes)
                throw new InvalidOperationException("A PDF scanned copy must be no larger than 5 MB.");

            byte[] documentData;
            using (BinaryReader reader = new BinaryReader(upload.PostedFile.InputStream))
            {
                documentData = reader.ReadBytes(upload.PostedFile.ContentLength);
            }

            if (documentData.Length != upload.PostedFile.ContentLength)
                throw new InvalidOperationException("The complete scanned copy could not be read.");

            if (!HasValidFileSignature(documentData, extension))
                throw new InvalidOperationException("The uploaded file contents do not match its PDF extension.");

            return new DaakDocument
            {
                OriginalFileName = originalFileName,
                ContentType = GetSafeContentType(upload.PostedFile.ContentType, extension),
                SizeBytes = documentData.LongLength,
                Data = documentData,
                Sha256 = ComputeSha256(documentData)
            };
        }

        internal static void DeleteUploadedDocument(DaakDocument document)
        {
            // Uploads remain in memory until the surrounding database insert
            // commits. There is no temporary filesystem artifact to delete.
        }

        private static void MigrateLegacyDocuments()
        {
            MigrateLegacyDocuments(
                "dbo.OfficeDaakDiary",
                "DiaryID",
                "Incoming");
            MigrateLegacyDocuments(
                "dbo.OfficeDaakDispatch",
                "DispatchID",
                "Outgoing");
        }

        private static void MigrateLegacyDocuments(
            string tableName,
            string keyColumn,
            string direction)
        {
            string selectSql = "SELECT " + keyColumn +
                               " AS RecordID, StoredFileName, OriginalFileName FROM " + tableName +
                               " WHERE DocumentData IS NULL AND StoredFileName IS NOT NULL;";
            DataTable records = new DataTable();

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlDataAdapter adapter = new SqlDataAdapter(selectSql, connection))
                {
                    adapter.Fill(records);
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Trace.TraceWarning(
                    "[OfficeDaakService] Legacy {0} scan skipped: {1}", direction, ex.Message);
                return;
            }

            foreach (DataRow row in records.Rows)
            {
                try
                {
                    string storedFileName = Path.GetFileName(Convert.ToString(row["StoredFileName"]));
                    if (string.IsNullOrWhiteSpace(storedFileName))
                        continue;

                    string fullPath = Path.Combine(GetStorageDirectory(direction), storedFileName);
                    if (!File.Exists(fullPath))
                        continue;

                    FileInfo file = new FileInfo(fullPath);
                    if (file.Length <= 0)
                        continue;

                    string extension = Path.GetExtension(storedFileName).ToLowerInvariant();
                    byte[] data;
                    string contentType;
                    string originalFileName = Path.GetFileName(Convert.ToString(row["OriginalFileName"]));
                    if (string.IsNullOrWhiteSpace(originalFileName))
                        originalFileName = storedFileName;

                    if (ImageUploadProcessor.IsCompressibleImageExtension(extension))
                    {
                        if (file.Length > ImageUploadProcessor.MaximumSourceImageBytes)
                            continue;
                        ProcessedImage image = ImageUploadProcessor.OptimizeBytes(
                            File.ReadAllBytes(fullPath),
                            originalFileName,
                            2000,
                            2600);
                        data = image.Data;
                        contentType = image.ContentType;
                        originalFileName = image.FileName;
                    }
                    else
                    {
                        if (file.Length > MaximumDocumentBytes)
                            continue;
                        data = File.ReadAllBytes(fullPath);
                        if (!HasValidFileSignature(data, extension))
                            continue;
                        if (extension == ".webp" && data.Length > ImageUploadProcessor.MaximumStoredImageBytes)
                            continue;
                        contentType = GetSafeContentType(null, extension);
                    }

                    string updateSql = "UPDATE " + tableName +
                                       " SET DocumentData=@DocumentData, DocumentSha256=@DocumentSha256," +
                                       " FileSizeBytes=@FileSizeBytes," +
                                       " FileContentType=@FileContentType, OriginalFileName=@OriginalFileName" +
                                       " WHERE " + keyColumn + "=@RecordID AND DocumentData IS NULL;";

                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    using (SqlCommand command = new SqlCommand(updateSql, connection))
                    {
                        command.Parameters.Add("@DocumentData", SqlDbType.VarBinary, -1).Value = data;
                        command.Parameters.Add("@DocumentSha256", SqlDbType.Char, 64).Value = ComputeSha256(data);
                        command.Parameters.Add("@FileSizeBytes", SqlDbType.BigInt).Value = data.LongLength;
                        command.Parameters.Add("@FileContentType", SqlDbType.NVarChar, 100).Value = contentType;
                        command.Parameters.Add("@OriginalFileName", SqlDbType.NVarChar, 260).Value = originalFileName;
                        command.Parameters.Add("@RecordID", SqlDbType.Int).Value = Convert.ToInt32(row["RecordID"]);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning(
                        "[OfficeDaakService] Legacy {0} document {1} was not migrated: {2}",
                        direction,
                        row["RecordID"],
                        ex.Message);
                }
            }
        }

        private static string GetNextRegisterNumber(
            SqlConnection connection,
            SqlTransaction transaction,
            string registerType,
            string prefix,
            DateTime registerDate)
        {
            const string selectSql = @"
SELECT LastNumber
FROM dbo.OfficeDaakCounters WITH (UPDLOCK, HOLDLOCK)
WHERE RegisterType = @RegisterType AND CounterYear = @CounterYear;";

            int year = registerDate.Year;
            int nextNumber;
            using (SqlCommand selectCommand = new SqlCommand(selectSql, connection, transaction))
            {
                selectCommand.Parameters.Add("@RegisterType", SqlDbType.NVarChar, 20).Value = registerType;
                selectCommand.Parameters.Add("@CounterYear", SqlDbType.SmallInt).Value = year;
                object current = selectCommand.ExecuteScalar();
                nextNumber = current == null || current == DBNull.Value ? 1 : Convert.ToInt32(current) + 1;
            }

            if (nextNumber == 1)
            {
                const string insertSql = @"
INSERT INTO dbo.OfficeDaakCounters (RegisterType, CounterYear, LastNumber)
VALUES (@RegisterType, @CounterYear, @LastNumber);";
                using (SqlCommand insertCommand = new SqlCommand(insertSql, connection, transaction))
                {
                    insertCommand.Parameters.Add("@RegisterType", SqlDbType.NVarChar, 20).Value = registerType;
                    insertCommand.Parameters.Add("@CounterYear", SqlDbType.SmallInt).Value = year;
                    insertCommand.Parameters.Add("@LastNumber", SqlDbType.Int).Value = nextNumber;
                    insertCommand.ExecuteNonQuery();
                }
            }
            else
            {
                const string updateSql = @"
UPDATE dbo.OfficeDaakCounters
SET LastNumber = @LastNumber
WHERE RegisterType = @RegisterType AND CounterYear = @CounterYear;";
                using (SqlCommand updateCommand = new SqlCommand(updateSql, connection, transaction))
                {
                    updateCommand.Parameters.Add("@LastNumber", SqlDbType.Int).Value = nextNumber;
                    updateCommand.Parameters.Add("@RegisterType", SqlDbType.NVarChar, 20).Value = registerType;
                    updateCommand.Parameters.Add("@CounterYear", SqlDbType.SmallInt).Value = year;
                    updateCommand.ExecuteNonQuery();
                }
            }

            return prefix + "-" + year.ToString(CultureInfo.InvariantCulture) + "-" +
                   nextNumber.ToString("D5", CultureInfo.InvariantCulture);
        }

        private static DataTable GetRegisterData(
            string sql,
            string search,
            string status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                string cleanSearch = (search ?? string.Empty).Trim();
                command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = cleanSearch;
                command.Parameters.Add("@Pattern", SqlDbType.NVarChar, 204).Value = "%" + cleanSearch + "%";
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = (status ?? string.Empty).Trim();
                command.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.HasValue
                    ? (object)fromDate.Value.Date
                    : DBNull.Value;
                command.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.HasValue
                    ? (object)toDate.Value.Date
                    : DBNull.Value;
                adapter.Fill(table);
            }

            return table;
        }

        private static DaakRegisterStats GetStats(string sql)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return new DaakRegisterStats();
                    }

                    return new DaakRegisterStats
                    {
                        TotalRecords = GetInt32(reader, "TotalRecords"),
                        TodayRecords = GetInt32(reader, "TodayRecords"),
                        ActiveRecords = GetInt32(reader, "ActiveRecords"),
                        FiledCopies = GetInt32(reader, "FiledCopies")
                    };
                }
            }
        }

        private static DaakDocument GetDocument(string sql, int recordId, string direction)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@RecordID", SqlDbType.Int).Value = recordId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    byte[] data = reader["DocumentData"] as byte[];
                    string storedFileName = Path.GetFileName(Convert.ToString(reader["StoredFileName"]));
                    if ((data == null || data.Length == 0) && !string.IsNullOrWhiteSpace(storedFileName))
                    {
                        string legacyPath = Path.Combine(GetStorageDirectory(direction), storedFileName);
                        if (File.Exists(legacyPath))
                            data = File.ReadAllBytes(legacyPath);
                    }

                    if (data == null || data.Length == 0)
                        return null;

                    return new DaakDocument
                    {
                        StoredFileName = storedFileName,
                        OriginalFileName = Path.GetFileName(Convert.ToString(reader["OriginalFileName"])),
                        ContentType = Convert.ToString(reader["FileContentType"]),
                        SizeBytes = data.LongLength,
                        Data = data,
                        Sha256 = Convert.ToString(reader["DocumentSha256"])
                    };
                }
            }
        }

        private static void ValidateDirection(string direction)
        {
            if (!string.Equals(direction, "Incoming", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(direction, "Outgoing", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The Daak storage direction is invalid.");
            }
        }

        private static string GetStorageDirectory(string direction)
        {
            ValidateDirection(direction);

            string path = HostingEnvironment.MapPath("~/App_Data/OfficeDaak/" + direction);
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("The Office Daak storage folder could not be resolved.");
            }

            return path;
        }

        private static string GetSafeContentType(string postedContentType, string extension)
        {
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".webp":
                    return "image/webp";
                case ".pdf":
                    return "application/pdf";
                default:
                    return string.IsNullOrWhiteSpace(postedContentType)
                        ? "application/octet-stream"
                        : postedContentType;
            }
        }

        private static string LimitFileName(string fileName, string extension, int maximumLength)
        {
            if (fileName.Length <= maximumLength)
            {
                return fileName;
            }

            int stemLength = Math.Max(1, maximumLength - extension.Length);
            string stem = Path.GetFileNameWithoutExtension(fileName);
            if (stem.Length > stemLength)
            {
                stem = stem.Substring(0, stemLength);
            }

            return stem + extension;
        }

        private static bool HasValidFileSignature(byte[] data, string extension)
        {
            byte[] header = new byte[12];
            int bytesRead = data == null ? 0 : Math.Min(data.Length, header.Length);
            if (bytesRead > 0)
                Buffer.BlockCopy(data, 0, header, 0, bytesRead);

            if (extension == ".jpg" || extension == ".jpeg")
            {
                return bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            }

            if (extension == ".png")
            {
                byte[] png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                return StartsWith(header, bytesRead, png);
            }

            if (extension == ".pdf")
            {
                byte[] pdf = { 0x25, 0x50, 0x44, 0x46, 0x2D };
                return StartsWith(header, bytesRead, pdf);
            }

            if (extension == ".webp")
            {
                return bytesRead >= 12 &&
                       header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                       header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
            }

            return false;
        }

        private static string ComputeSha256(byte[] data)
        {
            using (SHA256 algorithm = SHA256.Create())
            {
                byte[] hash = algorithm.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        private static bool StartsWith(byte[] source, int sourceLength, byte[] expected)
        {
            if (sourceLength < expected.Length)
            {
                return false;
            }

            for (int index = 0; index < expected.Length; index++)
            {
                if (source[index] != expected[index])
                {
                    return false;
                }
            }

            return true;
        }

        private static void AddDocumentParameters(SqlCommand command, DaakDocument document)
        {
            // StoredFileName is retained only for one-time migration of V9/V10
            // records. New documents are stored in DocumentData inside SQL.
            AddNullableText(command, "@StoredFileName", 260, null);
            AddNullableText(command, "@OriginalFileName", 260, document == null ? null : document.OriginalFileName);
            AddNullableText(command, "@FileContentType", 100, document == null ? null : document.ContentType);
            SqlParameter sizeParameter = command.Parameters.Add("@FileSizeBytes", SqlDbType.BigInt);
            sizeParameter.Value = document == null ? (object)DBNull.Value : document.SizeBytes;
            SqlParameter dataParameter = command.Parameters.Add("@DocumentData", SqlDbType.VarBinary, -1);
            dataParameter.Value = document == null ? (object)DBNull.Value : document.Data;
            SqlParameter hashParameter = command.Parameters.Add("@DocumentSha256", SqlDbType.Char, 64);
            hashParameter.Value = document == null || string.IsNullOrWhiteSpace(document.Sha256)
                ? (object)DBNull.Value
                : document.Sha256;
        }

        private static void AddRequiredText(SqlCommand command, string name, int size, string value)
        {
            command.Parameters.Add(name, SqlDbType.NVarChar, size).Value = (value ?? string.Empty).Trim();
        }

        private static void AddNullableText(SqlCommand command, string name, int size, string value)
        {
            SqlParameter parameter = command.Parameters.Add(name, SqlDbType.NVarChar, size);
            parameter.Value = string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private static void AddNullableDate(SqlCommand command, string name, DateTime? value)
        {
            command.Parameters.Add(name, SqlDbType.Date).Value = value.HasValue
                ? (object)value.Value.Date
                : DBNull.Value;
        }

        private static void AddNullableInt(SqlCommand command, string name, int? value)
        {
            command.Parameters.Add(name, SqlDbType.Int).Value = value.HasValue
                ? (object)value.Value
                : DBNull.Value;
        }

        private static int GetInt32(IDataRecord record, string columnName)
        {
            object value = record[columnName];
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }
    }

    internal sealed class DaakDiaryRecord
    {
        internal DateTime ReceivedDate { get; set; }
        internal DateTime? LetterDate { get; set; }
        internal string SenderOffice { get; set; }
        internal string SenderReferenceNo { get; set; }
        internal string Subject { get; set; }
        internal string Description { get; set; }
        internal string Category { get; set; }
        internal string Priority { get; set; }
        internal string DeliveryMode { get; set; }
        internal string AssignedTo { get; set; }
        internal DateTime? ActionDueDate { get; set; }
        internal string Status { get; set; }
        internal string Remarks { get; set; }
        internal DaakDocument Document { get; set; }
        internal int? CreatedByUserID { get; set; }
    }

    internal sealed class DaakDispatchRecord
    {
        internal DateTime DispatchDate { get; set; }
        internal DateTime? LetterDate { get; set; }
        internal string RecipientOffice { get; set; }
        internal string RecipientAddress { get; set; }
        internal string RecipientContact { get; set; }
        internal string Subject { get; set; }
        internal string ReferenceNo { get; set; }
        internal string Description { get; set; }
        internal string Category { get; set; }
        internal string Priority { get; set; }
        internal string DispatchMode { get; set; }
        internal string TrackingNo { get; set; }
        internal string SignedBy { get; set; }
        internal string PreparedBy { get; set; }
        internal string Status { get; set; }
        internal string Remarks { get; set; }
        internal DaakDocument Document { get; set; }
        internal int? CreatedByUserID { get; set; }
    }

    internal sealed class DaakDocument
    {
        internal string StoredFileName { get; set; }
        internal string OriginalFileName { get; set; }
        internal string ContentType { get; set; }
        internal long SizeBytes { get; set; }
        internal byte[] Data { get; set; }
        internal string Sha256 { get; set; }
    }

    internal sealed class DaakRegisterStats
    {
        internal int TotalRecords { get; set; }
        internal int TodayRecords { get; set; }
        internal int ActiveRecords { get; set; }
        internal int FiledCopies { get; set; }
    }
}
