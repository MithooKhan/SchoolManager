SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.OldSchoolRecords',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OldSchoolRecords
    (
        RecordID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OldSchoolRecords PRIMARY KEY,
        ArchiveNumber NVARCHAR(60) NOT NULL,
        Category NVARCHAR(80) NOT NULL,
        RecordTitle NVARCHAR(250) NOT NULL,
        RecordReference NVARCHAR(120) NULL,
        RecordStartDate DATE NULL,
        RecordEndDate DATE NULL,
        RegisterYear NVARCHAR(30) NULL,
        PhysicalLocation NVARCHAR(250) NULL,
        ConfidentialityLevel NVARCHAR(30) NOT NULL CONSTRAINT DF_OldSchoolRecords_Confidentiality DEFAULT(N'Official'),
        Description NVARCHAR(1500) NULL,
        Keywords NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_OldSchoolRecords_Active DEFAULT(1),
        CreatedByUserID INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_OldSchoolRecords_Created DEFAULT(SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_OldSchoolRecords_Updated DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT UX_OldSchoolRecords_Archive UNIQUE(ArchiveNumber)
    );
END;

IF OBJECT_ID(N'dbo.OldSchoolRecordDocuments',N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OldSchoolRecordDocuments
    (
        DocumentID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OldSchoolRecordDocuments PRIMARY KEY,
        RecordID INT NOT NULL,
        PageNumber INT NOT NULL,
        OriginalFileName NVARCHAR(260) NOT NULL,
        ContentType NVARCHAR(100) NOT NULL,
        FileSizeBytes INT NOT NULL,
        DocumentData VARBINARY(MAX) NOT NULL,
        UploadedByUserID INT NULL,
        UploadedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_OldSchoolRecordDocuments_Uploaded DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT FK_OldSchoolRecordDocuments_Record FOREIGN KEY(RecordID) REFERENCES dbo.OldSchoolRecords(RecordID) ON DELETE CASCADE,
        CONSTRAINT UX_OldSchoolRecordDocuments_Page UNIQUE(RecordID,PageNumber)
    );
END;

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.OldSchoolRecords') AND name=N'IX_OldSchoolRecords_Search')
    CREATE INDEX IX_OldSchoolRecords_Search ON dbo.OldSchoolRecords(Category,RecordTitle,RegisterYear);
