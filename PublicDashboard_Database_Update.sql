SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.PublicSchoolProfile', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PublicSchoolProfile
    (
        ProfileID INT NOT NULL CONSTRAINT PK_PublicSchoolProfile PRIMARY KEY,
        PrincipalTeacherID INT NULL,
        PrincipalDisplayName NVARCHAR(150) NULL,
        PrincipalDesignation NVARCHAR(120) NULL,
        PrincipalMessage NVARCHAR(1200) NULL,
        SchoolIntroduction NVARCHAR(2000) NOT NULL,
        SchoolHistory NVARCHAR(3000) NOT NULL,
        UpdatedByUserID INT NULL,
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_PublicSchoolProfile_Updated DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT CK_PublicSchoolProfile_OneRow CHECK(ProfileID=1)
    );
END;

IF NOT EXISTS(SELECT 1 FROM dbo.PublicSchoolProfile WHERE ProfileID=1)
BEGIN
    INSERT dbo.PublicSchoolProfile(ProfileID,PrincipalDisplayName,PrincipalDesignation,PrincipalMessage,SchoolIntroduction,SchoolHistory)
    VALUES
    (
        1,
        N'School Head / Principal',
        N'Principal',
        N'We welcome students, parents and the community to a disciplined learning environment focused on knowledge, character and responsible citizenship.',
        N'Government Higher Secondary School Maankot serves the community by providing accessible education, qualified teaching support and opportunities for academic and personal development.',
        N'The school has grown with the educational needs of Maankot and surrounding communities. Its continuing record reflects the commitment of students, teachers, parents and school leadership to public education.'
    );
END;

IF OBJECT_ID(N'dbo.SchoolAnnouncements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchoolAnnouncements
    (
        AnnouncementID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchoolAnnouncements PRIMARY KEY,
        Title NVARCHAR(220) NOT NULL,
        AnnouncementCategory NVARCHAR(60) NOT NULL,
        Summary NVARCHAR(600) NOT NULL,
        AnnouncementBody NVARCHAR(4000) NOT NULL,
        EventDate DATE NULL,
        IsPublished BIT NOT NULL CONSTRAINT DF_SchoolAnnouncements_Published DEFAULT(0),
        IsPinned BIT NOT NULL CONSTRAINT DF_SchoolAnnouncements_Pinned DEFAULT(0),
        PublishedAtUtc DATETIME2(0) NULL,
        CoverImageData VARBINARY(MAX) NULL,
        CoverImageContentType NVARCHAR(100) NULL,
        CoverImageFileName NVARCHAR(260) NULL,
        MediaType NVARCHAR(20) NULL,
        MediaProvider NVARCHAR(30) NULL,
        MediaUrl NVARCHAR(1000) NULL,
        MediaEmbedUrl NVARCHAR(1200) NULL,
        MediaRenderMode NVARCHAR(30) NULL,
        MediaTitle NVARCHAR(200) NULL,
        CreatedByUserID INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SchoolAnnouncements_Created DEFAULT(SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SchoolAnnouncements_Updated DEFAULT(SYSUTCDATETIME())
    );
END;

IF COL_LENGTH(N'dbo.SchoolAnnouncements',N'MediaType') IS NULL
    ALTER TABLE dbo.SchoolAnnouncements ADD MediaType NVARCHAR(20) NULL;
IF COL_LENGTH(N'dbo.SchoolAnnouncements',N'MediaProvider') IS NULL
    ALTER TABLE dbo.SchoolAnnouncements ADD MediaProvider NVARCHAR(30) NULL;
IF COL_LENGTH(N'dbo.SchoolAnnouncements',N'MediaUrl') IS NULL
    ALTER TABLE dbo.SchoolAnnouncements ADD MediaUrl NVARCHAR(1000) NULL;
IF COL_LENGTH(N'dbo.SchoolAnnouncements',N'MediaEmbedUrl') IS NULL
    ALTER TABLE dbo.SchoolAnnouncements ADD MediaEmbedUrl NVARCHAR(1200) NULL;
IF COL_LENGTH(N'dbo.SchoolAnnouncements',N'MediaRenderMode') IS NULL
    ALTER TABLE dbo.SchoolAnnouncements ADD MediaRenderMode NVARCHAR(30) NULL;
IF COL_LENGTH(N'dbo.SchoolAnnouncements',N'MediaTitle') IS NULL
    ALTER TABLE dbo.SchoolAnnouncements ADD MediaTitle NVARCHAR(200) NULL;

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.SchoolAnnouncements') AND name=N'IX_SchoolAnnouncements_Public')
    CREATE INDEX IX_SchoolAnnouncements_Public ON dbo.SchoolAnnouncements(IsPublished,IsPinned,EventDate,PublishedAtUtc);
