/*
    Digital School Manager V10 - account recovery and administrator continuity

    The Login page applies this update automatically when the database account
    has permission. Run this script once against SchoolDatabase only if the
    automatic update reports a database-permission error.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.SystemUsers', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SystemUsers
        (
            UserID INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_SystemUsers PRIMARY KEY,
            TeacherID INT NOT NULL,
            Username NVARCHAR(50) NOT NULL,
            NormalizedUsername NVARCHAR(50) NOT NULL,
            PasswordHash VARBINARY(64) NOT NULL,
            PasswordSalt VARBINARY(32) NOT NULL,
            PasswordIterations INT NOT NULL
                CONSTRAINT DF_SystemUsers_PasswordIterations DEFAULT (120000),
            UserRole NVARCHAR(20) NOT NULL
                CONSTRAINT DF_SystemUsers_UserRole DEFAULT (N'Admin'),
            IsActive BIT NOT NULL
                CONSTRAINT DF_SystemUsers_IsActive DEFAULT (1),
            FailedLoginCount INT NOT NULL
                CONSTRAINT DF_SystemUsers_FailedLoginCount DEFAULT (0),
            LockoutEndUtc DATETIME2(0) NULL,
            CreatedAtUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_SystemUsers_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
            LastLoginAtUtc DATETIME2(0) NULL
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.SystemUsers')
          AND name = N'UX_SystemUsers_NormalizedUsername'
    )
        CREATE UNIQUE NONCLUSTERED INDEX UX_SystemUsers_NormalizedUsername
            ON dbo.SystemUsers (NormalizedUsername);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.SystemUsers')
          AND name = N'UX_SystemUsers_TeacherID'
    )
        CREATE UNIQUE NONCLUSTERED INDEX UX_SystemUsers_TeacherID
            ON dbo.SystemUsers (TeacherID);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE parent_object_id = OBJECT_ID(N'dbo.SystemUsers')
          AND name = N'FK_SystemUsers_Teachers'
    )
        ALTER TABLE dbo.SystemUsers WITH CHECK
            ADD CONSTRAINT FK_SystemUsers_Teachers
            FOREIGN KEY (TeacherID) REFERENCES dbo.Teachers (teacherid);

    IF OBJECT_ID(N'dbo.SystemAccountAudit', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.SystemAccountAudit
        (
            AuditID INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_SystemAccountAudit PRIMARY KEY,
            ActionType NVARCHAR(40) NOT NULL,
            ActorUserID INT NULL,
            TargetUserID INT NULL,
            Details NVARCHAR(500) NULL,
            CreatedAtUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_SystemAccountAudit_CreatedAtUtc DEFAULT (SYSUTCDATETIME())
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.SystemAccountAudit')
          AND name = N'IX_SystemAccountAudit_CreatedAtUtc'
    )
        CREATE NONCLUSTERED INDEX IX_SystemAccountAudit_CreatedAtUtc
            ON dbo.SystemAccountAudit (CreatedAtUtc DESC, ActionType);

    COMMIT TRANSACTION;
    PRINT 'System authentication and account audit tables are ready.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
