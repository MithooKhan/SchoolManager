/*
    Digital School Manager V7 - Fee Collection and reminders database update

    Run this script once against SchoolDatabase if the Fee Collection page
    reports that it cannot perform the update automatically. The script is
    idempotent and preserves all existing FundsCollection records.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.FundsCollection', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.FundsCollection
        (
            CollectionID INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_FundsCollection PRIMARY KEY,
            StudentID INT NOT NULL,
            FeeID INT NOT NULL,
            DateOfDeposit DATE NOT NULL
        );
    END;

    IF COL_LENGTH('dbo.FundsCollection', 'PaidAmount') IS NULL
    BEGIN
        ALTER TABLE dbo.FundsCollection
            ADD PaidAmount DECIMAL(18,2) NOT NULL
                CONSTRAINT DF_FundsCollection_PaidAmount DEFAULT (0) WITH VALUES;
    END;

    IF COL_LENGTH('dbo.FundsCollection', 'VoucherNo') IS NULL
    BEGIN
        ALTER TABLE dbo.FundsCollection ADD VoucherNo NVARCHAR(50) NULL;
    END;

    IF COL_LENGTH('dbo.FundsCollection', 'Remarks') IS NULL
    BEGIN
        ALTER TABLE dbo.FundsCollection ADD Remarks NVARCHAR(250) NULL;
    END;

    IF COL_LENGTH('dbo.FundsCollection', 'CollectedAt') IS NULL
    BEGIN
        ALTER TABLE dbo.FundsCollection
            ADD CollectedAt DATETIME2(0) NOT NULL
                CONSTRAINT DF_FundsCollection_CollectedAt DEFAULT (SYSDATETIME()) WITH VALUES;
    END;

    IF COL_LENGTH('dbo.FundsCollection', 'FeeMonth') IS NULL
    BEGIN
        ALTER TABLE dbo.FundsCollection ADD FeeMonth TINYINT NULL;
    END;

    IF COL_LENGTH('dbo.FundsCollection', 'FeeYear') IS NULL
    BEGIN
        ALTER TABLE dbo.FundsCollection ADD FeeYear SMALLINT NULL;
    END;

    IF COL_LENGTH('dbo.FundsCollection', 'CollectedByTeacherID') IS NULL
    BEGIN
        ALTER TABLE dbo.FundsCollection ADD CollectedByTeacherID INT NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.FundsCollection')
          AND name = N'IX_FundsCollection_VoucherNo'
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_FundsCollection_VoucherNo
            ON dbo.FundsCollection (VoucherNo)
            INCLUDE (StudentID, DateOfDeposit, PaidAmount);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.FundsCollection')
          AND name = N'IX_FundsCollection_StudentPeriod'
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_FundsCollection_StudentPeriod
            ON dbo.FundsCollection (StudentID, FeeYear, FeeMonth)
            INCLUDE (VoucherNo, FeeID, DateOfDeposit, PaidAmount);
    END;

    IF OBJECT_ID(N'dbo.FeeReminderOutbox', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.FeeReminderOutbox
        (
            ReminderID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FeeReminderOutbox PRIMARY KEY,
            StudentID INT NOT NULL,
            FeeMonth TINYINT NOT NULL,
            FeeYear SMALLINT NOT NULL,
            ParentContact NVARCHAR(30) NULL,
            StudentName NVARCHAR(150) NOT NULL,
            MessageText NVARCHAR(500) NOT NULL,
            Status NVARCHAR(20) NOT NULL CONSTRAINT DF_FeeReminderOutbox_Status DEFAULT (N'Pending'),
            Attempts INT NOT NULL CONSTRAINT DF_FeeReminderOutbox_Attempts DEFAULT (0),
            CreatedByUserID INT NULL,
            CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_FeeReminderOutbox_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
            LastAttemptAtUtc DATETIME2(0) NULL,
            SentAtUtc DATETIME2(0) NULL,
            ProviderResponse NVARCHAR(500) NULL
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.FeeReminderOutbox')
          AND name = N'UX_FeeReminderOutbox_StudentPeriod'
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UX_FeeReminderOutbox_StudentPeriod
            ON dbo.FeeReminderOutbox (StudentID, FeeYear, FeeMonth);
    END;

    COMMIT TRANSACTION;
    PRINT 'Fee Collection database update completed successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
