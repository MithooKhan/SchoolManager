SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SchoolAttendanceHolidays', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchoolAttendanceHolidays
    (
        HolidayID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchoolAttendanceHolidays PRIMARY KEY,
        HolidayDate DATE NOT NULL,
        HolidayEndDate DATE NOT NULL,
        HolidayName NVARCHAR(150) NOT NULL,
        CreatedByUserID INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SchoolAttendanceHolidays_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SchoolAttendanceHolidays_UpdatedAtUtc DEFAULT (SYSUTCDATETIME())
    );
END;

IF COL_LENGTH(N'dbo.SchoolAttendanceHolidays', N'HolidayEndDate') IS NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.SchoolAttendanceHolidays ADD HolidayEndDate DATE NULL;');
END;

/* Dynamic statements force compilation after the new column exists. */
EXEC(N'UPDATE dbo.SchoolAttendanceHolidays
SET HolidayEndDate = HolidayDate
WHERE HolidayEndDate IS NULL;');

IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.SchoolAttendanceHolidays')
      AND name = N'HolidayEndDate'
      AND is_nullable = 1
)
BEGIN
    EXEC(N'ALTER TABLE dbo.SchoolAttendanceHolidays ALTER COLUMN HolidayEndDate DATE NOT NULL;');
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.SchoolAttendanceHolidays')
      AND name = N'CK_SchoolAttendanceHolidays_DateRange'
)
BEGIN
    EXEC(N'ALTER TABLE dbo.SchoolAttendanceHolidays WITH CHECK
        ADD CONSTRAINT CK_SchoolAttendanceHolidays_DateRange
        CHECK (HolidayEndDate >= HolidayDate);');
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.SchoolAttendanceHolidays')
      AND name = N'UX_SchoolAttendanceHolidays_Date'
)
BEGIN
    EXEC(N'CREATE UNIQUE NONCLUSTERED INDEX UX_SchoolAttendanceHolidays_Date
        ON dbo.SchoolAttendanceHolidays (HolidayDate);');
END;

COMMIT TRANSACTION;
