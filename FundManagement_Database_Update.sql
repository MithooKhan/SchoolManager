/*
    Digital School Manager - NSB, FTF and SMC Fund Management
    Safe to run more than once against SchoolDatabase.
*/
SET XACT_ABORT ON;
GO

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
GO

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
GO

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
GO

IF EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_FundTransactions_FundType' AND definition NOT LIKE N'%SMC%')
BEGIN
    ALTER TABLE dbo.FundTransactions DROP CONSTRAINT CK_FundTransactions_FundType;
    ALTER TABLE dbo.FundTransactions ADD CONSTRAINT CK_FundTransactions_FundType CHECK(FundType IN(N'NSB',N'FTF',N'SMC'));
END;
GO

IF EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_FundBankAccounts_FundType' AND definition NOT LIKE N'%SMC%')
BEGIN
    ALTER TABLE dbo.FundBankAccounts DROP CONSTRAINT CK_FundBankAccounts_FundType;
    ALTER TABLE dbo.FundBankAccounts ADD CONSTRAINT CK_FundBankAccounts_FundType CHECK(FundType IN(N'NSB',N'FTF',N'SMC'));
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FundTransactions')
      AND name = N'UX_FundTransactions_TransactionNumber'
)
    CREATE UNIQUE NONCLUSTERED INDEX UX_FundTransactions_TransactionNumber
        ON dbo.FundTransactions (TransactionNumber);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FundTransactions')
      AND name = N'IX_FundTransactions_FundDate'
)
    CREATE NONCLUSTERED INDEX IX_FundTransactions_FundDate
        ON dbo.FundTransactions (FundType, TransactionDate DESC)
        INCLUDE (TransactionType, Amount, ChequeNo, WorkType);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FundTransactionDocuments')
      AND name = N'IX_FundTransactionDocuments_Transaction'
)
    CREATE NONCLUSTERED INDEX IX_FundTransactionDocuments_Transaction
        ON dbo.FundTransactionDocuments (FundTransactionID, DocumentType);
GO

PRINT 'NSB, FTF and SMC fund management schema is ready.';
GO
