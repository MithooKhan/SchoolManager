/*
    Digital School Manager - Version 11
    Office Daak Diary and Dispatch database-document update

    The Daak pages run this update automatically. This script is provided for
    installations where the website database account cannot create tables.
*/

SET XACT_ABORT ON;

BEGIN TRY
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
            DiaryID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OfficeDaakDiary PRIMARY KEY,
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
            CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_OfficeDaakDiary_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
            UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_OfficeDaakDiary_UpdatedAtUtc DEFAULT (SYSUTCDATETIME())
        );
    END;

    IF OBJECT_ID(N'dbo.OfficeDaakDispatch', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.OfficeDaakDispatch
        (
            DispatchID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OfficeDaakDispatch PRIMARY KEY,
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
            CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_OfficeDaakDispatch_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
            UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_OfficeDaakDispatch_UpdatedAtUtc DEFAULT (SYSUTCDATETIME())
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

    IF OBJECT_ID(N'dbo.OfficeDaakDocuments', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.OfficeDaakDocuments
        (
            DocumentID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OfficeDaakDocuments PRIMARY KEY,
            RecordType NVARCHAR(10) NOT NULL,
            RecordID INT NOT NULL,
            SequenceNo INT NOT NULL,
            OriginalFileName NVARCHAR(260) NOT NULL,
            FileContentType NVARCHAR(100) NOT NULL,
            FileSizeBytes INT NOT NULL,
            DocumentData VARBINARY(MAX) NOT NULL,
            DocumentSha256 CHAR(64) NOT NULL,
            UploadedByUserID INT NULL,
            UploadedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_OfficeDaakDocuments_Uploaded DEFAULT(SYSUTCDATETIME()),
            CONSTRAINT CK_OfficeDaakDocuments_Type CHECK(RecordType IN(N'Incoming',N'Outgoing')),
            CONSTRAINT CK_OfficeDaakDocuments_Size CHECK(FileSizeBytes > 0 AND FileSizeBytes <= 5242880),
            CONSTRAINT UX_OfficeDaakDocuments_Sequence UNIQUE(RecordType, RecordID, SequenceNo)
        );
        CREATE INDEX IX_OfficeDaakDocuments_Record ON dbo.OfficeDaakDocuments(RecordType, RecordID, DocumentID);
    END;

    INSERT dbo.OfficeDaakDocuments
        (RecordType, RecordID, SequenceNo, OriginalFileName, FileContentType,
         FileSizeBytes, DocumentData, DocumentSha256, UploadedAtUtc)
    SELECT N'Incoming', d.DiaryID, 1,
           ISNULL(NULLIF(d.OriginalFileName,N''),N'diary-scan'),
           ISNULL(NULLIF(d.FileContentType,N''),N'application/octet-stream'),
           CONVERT(INT,DATALENGTH(d.DocumentData)), d.DocumentData,
           ISNULL(NULLIF(d.DocumentSha256,''),CONVERT(CHAR(64),HASHBYTES('SHA2_256',d.DocumentData),2)),
           d.CreatedAtUtc
    FROM dbo.OfficeDaakDiary d
    WHERE d.DocumentData IS NOT NULL AND DATALENGTH(d.DocumentData)<=5242880
      AND NOT EXISTS(SELECT 1 FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Incoming' AND x.RecordID=d.DiaryID);

    INSERT dbo.OfficeDaakDocuments
        (RecordType, RecordID, SequenceNo, OriginalFileName, FileContentType,
         FileSizeBytes, DocumentData, DocumentSha256, UploadedAtUtc)
    SELECT N'Outgoing', d.DispatchID, 1,
           ISNULL(NULLIF(d.OriginalFileName,N''),N'dispatch-scan'),
           ISNULL(NULLIF(d.FileContentType,N''),N'application/octet-stream'),
           CONVERT(INT,DATALENGTH(d.DocumentData)), d.DocumentData,
           ISNULL(NULLIF(d.DocumentSha256,''),CONVERT(CHAR(64),HASHBYTES('SHA2_256',d.DocumentData),2)),
           d.CreatedAtUtc
    FROM dbo.OfficeDaakDispatch d
    WHERE d.DocumentData IS NOT NULL AND DATALENGTH(d.DocumentData)<=5242880
      AND NOT EXISTS(SELECT 1 FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Outgoing' AND x.RecordID=d.DispatchID);

    UPDATE d SET DocumentData=NULL, DocumentSha256=NULL, StoredFileName=NULL,
                 OriginalFileName=NULL, FileContentType=NULL, FileSizeBytes=NULL
    FROM dbo.OfficeDaakDiary d
    WHERE d.DocumentData IS NOT NULL
      AND EXISTS(SELECT 1 FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Incoming' AND x.RecordID=d.DiaryID);

    UPDATE d SET DocumentData=NULL, DocumentSha256=NULL, StoredFileName=NULL,
                 OriginalFileName=NULL, FileContentType=NULL, FileSizeBytes=NULL
    FROM dbo.OfficeDaakDispatch d
    WHERE d.DocumentData IS NOT NULL
      AND EXISTS(SELECT 1 FROM dbo.OfficeDaakDocuments x WHERE x.RecordType=N'Outgoing' AND x.RecordID=d.DispatchID);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OfficeDaakDiary') AND name = N'UX_OfficeDaakDiary_DiaryNo')
        CREATE UNIQUE NONCLUSTERED INDEX UX_OfficeDaakDiary_DiaryNo ON dbo.OfficeDaakDiary (DiaryNo);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OfficeDaakDiary') AND name = N'IX_OfficeDaakDiary_Register')
        CREATE NONCLUSTERED INDEX IX_OfficeDaakDiary_Register ON dbo.OfficeDaakDiary (ReceivedDate DESC, Status)
            INCLUDE (DiaryNo, SenderOffice, Subject, Priority);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OfficeDaakDispatch') AND name = N'UX_OfficeDaakDispatch_DispatchNo')
        CREATE UNIQUE NONCLUSTERED INDEX UX_OfficeDaakDispatch_DispatchNo ON dbo.OfficeDaakDispatch (DispatchNo);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OfficeDaakDispatch') AND name = N'IX_OfficeDaakDispatch_Register')
        CREATE NONCLUSTERED INDEX IX_OfficeDaakDispatch_Register ON dbo.OfficeDaakDispatch (DispatchDate DESC, Status)
            INCLUDE (DispatchNo, RecipientOffice, Subject, Priority);

    COMMIT TRANSACTION;
    PRINT 'Office Daak database document columns are ready.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
