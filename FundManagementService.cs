using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    internal static class FundManagementService
    {
        private const string FtfChargePredicate = @"
(
    UPPER(REPLACE(REPLACE(ISNULL(rate.Description, N''), N'-', N''), N' ', N'')) LIKE N'%FTF%'
    OR UPPER(ISNULL(rate.Description, N'')) LIKE N'%FAROGH%TALEEM%'
)";

        private static string ConnectionString
        {
            get
            {
                ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["SchoolDB"];
                if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                    throw new ConfigurationErrorsException("The SchoolDB connection string is missing from Web.config.");
                return setting.ConnectionString;
            }
        }

        internal static void EnsureSchema()
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.FundTransactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FundTransactions
    (
        FundTransactionID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_FundTransactions PRIMARY KEY,
        TransactionNumber NVARCHAR(50) NOT NULL,
        FundType NVARCHAR(10) NOT NULL,
        TransactionType NVARCHAR(20) NOT NULL,
        TransactionDate DATE NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        SourceOrPayee NVARCHAR(200) NULL,
        ReferenceNo NVARCHAR(100) NULL,
        ChequeNo NVARCHAR(100) NULL,
        WorkType NVARCHAR(100) NULL,
        Purpose NVARCHAR(500) NULL,
        Remarks NVARCHAR(500) NULL,
        CreatedByUserID INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_FundTransactions_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_FundTransactions_FundType
            CHECK (FundType IN (N'NSB', N'FTF', N'SMC')),
        CONSTRAINT CK_FundTransactions_TransactionType
            CHECK (TransactionType IN (N'Receipt', N'Deposit', N'Utilization')),
        CONSTRAINT CK_FundTransactions_Amount CHECK (Amount > 0)
    );
END;

IF OBJECT_ID(N'dbo.FundTransactionDocuments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FundTransactionDocuments
    (
        FundDocumentID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_FundTransactionDocuments PRIMARY KEY,
        FundTransactionID INT NOT NULL,
        DocumentType NVARCHAR(20) NOT NULL,
        OriginalFileName NVARCHAR(260) NOT NULL,
        ContentType NVARCHAR(100) NOT NULL,
        FileSizeBytes INT NOT NULL,
        DocumentData VARBINARY(MAX) NOT NULL,
        UploadedAtUtc DATETIME2(0) NOT NULL
            CONSTRAINT DF_FundTransactionDocuments_UploadedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_FundTransactionDocuments_Transaction
            FOREIGN KEY (FundTransactionID)
            REFERENCES dbo.FundTransactions (FundTransactionID)
            ON DELETE CASCADE,
        CONSTRAINT CK_FundTransactionDocuments_Type
            CHECK (DocumentType IN (N'Receipt', N'Cheque')),
        CONSTRAINT CK_FundTransactionDocuments_Size
            CHECK (FileSizeBytes > 0 AND FileSizeBytes <= 1048576)
    );
END;

IF OBJECT_ID(N'dbo.FundBankAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FundBankAccounts
    (
        FundType NVARCHAR(10) NOT NULL CONSTRAINT PK_FundBankAccounts PRIMARY KEY,
        BankName NVARCHAR(150) NOT NULL,
        BranchCode NVARCHAR(50) NOT NULL,
        BranchAddress NVARCHAR(300) NOT NULL,
        AccountIBAN NVARCHAR(50) NOT NULL,
        UpdatedByUserID INT NULL,
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_FundBankAccounts_UpdatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_FundBankAccounts_FundType CHECK (FundType IN (N'NSB', N'FTF', N'SMC'))
    );
END;

IF EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_FundTransactions_FundType' AND definition NOT LIKE N'%SMC%')
BEGIN
    ALTER TABLE dbo.FundTransactions DROP CONSTRAINT CK_FundTransactions_FundType;
    ALTER TABLE dbo.FundTransactions ADD CONSTRAINT CK_FundTransactions_FundType CHECK(FundType IN(N'NSB',N'FTF',N'SMC'));
END;

IF EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_FundBankAccounts_FundType' AND definition NOT LIKE N'%SMC%')
BEGIN
    ALTER TABLE dbo.FundBankAccounts DROP CONSTRAINT CK_FundBankAccounts_FundType;
    ALTER TABLE dbo.FundBankAccounts ADD CONSTRAINT CK_FundBankAccounts_FundType CHECK(FundType IN(N'NSB',N'FTF',N'SMC'));
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FundTransactions')
      AND name = N'UX_FundTransactions_TransactionNumber'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_FundTransactions_TransactionNumber
        ON dbo.FundTransactions (TransactionNumber);

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FundTransactions')
      AND name = N'IX_FundTransactions_FundDate'
)
    CREATE NONCLUSTERED INDEX IX_FundTransactions_FundDate
        ON dbo.FundTransactions (FundType, TransactionDate DESC)
        INCLUDE (TransactionType, Amount, ChequeNo, WorkType);

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FundTransactionDocuments')
      AND name = N'IX_FundTransactionDocuments_Transaction'
)
    CREATE NONCLUSTERED INDEX IX_FundTransactionDocuments_Transaction
        ON dbo.FundTransactionDocuments (FundTransactionID, DocumentType);";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = 60;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        internal static FundBankAccountDetails GetBankAccount(string fundType)
        {
            const string sql = @"
SELECT FundType, BankName, BranchCode, BranchAddress, AccountIBAN, UpdatedAtUtc
FROM dbo.FundBankAccounts
WHERE FundType=@FundType;";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@FundType", SqlDbType.NVarChar, 10).Value = NormalizeFundType(fundType);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                        return new FundBankAccountDetails { FundType = NormalizeFundType(fundType) };
                    return new FundBankAccountDetails
                    {
                        FundType = Convert.ToString(reader["FundType"], CultureInfo.InvariantCulture),
                        BankName = Convert.ToString(reader["BankName"], CultureInfo.InvariantCulture),
                        BranchCode = Convert.ToString(reader["BranchCode"], CultureInfo.InvariantCulture),
                        BranchAddress = Convert.ToString(reader["BranchAddress"], CultureInfo.InvariantCulture),
                        AccountIBAN = Convert.ToString(reader["AccountIBAN"], CultureInfo.InvariantCulture),
                        UpdatedAtUtc = Convert.ToDateTime(reader["UpdatedAtUtc"], CultureInfo.InvariantCulture)
                    };
                }
            }
        }

        internal static void SaveBankAccount(FundBankAccountDetails details)
        {
            if (details == null)
                throw new InvalidOperationException("Enter the bank account details.");
            details.FundType = NormalizeFundType(details.FundType);
            details.BankName = RequireText(details.BankName, "bank name", 150);
            details.BranchCode = RequireText(details.BranchCode, "branch code", 50);
            details.BranchAddress = RequireText(details.BranchAddress, "branch address", 300);
            details.AccountIBAN = RequireText(details.AccountIBAN, "account number / IBAN", 50).ToUpperInvariant().Replace(" ", string.Empty);

            const string sql = @"
UPDATE dbo.FundBankAccounts
SET BankName=@BankName, BranchCode=@BranchCode, BranchAddress=@BranchAddress,
    AccountIBAN=@AccountIBAN, UpdatedByUserID=@UpdatedByUserID, UpdatedAtUtc=SYSUTCDATETIME()
WHERE FundType=@FundType;
IF @@ROWCOUNT=0
    INSERT dbo.FundBankAccounts
        (FundType, BankName, BranchCode, BranchAddress, AccountIBAN, UpdatedByUserID, UpdatedAtUtc)
    VALUES
        (@FundType, @BankName, @BranchCode, @BranchAddress, @AccountIBAN, @UpdatedByUserID, SYSUTCDATETIME());";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                AddText(command, "@FundType", 10, details.FundType);
                AddText(command, "@BankName", 150, details.BankName);
                AddText(command, "@BranchCode", 50, details.BranchCode);
                AddText(command, "@BranchAddress", 300, details.BranchAddress);
                AddText(command, "@AccountIBAN", 50, details.AccountIBAN);
                AddNullableInt(command, "@UpdatedByUserID", details.UpdatedByUserID);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        internal static string SaveTransaction(FundTransactionEntry entry, IList<FundDocumentUpload> documents)
        {
            ValidateEntry(entry);
            documents = documents ?? new List<FundDocumentUpload>();
            if (documents.Count > 12)
                throw new InvalidOperationException("A maximum of 12 evidence images can be saved with one transaction.");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    ValidateAvailableBalance(connection, transaction, entry);

                    const string insertSql = @"
INSERT INTO dbo.FundTransactions
       (TransactionNumber, FundType, TransactionType, TransactionDate, Amount,
        SourceOrPayee, ReferenceNo, ChequeNo, WorkType, Purpose, Remarks,
        CreatedByUserID, CreatedAtUtc)
VALUES (@TemporaryNumber, @FundType, @TransactionType, @TransactionDate, @Amount,
        @SourceOrPayee, @ReferenceNo, @ChequeNo, @WorkType, @Purpose, @Remarks,
        @CreatedByUserID, SYSUTCDATETIME());
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int transactionId;
                    using (SqlCommand command = new SqlCommand(insertSql, connection, transaction))
                    {
                        AddText(command, "@TemporaryNumber", 50, "PENDING-" + Guid.NewGuid().ToString("N"));
                        AddText(command, "@FundType", 10, entry.FundType);
                        AddText(command, "@TransactionType", 20, entry.TransactionType);
                        command.Parameters.Add("@TransactionDate", SqlDbType.Date).Value = entry.TransactionDate.Date;
                        SqlParameter amount = command.Parameters.Add("@Amount", SqlDbType.Decimal);
                        amount.Precision = 18;
                        amount.Scale = 2;
                        amount.Value = decimal.Round(entry.Amount, 2);
                        AddNullableText(command, "@SourceOrPayee", 200, entry.SourceOrPayee);
                        AddNullableText(command, "@ReferenceNo", 100, entry.ReferenceNo);
                        AddNullableText(command, "@ChequeNo", 100, entry.ChequeNo);
                        AddNullableText(command, "@WorkType", 100, entry.WorkType);
                        AddNullableText(command, "@Purpose", 500, entry.Purpose);
                        AddNullableText(command, "@Remarks", 500, entry.Remarks);
                        SqlParameter createdBy = command.Parameters.Add("@CreatedByUserID", SqlDbType.Int);
                        createdBy.Value = entry.CreatedByUserID.HasValue
                            ? (object)entry.CreatedByUserID.Value
                            : DBNull.Value;
                        transactionId = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                    }

                    string number = BuildTransactionNumber(entry, transactionId);
                    using (SqlCommand update = new SqlCommand(
                        "UPDATE dbo.FundTransactions SET TransactionNumber=@Number WHERE FundTransactionID=@ID;",
                        connection,
                        transaction))
                    {
                        update.Parameters.Add("@Number", SqlDbType.NVarChar, 50).Value = number;
                        update.Parameters.Add("@ID", SqlDbType.Int).Value = transactionId;
                        update.ExecuteNonQuery();
                    }

                    foreach (FundDocumentUpload document in documents)
                    {
                        if (document == null || document.Data == null || document.Data.Length == 0)
                            continue;
                        if (document.Data.Length > ImageUploadProcessor.MaximumStoredImageBytes)
                            throw new InvalidOperationException("Every stored evidence image must be no larger than 1 MB.");

                        const string documentSql = @"
INSERT INTO dbo.FundTransactionDocuments
       (FundTransactionID, DocumentType, OriginalFileName, ContentType,
        FileSizeBytes, DocumentData, UploadedAtUtc)
VALUES (@FundTransactionID, @DocumentType, @OriginalFileName, @ContentType,
        @FileSizeBytes, @DocumentData, SYSUTCDATETIME());";
                        using (SqlCommand command = new SqlCommand(documentSql, connection, transaction))
                        {
                            command.Parameters.Add("@FundTransactionID", SqlDbType.Int).Value = transactionId;
                            AddText(command, "@DocumentType", 20, document.DocumentType);
                            AddText(command, "@OriginalFileName", 260, document.FileName);
                            AddText(command, "@ContentType", 100, document.ContentType);
                            command.Parameters.Add("@FileSizeBytes", SqlDbType.Int).Value = document.Data.Length;
                            command.Parameters.Add("@DocumentData", SqlDbType.VarBinary, -1).Value = document.Data;
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return number;
                }
            }
        }

        internal static IList<FundDocumentUpload> ProcessImages(
            FileUpload upload,
            string documentType,
            int maximumFiles)
        {
            var results = new List<FundDocumentUpload>();
            if (upload == null || !upload.HasFiles)
                return results;

            for (int index = 0; index < upload.PostedFiles.Count; index++)
            {
                HttpPostedFile file = upload.PostedFiles[index];
                if (file == null || file.ContentLength <= 0)
                    continue;
                if (results.Count >= maximumFiles)
                    throw new InvalidOperationException("Select no more than " + maximumFiles + " image(s) in this upload field.");

                ProcessedImage image = ImageUploadProcessor.ReadAndOptimize(file, 2000, 2600);
                results.Add(new FundDocumentUpload
                {
                    DocumentType = documentType,
                    FileName = image.FileName,
                    ContentType = image.ContentType,
                    Data = image.Data
                });
            }

            return results;
        }

        internal static FundAccountSummary GetSummary()
        {
            string sql = @"
SELECT
    ISNULL(SUM(CASE WHEN FundType=N'NSB' AND TransactionType=N'Receipt' THEN Amount ELSE 0 END), 0) AS NSBReceived,
    ISNULL(SUM(CASE WHEN FundType=N'NSB' AND TransactionType=N'Utilization' THEN Amount ELSE 0 END), 0) AS NSBUtilized,
    ISNULL(SUM(CASE WHEN FundType=N'FTF' AND TransactionType=N'Deposit' THEN Amount ELSE 0 END), 0) AS FTFDeposited,
    ISNULL(SUM(CASE WHEN FundType=N'FTF' AND TransactionType=N'Utilization' THEN Amount ELSE 0 END), 0) AS FTFUtilized
FROM dbo.FundTransactions;

SELECT ISNULL(SUM(paid.PaidAmount), 0)
FROM dbo.FundsCollection paid
INNER JOIN dbo.FeeSchoolCharges rate ON rate.FeeID = paid.FeeID
WHERE " + FtfChargePredicate + ";";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var summary = new FundAccountSummary();
                    if (reader.Read())
                    {
                        summary.NsbReceived = GetDecimal(reader, "NSBReceived");
                        summary.NsbUtilized = GetDecimal(reader, "NSBUtilized");
                        summary.FtfDeposited = GetDecimal(reader, "FTFDeposited");
                        summary.FtfUtilized = GetDecimal(reader, "FTFUtilized");
                    }
                    if (reader.NextResult() && reader.Read())
                        summary.FtfCollected = reader.IsDBNull(0) ? 0m : reader.GetDecimal(0);
                    return summary;
                }
            }
        }

        internal static DataTable GetTransactions(string fundType, DateTime fromDate, DateTime toDate)
        {
            const string sql = @"
SELECT t.FundTransactionID,
       t.TransactionNumber,
       t.TransactionDate,
       t.TransactionType,
       t.Amount,
       ISNULL(t.SourceOrPayee, N'') AS SourceOrPayee,
       ISNULL(t.ReferenceNo, N'') AS ReferenceNo,
       ISNULL(t.ChequeNo, N'') AS ChequeNo,
       ISNULL(t.WorkType, N'') AS WorkType,
       ISNULL(t.Purpose, N'') AS Purpose,
       ISNULL(t.Remarks, N'') AS Remarks,
       (SELECT COUNT(*) FROM dbo.FundTransactionDocuments d
        WHERE d.FundTransactionID=t.FundTransactionID) AS DocumentCount
FROM dbo.FundTransactions t
WHERE t.FundType=@FundType
  AND t.TransactionDate >= @FromDate
  AND t.TransactionDate <= @ToDate
ORDER BY t.TransactionDate, t.FundTransactionID;";

            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@FundType", SqlDbType.NVarChar, 10).Value = NormalizeFundType(fundType);
                command.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;
                command.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;
                adapter.Fill(table);
            }
            return table;
        }

        internal static FundStatementTotals GetStatementTotals(string fundType, DateTime fromDate, DateTime toDate)
        {
            string normalizedFund = NormalizeFundType(fundType);
            string creditType = normalizedFund == "FTF" ? "Deposit" : "Receipt";
            const string sql = @"
SELECT
    ISNULL(SUM(CASE WHEN TransactionDate < @FromDate AND TransactionType=@CreditType THEN Amount
                    WHEN TransactionDate < @FromDate AND TransactionType=N'Utilization' THEN -Amount
                    ELSE 0 END), 0) AS OpeningBalance,
    ISNULL(SUM(CASE WHEN TransactionDate BETWEEN @FromDate AND @ToDate AND TransactionType=@CreditType THEN Amount ELSE 0 END), 0) AS PeriodCredits,
    ISNULL(SUM(CASE WHEN TransactionDate BETWEEN @FromDate AND @ToDate AND TransactionType=N'Utilization' THEN Amount ELSE 0 END), 0) AS PeriodDebits
FROM dbo.FundTransactions
WHERE FundType=@FundType;";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@FundType", SqlDbType.NVarChar, 10).Value = normalizedFund;
                command.Parameters.Add("@CreditType", SqlDbType.NVarChar, 20).Value = creditType;
                command.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;
                command.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                        return new FundStatementTotals();
                    return new FundStatementTotals
                    {
                        OpeningBalance = GetDecimal(reader, "OpeningBalance"),
                        PeriodCredits = GetDecimal(reader, "PeriodCredits"),
                        PeriodDebits = GetDecimal(reader, "PeriodDebits")
                    };
                }
            }
        }

        internal static DataTable GetFtfCollections(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
SELECT paid.DateOfDeposit,
       ISNULL(paid.VoucherNo, N'') AS VoucherNo,
       paid.StudentID,
       ISNULL(s.Name, N'') AS StudentName,
       ISNULL(s.Regno, N'') AS RegistrationNo,
       ISNULL(c.ClassName, N'Not assigned') AS ClassName,
       SUM(paid.PaidAmount) AS Amount
FROM dbo.FundsCollection paid
INNER JOIN dbo.FeeSchoolCharges rate ON rate.FeeID=paid.FeeID
INNER JOIN dbo.Students s ON s.StudentID=paid.StudentID
OUTER APPLY
(
    SELECT TOP (1) classes.ClassName
    FROM dbo.StudentClass sc
    INNER JOIN dbo.Classes classes ON classes.ClassID=sc.ClassID
    WHERE sc.StudentID=paid.StudentID
    ORDER BY sc.ClassID DESC
) c
WHERE " + FtfChargePredicate + @"
  AND paid.DateOfDeposit >= @FromDate
  AND paid.DateOfDeposit <= @ToDate
GROUP BY paid.DateOfDeposit, paid.VoucherNo, paid.StudentID, s.Name, s.Regno, c.ClassName
ORDER BY paid.DateOfDeposit, paid.VoucherNo, s.Name;";

            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;
                command.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;
                adapter.Fill(table);
            }
            return table;
        }

        internal static FundStatementTotals GetFtfWalletStatementTotals(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
SELECT
    ISNULL((SELECT SUM(paid.PaidAmount)
            FROM dbo.FundsCollection paid
            INNER JOIN dbo.FeeSchoolCharges rate ON rate.FeeID=paid.FeeID
            WHERE " + FtfChargePredicate + @" AND paid.DateOfDeposit < @FromDate), 0)
    -
    ISNULL((SELECT SUM(t.Amount) FROM dbo.FundTransactions t
            WHERE t.FundType=N'FTF' AND t.TransactionType=N'Deposit'
              AND t.TransactionDate < @FromDate), 0) AS OpeningBalance,
    ISNULL((SELECT SUM(paid.PaidAmount)
            FROM dbo.FundsCollection paid
            INNER JOIN dbo.FeeSchoolCharges rate ON rate.FeeID=paid.FeeID
            WHERE " + FtfChargePredicate + @"
              AND paid.DateOfDeposit BETWEEN @FromDate AND @ToDate), 0) AS PeriodCredits,
    ISNULL((SELECT SUM(t.Amount) FROM dbo.FundTransactions t
            WHERE t.FundType=N'FTF' AND t.TransactionType=N'Deposit'
              AND t.TransactionDate BETWEEN @FromDate AND @ToDate), 0) AS PeriodDebits;";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;
                command.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                        return new FundStatementTotals();
                    return new FundStatementTotals
                    {
                        OpeningBalance = GetDecimal(reader, "OpeningBalance"),
                        PeriodCredits = GetDecimal(reader, "PeriodCredits"),
                        PeriodDebits = GetDecimal(reader, "PeriodDebits")
                    };
                }
            }
        }

        internal static FtfCollectionPerformance GetFtfPerformance(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
WITH CurrentClass AS
(
    SELECT sc.StudentID, MAX(sc.ClassID) AS ClassID
    FROM dbo.StudentClass sc
    GROUP BY sc.StudentID
),
FtfRates AS
(
    SELECT rate.ClassID, SUM(rate.Rate) AS MonthlyRate
    FROM dbo.FeeSchoolCharges rate
    WHERE " + FtfChargePredicate + @"
    GROUP BY rate.ClassID
)
SELECT COUNT(*) AS TotalStudents,
       SUM(CASE WHEN r.MonthlyRate IS NOT NULL THEN 1 ELSE 0 END) AS StudentsWithRate,
       ISNULL(SUM(ISNULL(r.MonthlyRate, 0)), 0) AS MonthlyExpected
FROM CurrentClass cc
INNER JOIN dbo.Students s ON s.StudentID=cc.StudentID
LEFT JOIN FtfRates r ON r.ClassID=cc.ClassID
WHERE LTRIM(RTRIM(ISNULL(s.Isactive, 'Active'))) IN ('Active', 'True', '1');

SELECT ISNULL(SUM(paid.PaidAmount), 0) AS ActualCollected
FROM dbo.FundsCollection paid
INNER JOIN dbo.FeeSchoolCharges rate ON rate.FeeID=paid.FeeID
WHERE " + FtfChargePredicate + @"
  AND paid.DateOfDeposit >= @FromDate
  AND paid.DateOfDeposit <= @ToDate;";

            var result = new FtfCollectionPerformance();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate.Date;
                command.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate.Date;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result.TotalStudents = GetInt32(reader, "TotalStudents");
                        result.StudentsWithRate = GetInt32(reader, "StudentsWithRate");
                        result.MonthlyExpected = GetDecimal(reader, "MonthlyExpected");
                    }
                    if (reader.NextResult() && reader.Read())
                        result.ActualCollected = GetDecimal(reader, "ActualCollected");
                }
            }

            result.MonthCount = ((toDate.Year - fromDate.Year) * 12) + toDate.Month - fromDate.Month + 1;
            if (result.MonthCount < 1)
                result.MonthCount = 1;
            result.ExpectedForPeriod = result.MonthlyExpected * result.MonthCount;
            return result;
        }

        internal static DataTable GetDocuments(int transactionId)
        {
            const string sql = @"
SELECT FundDocumentID, DocumentType, OriginalFileName, ContentType, FileSizeBytes, UploadedAtUtc
FROM dbo.FundTransactionDocuments
WHERE FundTransactionID=@FundTransactionID
ORDER BY CASE WHEN DocumentType=N'Cheque' THEN 0 ELSE 1 END, FundDocumentID;";
            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@FundTransactionID", SqlDbType.Int).Value = transactionId;
                adapter.Fill(table);
            }
            return table;
        }

        internal static FundDocumentData GetDocument(int documentId)
        {
            const string sql = @"
SELECT OriginalFileName, ContentType, FileSizeBytes, DocumentData
FROM dbo.FundTransactionDocuments
WHERE FundDocumentID=@FundDocumentID;";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@FundDocumentID", SqlDbType.Int).Value = documentId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                        return null;
                    return new FundDocumentData
                    {
                        FileName = Convert.ToString(reader["OriginalFileName"]),
                        ContentType = Convert.ToString(reader["ContentType"]),
                        Data = reader["DocumentData"] as byte[]
                    };
                }
            }
        }

        internal static FundTransactionEntry GetTransaction(int transactionId)
        {
            const string sql = @"
SELECT FundTransactionID, TransactionNumber, FundType, TransactionType, TransactionDate,
       Amount, SourceOrPayee, ReferenceNo, ChequeNo, WorkType, Purpose, Remarks
FROM dbo.FundTransactions WHERE FundTransactionID=@ID;";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@ID", SqlDbType.Int).Value = transactionId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                        return null;
                    return new FundTransactionEntry
                    {
                        FundTransactionID = transactionId,
                        TransactionNumber = Convert.ToString(reader["TransactionNumber"]),
                        FundType = Convert.ToString(reader["FundType"]),
                        TransactionType = Convert.ToString(reader["TransactionType"]),
                        TransactionDate = Convert.ToDateTime(reader["TransactionDate"], CultureInfo.InvariantCulture),
                        Amount = Convert.ToDecimal(reader["Amount"], CultureInfo.InvariantCulture),
                        SourceOrPayee = Convert.ToString(reader["SourceOrPayee"]),
                        ReferenceNo = Convert.ToString(reader["ReferenceNo"]),
                        ChequeNo = Convert.ToString(reader["ChequeNo"]),
                        WorkType = Convert.ToString(reader["WorkType"]),
                        Purpose = Convert.ToString(reader["Purpose"]),
                        Remarks = Convert.ToString(reader["Remarks"])
                    };
                }
            }
        }

        private static void ValidateAvailableBalance(SqlConnection connection, SqlTransaction transaction, FundTransactionEntry entry)
        {
            if ((entry.FundType == "NSB" || entry.FundType == "SMC") && entry.TransactionType == "Utilization")
            {
                decimal available;
                using (SqlCommand balanceCommand = new SqlCommand(@"
SELECT ISNULL(SUM(CASE WHEN TransactionType=N'Receipt' THEN Amount
                       WHEN TransactionType=N'Utilization' THEN -Amount ELSE 0 END), 0)
FROM dbo.FundTransactions WITH (UPDLOCK, HOLDLOCK)
WHERE FundType=@FundType;", connection, transaction))
                {
                    balanceCommand.Parameters.Add("@FundType", SqlDbType.NVarChar, 10).Value = entry.FundType;
                    object value = balanceCommand.ExecuteScalar();
                    available = value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                }
                if (entry.Amount > available)
                    throw new InvalidOperationException(entry.FundType + " utilization cannot exceed the current balance of Rs. " + available.ToString("N2", CultureInfo.InvariantCulture) + ".");
            }
            else if (entry.FundType == "FTF" && entry.TransactionType == "Deposit")
            {
                string sql = @"
SELECT
    ISNULL((SELECT SUM(paid.PaidAmount)
            FROM dbo.FundsCollection paid
            INNER JOIN dbo.FeeSchoolCharges rate ON rate.FeeID=paid.FeeID
            WHERE " + FtfChargePredicate + @"), 0)
    -
    ISNULL((SELECT SUM(t.Amount)
            FROM dbo.FundTransactions t WITH (UPDLOCK, HOLDLOCK)
            WHERE t.FundType=N'FTF' AND t.TransactionType=N'Deposit'), 0);";
                decimal available = ExecuteDecimal(connection, transaction, sql);
                if (entry.Amount > available)
                    throw new InvalidOperationException("The bank deposit cannot exceed FTF Cash in Hand of Rs. " + available.ToString("N2", CultureInfo.InvariantCulture) + ".");
            }
            else if (entry.FundType == "FTF" && entry.TransactionType == "Utilization")
            {
                decimal available = ExecuteDecimal(connection, transaction, @"
SELECT ISNULL(SUM(CASE WHEN TransactionType=N'Deposit' THEN Amount
                       WHEN TransactionType=N'Utilization' THEN -Amount ELSE 0 END), 0)
FROM dbo.FundTransactions WITH (UPDLOCK, HOLDLOCK)
WHERE FundType=N'FTF';");
                if (entry.Amount > available)
                    throw new InvalidOperationException("FTF utilization cannot exceed the current FTF account balance of Rs. " + available.ToString("N2", CultureInfo.InvariantCulture) + ".");
            }
        }

        private static decimal ExecuteDecimal(SqlConnection connection, SqlTransaction transaction, string sql)
        {
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value
                    ? 0m
                    : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            }
        }

        private static void ValidateEntry(FundTransactionEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");
            entry.FundType = NormalizeFundType(entry.FundType);
            entry.TransactionType = (entry.TransactionType ?? string.Empty).Trim();
            bool validType = ((entry.FundType == "NSB" || entry.FundType == "SMC") && (entry.TransactionType == "Receipt" || entry.TransactionType == "Utilization"))
                             || (entry.FundType == "FTF" && (entry.TransactionType == "Deposit" || entry.TransactionType == "Utilization"));
            if (!validType)
                throw new InvalidOperationException("The selected fund transaction type is invalid.");
            if (entry.Amount <= 0m || entry.Amount > 999999999999.99m)
                throw new InvalidOperationException("Enter a transaction amount greater than zero and within the supported limit.");
            if (entry.TransactionDate.Year < 2000 || entry.TransactionDate.Year > 2100)
                throw new InvalidOperationException("Enter a valid transaction date.");
            if (entry.TransactionType == "Utilization" && string.IsNullOrWhiteSpace(entry.ChequeNo))
                throw new InvalidOperationException("Enter the cheque number for this utilization transaction.");
            if (entry.TransactionType == "Utilization" && string.IsNullOrWhiteSpace(entry.Purpose))
                throw new InvalidOperationException("Enter the purpose of fund utilization.");
        }

        private static string NormalizeFundType(string fundType)
        {
            string value = (fundType ?? string.Empty).Trim().ToUpperInvariant();
            if (value != "NSB" && value != "FTF" && value != "SMC")
                throw new InvalidOperationException("The fund type must be NSB, FTF, or SMC.");
            return value;
        }

        private static string BuildTransactionNumber(FundTransactionEntry entry, int transactionId)
        {
            string code = entry.TransactionType == "Utilization" ? "UTL" :
                          entry.TransactionType == "Deposit" ? "DEP" : "REC";
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}-{1}-{2:yyyyMMdd}-{3:D4}",
                entry.FundType,
                code,
                entry.TransactionDate,
                transactionId);
        }

        private static void AddText(SqlCommand command, string name, int size, string value)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (normalized.Length > size)
                normalized = normalized.Substring(0, size);
            command.Parameters.Add(name, SqlDbType.NVarChar, size).Value = normalized;
        }

        private static void AddNullableText(SqlCommand command, string name, int size, string value)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (normalized.Length > size)
                normalized = normalized.Substring(0, size);
            command.Parameters.Add(name, SqlDbType.NVarChar, size).Value = normalized.Length == 0
                ? (object)DBNull.Value
                : normalized;
        }

        private static void AddNullableInt(SqlCommand command, string name, int? value)
        {
            command.Parameters.Add(name, SqlDbType.Int).Value = value.HasValue
                ? (object)value.Value
                : DBNull.Value;
        }

        private static string RequireText(string value, string fieldName, int maximumLength)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (normalized.Length == 0)
                throw new InvalidOperationException("Enter the " + fieldName + ".");
            if (normalized.Length > maximumLength)
                throw new InvalidOperationException("The " + fieldName + " cannot exceed " + maximumLength + " characters.");
            return normalized;
        }

        private static decimal GetDecimal(IDataRecord record, string name)
        {
            int ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? 0m : Convert.ToDecimal(record.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        private static int GetInt32(IDataRecord record, string name)
        {
            int ordinal = record.GetOrdinal(name);
            return record.IsDBNull(ordinal) ? 0 : Convert.ToInt32(record.GetValue(ordinal), CultureInfo.InvariantCulture);
        }
    }

    internal sealed class FundBankAccountDetails
    {
        internal string FundType { get; set; }
        internal string BankName { get; set; }
        internal string BranchCode { get; set; }
        internal string BranchAddress { get; set; }
        internal string AccountIBAN { get; set; }
        internal int? UpdatedByUserID { get; set; }
        internal DateTime? UpdatedAtUtc { get; set; }
        internal bool IsConfigured { get { return !string.IsNullOrWhiteSpace(BankName) && !string.IsNullOrWhiteSpace(AccountIBAN); } }
    }

    internal sealed class FundTransactionEntry
    {
        internal int FundTransactionID { get; set; }
        internal string TransactionNumber { get; set; }
        internal string FundType { get; set; }
        internal string TransactionType { get; set; }
        internal DateTime TransactionDate { get; set; }
        internal decimal Amount { get; set; }
        internal string SourceOrPayee { get; set; }
        internal string ReferenceNo { get; set; }
        internal string ChequeNo { get; set; }
        internal string WorkType { get; set; }
        internal string Purpose { get; set; }
        internal string Remarks { get; set; }
        internal int? CreatedByUserID { get; set; }
    }

    internal sealed class FundDocumentUpload
    {
        internal string DocumentType { get; set; }
        internal string FileName { get; set; }
        internal string ContentType { get; set; }
        internal byte[] Data { get; set; }
    }

    internal sealed class FundDocumentData
    {
        internal string FileName { get; set; }
        internal string ContentType { get; set; }
        internal byte[] Data { get; set; }
    }

    internal sealed class FundAccountSummary
    {
        internal decimal NsbReceived { get; set; }
        internal decimal NsbUtilized { get; set; }
        internal decimal NsbBalance { get { return NsbReceived - NsbUtilized; } }
        internal decimal FtfCollected { get; set; }
        internal decimal FtfDeposited { get; set; }
        internal decimal FtfCashInHand { get { return FtfCollected - FtfDeposited; } }
        internal decimal FtfUtilized { get; set; }
        internal decimal FtfAccountBalance { get { return FtfDeposited - FtfUtilized; } }
    }

    internal sealed class FundStatementTotals
    {
        internal decimal OpeningBalance { get; set; }
        internal decimal PeriodCredits { get; set; }
        internal decimal PeriodDebits { get; set; }
        internal decimal ClosingBalance { get { return OpeningBalance + PeriodCredits - PeriodDebits; } }
    }

    internal sealed class FtfCollectionPerformance
    {
        internal int TotalStudents { get; set; }
        internal int StudentsWithRate { get; set; }
        internal int MonthCount { get; set; }
        internal decimal MonthlyExpected { get; set; }
        internal decimal ExpectedForPeriod { get; set; }
        internal decimal ActualCollected { get; set; }
        internal decimal Variance { get { return ActualCollected - ExpectedForPeriod; } }
        internal decimal CollectionPercentage
        {
            get { return ExpectedForPeriod <= 0m ? 0m : (ActualCollected * 100m) / ExpectedForPeriod; }
        }
    }
}
