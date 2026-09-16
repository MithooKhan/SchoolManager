/*
 Digital School Manager Revision 7
 Teacher portal, Teachers Diary and student profile support.
 Run against SchoolDatabase if the application identity cannot create tables.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.PortalLoginAttempts',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.PortalLoginAttempts
 (
  LoginKey NVARCHAR(120) NOT NULL CONSTRAINT PK_PortalLoginAttempts PRIMARY KEY,
  FailedCount INT NOT NULL CONSTRAINT DF_PortalLoginAttempts_Failed DEFAULT(0),
  LockoutEndUtc DATETIME2(0) NULL,
  LastAttemptUtc DATETIME2(0) NOT NULL CONSTRAINT DF_PortalLoginAttempts_Last DEFAULT(SYSUTCDATETIME())
 );
END;

IF OBJECT_ID(N'dbo.TeacherDiaryEntries',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.TeacherDiaryEntries
 (
  DiaryEntryID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TeacherDiaryEntries PRIMARY KEY,
  TeacherID INT NOT NULL, ClassID INT NOT NULL, SubjectID INT NOT NULL,
  WeekStartDate DATE NOT NULL, LessonDate DATE NOT NULL, PeriodNo NVARCHAR(30) NULL,
  UpcomingTopics NVARCHAR(500) NOT NULL, StudyPlan NVARCHAR(1500) NULL,
  LessonPlan NVARCHAR(MAX) NULL, AssignmentText NVARCHAR(1500) NULL,
  ClassTestText NVARCHAR(1000) NULL, WorkDone NVARCHAR(1500) NULL,
  DueDate DATE NULL, VisibilityStatus NVARCHAR(20) NOT NULL
    CONSTRAINT DF_TeacherDiaryEntries_Status DEFAULT(N'Published'),
  CreatedAtUtc DATETIME2(0) NOT NULL
    CONSTRAINT DF_TeacherDiaryEntries_Created DEFAULT(SYSUTCDATETIME()),
  UpdatedAtUtc DATETIME2(0) NULL
 );
END;

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.TeacherDiaryEntries') AND name=N'IX_TeacherDiary_ClassDate')
 CREATE INDEX IX_TeacherDiary_ClassDate ON dbo.TeacherDiaryEntries(ClassID,LessonDate DESC)
 INCLUDE(SubjectID,TeacherID,VisibilityStatus,DueDate);

IF OBJECT_ID(N'dbo.StudentActivities',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.StudentActivities
 (
  ActivityID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StudentActivities PRIMARY KEY,
  StudentID INT NOT NULL, ActivityType NVARCHAR(80) NOT NULL,
  Title NVARCHAR(200) NOT NULL, ParticipationDate DATE NOT NULL,
  PositionAward NVARCHAR(120) NULL, Remarks NVARCHAR(500) NULL,
  CreatedByUserID INT NULL,
  CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_StudentActivities_Created DEFAULT(SYSUTCDATETIME())
 );
END;

IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.StudentActivities') AND name=N'IX_StudentActivities_Student')
 CREATE INDEX IX_StudentActivities_Student ON dbo.StudentActivities(StudentID,ParticipationDate DESC);

COMMIT;
