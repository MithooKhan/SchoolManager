SET NOCOUNT ON;

IF COL_LENGTH(N'dbo.ClassSubjects', N'SubjectGroup') IS NULL
    ALTER TABLE dbo.ClassSubjects ADD SubjectGroup NVARCHAR(20) NOT NULL CONSTRAINT DF_ClassSubjects_SubjectGroup DEFAULT(N'Compulsory');
IF COL_LENGTH(N'dbo.ClassSubjects', N'OptionGroupCode') IS NULL
    ALTER TABLE dbo.ClassSubjects ADD OptionGroupCode NVARCHAR(30) NULL;
IF COL_LENGTH(N'dbo.ClassSubjects', N'OptionGroupName') IS NULL
    ALTER TABLE dbo.ClassSubjects ADD OptionGroupName NVARCHAR(100) NULL;
IF COL_LENGTH(N'dbo.ClassSubjects', N'ReligionEligibility') IS NULL
    ALTER TABLE dbo.ClassSubjects ADD ReligionEligibility NVARCHAR(20) NOT NULL CONSTRAINT DF_ClassSubjects_Religion DEFAULT(N'All');
IF COL_LENGTH(N'dbo.ClassSubjects', N'IsStudentSelectable') IS NULL
    ALTER TABLE dbo.ClassSubjects ADD IsStudentSelectable BIT NOT NULL CONSTRAINT DF_ClassSubjects_Selectable DEFAULT(0);
IF COL_LENGTH(N'dbo.ClassSubjects', N'DisplayOrder') IS NULL
    ALTER TABLE dbo.ClassSubjects ADD DisplayOrder INT NOT NULL CONSTRAINT DF_ClassSubjects_DisplayOrder DEFAULT(0);

IF COL_LENGTH(N'dbo.Students', N'Religion') IS NULL
    ALTER TABLE dbo.Students ADD Religion NVARCHAR(20) NULL;

IF OBJECT_ID(N'dbo.StudentSubjectSelections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StudentSubjectSelections
    (
        StudentSubjectSelectionID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StudentSubjectSelections PRIMARY KEY,
        StudentID INT NOT NULL,
        ClassID INT NOT NULL,
        SubjectID INT NOT NULL,
        SelectionType NVARCHAR(30) NOT NULL CONSTRAINT DF_StudentSubjectSelections_Type DEFAULT(N'Registration'),
        SelectedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_StudentSubjectSelections_Selected DEFAULT(SYSUTCDATETIME()),
        CONSTRAINT UX_StudentSubjectSelections UNIQUE(StudentID,ClassID,SubjectID)
    );
END;

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.ClassSubjects') AND name=N'IX_ClassSubjects_RegistrationChoices')
    CREATE INDEX IX_ClassSubjects_RegistrationChoices ON dbo.ClassSubjects(ClassID,SubjectGroup,OptionGroupCode,ReligionEligibility,DisplayOrder);

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.StudentSubjectSelections') AND name=N'IX_StudentSubjectSelections_Student')
    CREATE INDEX IX_StudentSubjectSelections_Student ON dbo.StudentSubjectSelections(StudentID,ClassID);
