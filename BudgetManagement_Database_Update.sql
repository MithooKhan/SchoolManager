/* Digital School Manager - Annual Budget module
   Safe to run more than once against SchoolDatabase. */
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.BudgetAllowances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BudgetAllowances
    (
        AllowanceID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BudgetAllowances PRIMARY KEY,
        AllowanceCode NVARCHAR(25) NOT NULL,
        AllowanceName NVARCHAR(160) NOT NULL,
        SortOrder INT NOT NULL CONSTRAINT DF_BudgetAllowances_SortOrder DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_BudgetAllowances_IsActive DEFAULT (1),
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_BudgetAllowances_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UX_BudgetAllowances_Code UNIQUE (AllowanceCode)
    );
END;

IF OBJECT_ID(N'dbo.BudgetPayScales', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BudgetPayScales
    (
        BPS INT NOT NULL CONSTRAINT PK_BudgetPayScales PRIMARY KEY,
        MinimumPay DECIMAL(18,2) NOT NULL,
        MaximumPay DECIMAL(18,2) NOT NULL,
        AnnualIncrement DECIMAL(18,2) NOT NULL,
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_BudgetPayScales_Updated DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_BudgetPayScales_BPS CHECK (BPS BETWEEN 1 AND 22),
        CONSTRAINT CK_BudgetPayScales_Amounts CHECK (MinimumPay >= 0 AND MaximumPay >= MinimumPay AND AnnualIncrement >= 0)
    );
END;

IF OBJECT_ID(N'dbo.BudgetPostCodes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BudgetPostCodes
    (
        BudgetPostID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BudgetPostCodes PRIMARY KEY,
        StaffCategory NVARCHAR(20) NOT NULL,
        SourcePostID INT NOT NULL,
        PostCode NVARCHAR(30) NOT NULL,
        PostName NVARCHAR(160) NOT NULL,
        BPS INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_BudgetPostCodes_IsActive DEFAULT (1),
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_BudgetPostCodes_Updated DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_BudgetPostCodes_Category CHECK (StaffCategory IN (N'Teaching', N'Non-Teaching')),
        CONSTRAINT UX_BudgetPostCodes_Source UNIQUE (StaffCategory, SourcePostID),
        CONSTRAINT UX_BudgetPostCodes_Code UNIQUE (PostCode)
    );
END;

IF OBJECT_ID(N'dbo.AnnualBudgets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AnnualBudgets
    (
        BudgetID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AnnualBudgets PRIMARY KEY,
        BudgetName NVARCHAR(120) NOT NULL,
        FiscalStartYear INT NOT NULL,
        FiscalEndYear INT NOT NULL,
        SchoolName NVARCHAR(250) NOT NULL,
        LocalGovernmentName NVARCHAR(200) NOT NULL,
        DemandName NVARCHAR(200) NOT NULL,
        GrantNo NVARCHAR(30) NOT NULL,
        DetailedFunctionCode NVARCHAR(30) NOT NULL,
        DetailedFunctionName NVARCHAR(200) NOT NULL,
        CostCenterCode NVARCHAR(30) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_AnnualBudgets_Status DEFAULT (N'Draft'),
        CreatedByUserID INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_AnnualBudgets_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_AnnualBudgets_Updated DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UX_AnnualBudgets_Name UNIQUE (BudgetName),
        CONSTRAINT CK_AnnualBudgets_Years CHECK (FiscalEndYear = FiscalStartYear + 1),
        CONSTRAINT CK_AnnualBudgets_Status CHECK (Status IN (N'Draft', N'Finalized'))
    );
END;

IF OBJECT_ID(N'dbo.BudgetStaffLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BudgetStaffLines
    (
        BudgetStaffLineID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BudgetStaffLines PRIMARY KEY,
        BudgetID INT NOT NULL,
        BudgetPostID INT NOT NULL,
        StaffCategory NVARCHAR(20) NOT NULL,
        SourceStaffID INT NULL,
        VacancySequence INT NULL,
        EmployeeName NVARCHAR(160) NOT NULL,
        Gender NVARCHAR(10) NOT NULL CONSTRAINT DF_BudgetStaffLines_Gender DEFAULT (N'Male'),
        Designation NVARCHAR(160) NOT NULL,
        PostCode NVARCHAR(30) NOT NULL,
        BPS INT NOT NULL,
        IsVacant BIT NOT NULL CONSTRAINT DF_BudgetStaffLines_IsVacant DEFAULT (0),
        BasicPay DECIMAL(18,2) NOT NULL CONSTRAINT DF_BudgetStaffLines_BasicPay DEFAULT (0),
        MinimumPay DECIMAL(18,2) NOT NULL CONSTRAINT DF_BudgetStaffLines_MinimumPay DEFAULT (0),
        MaximumPay DECIMAL(18,2) NOT NULL CONSTRAINT DF_BudgetStaffLines_MaximumPay DEFAULT (0),
        IncrementDate DATE NULL,
        IncrementRate DECIMAL(18,2) NOT NULL CONSTRAINT DF_BudgetStaffLines_Increment DEFAULT (0),
        RecruitmentPlanned BIT NOT NULL CONSTRAINT DF_BudgetStaffLines_Recruitment DEFAULT (0),
        Remarks NVARCHAR(300) NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_BudgetStaffLines_Created DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_BudgetStaffLines_Updated DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_BudgetStaffLines_Budget FOREIGN KEY (BudgetID) REFERENCES dbo.AnnualBudgets(BudgetID) ON DELETE CASCADE,
        CONSTRAINT FK_BudgetStaffLines_Post FOREIGN KEY (BudgetPostID) REFERENCES dbo.BudgetPostCodes(BudgetPostID),
        CONSTRAINT CK_BudgetStaffLines_Category CHECK (StaffCategory IN (N'Teaching', N'Non-Teaching')),
        CONSTRAINT CK_BudgetStaffLines_Gender CHECK (Gender IN (N'Male', N'Female')),
        CONSTRAINT CK_BudgetStaffLines_Amounts CHECK (BasicPay >= 0 AND IncrementRate >= 0)
    );
END;

IF OBJECT_ID(N'dbo.BudgetStaffAllowances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BudgetStaffAllowances
    (
        BudgetStaffAllowanceID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BudgetStaffAllowances PRIMARY KEY,
        BudgetStaffLineID INT NOT NULL,
        AllowanceID INT NOT NULL,
        AllowanceCodeSnapshot NVARCHAR(25) NOT NULL,
        AllowanceNameSnapshot NVARCHAR(160) NOT NULL,
        SortOrderSnapshot INT NOT NULL,
        MonthlyAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_BudgetStaffAllowances_Amount DEFAULT (0),
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_BudgetStaffAllowances_Updated DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_BudgetStaffAllowances_Line FOREIGN KEY (BudgetStaffLineID) REFERENCES dbo.BudgetStaffLines(BudgetStaffLineID) ON DELETE CASCADE,
        CONSTRAINT FK_BudgetStaffAllowances_Allowance FOREIGN KEY (AllowanceID) REFERENCES dbo.BudgetAllowances(AllowanceID),
        CONSTRAINT UX_BudgetStaffAllowances_LineAllowance UNIQUE (BudgetStaffLineID, AllowanceID),
        CONSTRAINT CK_BudgetStaffAllowances_Amount CHECK (MonthlyAmount >= 0)
    );
END;

IF COL_LENGTH(N'dbo.BudgetStaffLines', N'MinimumPay') IS NULL
    ALTER TABLE dbo.BudgetStaffLines ADD MinimumPay DECIMAL(18,2) NOT NULL CONSTRAINT DF_BudgetStaffLines_MinimumPay_Legacy DEFAULT (0);
IF COL_LENGTH(N'dbo.BudgetStaffLines', N'MaximumPay') IS NULL
    ALTER TABLE dbo.BudgetStaffLines ADD MaximumPay DECIMAL(18,2) NOT NULL CONSTRAINT DF_BudgetStaffLines_MaximumPay_Legacy DEFAULT (0);
IF COL_LENGTH(N'dbo.BudgetStaffAllowances', N'AllowanceCodeSnapshot') IS NULL
    ALTER TABLE dbo.BudgetStaffAllowances ADD AllowanceCodeSnapshot NVARCHAR(25) NOT NULL CONSTRAINT DF_BudgetStaffAllowances_CodeSnapshot DEFAULT (N'');
IF COL_LENGTH(N'dbo.BudgetStaffAllowances', N'AllowanceNameSnapshot') IS NULL
    ALTER TABLE dbo.BudgetStaffAllowances ADD AllowanceNameSnapshot NVARCHAR(160) NOT NULL CONSTRAINT DF_BudgetStaffAllowances_NameSnapshot DEFAULT (N'');
IF COL_LENGTH(N'dbo.BudgetStaffAllowances', N'SortOrderSnapshot') IS NULL
    ALTER TABLE dbo.BudgetStaffAllowances ADD SortOrderSnapshot INT NOT NULL CONSTRAINT DF_BudgetStaffAllowances_SortSnapshot DEFAULT (0);

IF OBJECT_ID(N'dbo.BudgetObjectEntries', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BudgetObjectEntries
    (
        BudgetObjectEntryID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BudgetObjectEntries PRIMARY KEY,
        BudgetID INT NOT NULL,
        ObjectCode NVARCHAR(25) NOT NULL,
        ObjectName NVARCHAR(180) NOT NULL,
        PreviousBudget DECIMAL(18,2) NOT NULL CONSTRAINT DF_BudgetObjectEntries_Previous DEFAULT (0),
        CurrentRevised DECIMAL(18,2) NOT NULL CONSTRAINT DF_BudgetObjectEntries_Revised DEFAULT (0),
        ProposedBudget DECIMAL(18,2) NOT NULL CONSTRAINT DF_BudgetObjectEntries_Proposed DEFAULT (0),
        SortOrder INT NOT NULL CONSTRAINT DF_BudgetObjectEntries_Sort DEFAULT (0),
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_BudgetObjectEntries_Updated DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_BudgetObjectEntries_Budget FOREIGN KEY (BudgetID) REFERENCES dbo.AnnualBudgets(BudgetID) ON DELETE CASCADE,
        CONSTRAINT UX_BudgetObjectEntries_BudgetCode UNIQUE (BudgetID, ObjectCode),
        CONSTRAINT CK_BudgetObjectEntries_Amounts CHECK (PreviousBudget >= 0 AND CurrentRevised >= 0 AND ProposedBudget >= 0)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.BudgetStaffLines') AND name=N'IX_BudgetStaffLines_Budget')
    CREATE INDEX IX_BudgetStaffLines_Budget ON dbo.BudgetStaffLines(BudgetID, StaffCategory, BPS DESC, Designation);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.BudgetStaffLines') AND name=N'UX_BudgetStaffLines_Employee')
    CREATE UNIQUE INDEX UX_BudgetStaffLines_Employee ON dbo.BudgetStaffLines(BudgetID, StaffCategory, SourceStaffID)
    WHERE SourceStaffID IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.BudgetStaffLines') AND name=N'UX_BudgetStaffLines_Vacancy')
    CREATE UNIQUE INDEX UX_BudgetStaffLines_Vacancy ON dbo.BudgetStaffLines(BudgetID, BudgetPostID, VacancySequence)
    WHERE IsVacant = 1;

IF NOT EXISTS (SELECT 1 FROM dbo.BudgetAllowances)
BEGIN
    INSERT dbo.BudgetAllowances(AllowanceCode, AllowanceName, SortOrder, IsActive) VALUES
    (N'A01202',N'House Rent Allowance',10,1),(N'A01203',N'Conveyance Allowance',20,1),
    (N'A01207',N'Washing Allowance',30,0),(N'A01208',N'Dress Allowance',40,0),
    (N'A0120D',N'Integrated Allowance',50,1),(N'A0120X',N'Adhoc Relief 50% (2010-11)',60,0),
    (N'A01216',N'Qualification Allowance',70,1),(N'A01217',N'Medical Allowance',80,1),
    (N'A0121N',N'Personal Allowance',90,1),(N'A0121M',N'Adhoc Relief 20% (2012-13)',100,0),
    (N'A0121T',N'Adhoc Relief 10% (2013-14)',110,0),(N'A01224',N'Entertainment Allowance',120,1),
    (N'A01238',N'Charge Allowance',130,1),(N'A01253',N'Science Teaching Allowance',140,1),
    (N'A01270',N'Other Allowances',150,0),(N'A0122C',N'7.50% Adhoc Relief Allowance 2015',160,0),
    (N'A0122M',N'10% Adhoc Relief 2016-17',170,0),(N'A0122N',N'Special Conveyance Allowance to Disabled',180,1),
    (N'A0122Y',N'10% Adhoc Relief 2017-18',190,0),(N'A0123G',N'10% Adhoc Relief 2018-19',200,0),
    (N'A0123P',N'10% Adhoc Relief 2019-20',210,0),(N'A0124F',N'10% Adhoc Relief 2021-22',220,0),
    (N'A0124H',N'25% Special Allowance 2021',230,1),(N'A0124R',N'15% Adhoc Relief 2022',240,1),
    (N'A0124T',N'15% Special Allowance 2022',250,1),(N'A0124X',N'35% Special Allowance 2023',260,1),
    (N'A04115',N'30% in lieu of pension benefit',270,1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.BudgetPayScales)
BEGIN
    INSERT dbo.BudgetPayScales(BPS, MinimumPay, MaximumPay, AnnualIncrement) VALUES
    (1,13550,26450,430),(2,13820,28520,490),(3,14260,31660,580),(4,14690,34490,660),
    (5,15230,37730,750),(6,15760,40960,840),(7,16310,43610,910),(8,16890,46890,1000),
    (9,17470,50170,1090),(10,18050,53750,1190),(11,18650,57950,1310),(12,19770,62670,1430),
    (13,21160,67960,1560),(14,22530,74730,1740),(15,23920,83320,1980),(16,28070,95870,2260),
    (17,45070,113470,3420),(18,56880,142080,4260),(19,87840,178440,4530),(20,102470,196130,6690);
END;

UPDATE x
SET AllowanceCodeSnapshot = a.AllowanceCode,
    AllowanceNameSnapshot = a.AllowanceName,
    SortOrderSnapshot = a.SortOrder
FROM dbo.BudgetStaffAllowances x
INNER JOIN dbo.BudgetAllowances a ON a.AllowanceID=x.AllowanceID
WHERE x.AllowanceCodeSnapshot=N'' OR x.AllowanceNameSnapshot=N'';

UPDATE l
SET MinimumPay=p.MinimumPay, MaximumPay=p.MaximumPay
FROM dbo.BudgetStaffLines l
INNER JOIN dbo.BudgetPayScales p ON p.BPS=l.BPS
WHERE l.MinimumPay=0 AND l.MaximumPay=0;
