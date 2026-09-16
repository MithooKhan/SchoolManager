SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.BoardResultSessions', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.BoardResultSessions
        (
            BoardResultSessionID INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_BoardResultSessions PRIMARY KEY,
            ClassID INT NOT NULL,
            ClassNameSnapshot NVARCHAR(150) NOT NULL,
            ClassLevel TINYINT NOT NULL,
            StudyGroup NVARCHAR(100) NOT NULL
                CONSTRAINT DF_BoardResultSessions_StudyGroup DEFAULT(N'General'),
            ExamTitle NVARCHAR(160) NOT NULL,
            ExamYear SMALLINT NOT NULL,
            ResultDate DATE NOT NULL,
            BoardName NVARCHAR(160) NOT NULL,
            EMISCode NVARCHAR(30) NULL,
            BISECode NVARCHAR(30) NULL,
            HeadTeacherID INT NULL,
            HeadNameSnapshot NVARCHAR(160) NOT NULL,
            HeadDesignationSnapshot NVARCHAR(120) NULL,
            HeadScaleSnapshot NVARCHAR(30) NULL,
            HeadMobileSnapshot NVARCHAR(40) NULL,
            HeadPeriodFrom DATE NULL,
            HeadPeriodTo DATE NULL,
            RegisteredCount INT NOT NULL,
            RegisteredYear SMALLINT NOT NULL,
            PreviousAppearedCount INT NULL,
            PreviousAppearedYear SMALLINT NULL,
            AppearedCount INT NOT NULL,
            PassedCount INT NOT NULL,
            FailedCount AS (AppearedCount - PassedCount) PERSISTED,
            SchoolPassPercentage AS
            (
                CONVERT(DECIMAL(6,2),
                    CASE WHEN AppearedCount = 0 THEN 0
                         ELSE (PassedCount * 100.0) / AppearedCount END)
            ) PERSISTED,
            BoardPassPercentage DECIMAL(6,2) NOT NULL,
            GradeAPlusCount INT NOT NULL CONSTRAINT DF_BoardResultSessions_APlus DEFAULT(0),
            GradeACount INT NOT NULL CONSTRAINT DF_BoardResultSessions_A DEFAULT(0),
            GradeBCount INT NOT NULL CONSTRAINT DF_BoardResultSessions_B DEFAULT(0),
            GradeCCount INT NOT NULL CONSTRAINT DF_BoardResultSessions_C DEFAULT(0),
            GradeDCount INT NOT NULL CONSTRAINT DF_BoardResultSessions_D DEFAULT(0),
            GradeECount INT NOT NULL CONSTRAINT DF_BoardResultSessions_E DEFAULT(0),
            Remarks NVARCHAR(500) NULL,
            CreatedByUserID INT NULL,
            CreatedAtUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_BoardResultSessions_CreatedAt DEFAULT(SYSUTCDATETIME()),
            UpdatedAtUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_BoardResultSessions_UpdatedAt DEFAULT(SYSUTCDATETIME()),
            CONSTRAINT FK_BoardResultSessions_Classes
                FOREIGN KEY(ClassID) REFERENCES dbo.Classes(ClassID),
            CONSTRAINT FK_BoardResultSessions_HeadTeacher
                FOREIGN KEY(HeadTeacherID) REFERENCES dbo.Teachers(teacherid),
            CONSTRAINT CK_BoardResultSessions_ClassLevel
                CHECK(ClassLevel BETWEEN 9 AND 12),
            CONSTRAINT CK_BoardResultSessions_Counts
                CHECK
                (
                    RegisteredCount >= 0 AND AppearedCount >= 0 AND
                    PassedCount >= 0 AND PassedCount <= AppearedCount AND
                    (PreviousAppearedCount IS NULL OR PreviousAppearedCount >= 0)
                ),
            CONSTRAINT CK_BoardResultSessions_BoardPercentage
                CHECK(BoardPassPercentage BETWEEN 0 AND 100),
            CONSTRAINT CK_BoardResultSessions_Grades
                CHECK
                (
                    GradeAPlusCount >= 0 AND GradeACount >= 0 AND GradeBCount >= 0 AND
                    GradeCCount >= 0 AND GradeDCount >= 0 AND GradeECount >= 0 AND
                    GradeAPlusCount + GradeACount + GradeBCount +
                    GradeCCount + GradeDCount + GradeECount <= PassedCount
                )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.BoardResultSessions')
          AND name = N'UX_BoardResultSessions_ClassExam'
    )
        CREATE UNIQUE NONCLUSTERED INDEX UX_BoardResultSessions_ClassExam
            ON dbo.BoardResultSessions(ClassID, ExamYear, ExamTitle, StudyGroup);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.BoardResultSessions')
          AND name = N'IX_BoardResultSessions_Recent'
    )
        CREATE NONCLUSTERED INDEX IX_BoardResultSessions_Recent
            ON dbo.BoardResultSessions(ExamYear DESC, ResultDate DESC)
            INCLUDE(ClassID, ClassNameSnapshot, StudyGroup, AppearedCount, PassedCount);

    IF OBJECT_ID(N'dbo.TeacherSubjectBoardResults', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.TeacherSubjectBoardResults
        (
            TeacherSubjectResultID INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_TeacherSubjectBoardResults PRIMARY KEY,
            BoardResultSessionID INT NOT NULL,
            TeacherID INT NOT NULL,
            SubjectID INT NOT NULL,
            TeacherNameSnapshot NVARCHAR(160) NOT NULL,
            DesignationSnapshot NVARCHAR(120) NULL,
            ScaleSnapshot NVARCHAR(30) NULL,
            MobileSnapshot NVARCHAR(40) NULL,
            SubjectNameSnapshot NVARCHAR(140) NOT NULL,
            ResponsibilityFrom DATE NULL,
            ResponsibilityTo DATE NULL,
            RegisteredCount INT NOT NULL,
            PreviousAppearedCount INT NULL,
            AppearedCount INT NOT NULL,
            PassedCount INT NOT NULL,
            FailedCount AS (AppearedCount - PassedCount) PERSISTED,
            SchoolPassPercentage AS
            (
                CONVERT(DECIMAL(6,2),
                    CASE WHEN AppearedCount = 0 THEN 0
                         ELSE (PassedCount * 100.0) / AppearedCount END)
            ) PERSISTED,
            BoardPassPercentage DECIMAL(6,2) NOT NULL,
            Notes NVARCHAR(500) NULL,
            CreatedAtUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_TeacherSubjectBoardResults_CreatedAt DEFAULT(SYSUTCDATETIME()),
            UpdatedAtUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_TeacherSubjectBoardResults_UpdatedAt DEFAULT(SYSUTCDATETIME()),
            CONSTRAINT FK_TeacherSubjectBoardResults_Session
                FOREIGN KEY(BoardResultSessionID) REFERENCES dbo.BoardResultSessions(BoardResultSessionID),
            CONSTRAINT FK_TeacherSubjectBoardResults_Teacher
                FOREIGN KEY(TeacherID) REFERENCES dbo.Teachers(teacherid),
            CONSTRAINT FK_TeacherSubjectBoardResults_Subject
                FOREIGN KEY(SubjectID) REFERENCES dbo.Subjects(SubjectID),
            CONSTRAINT CK_TeacherSubjectBoardResults_Counts
                CHECK
                (
                    RegisteredCount >= 0 AND AppearedCount >= 0 AND
                    PassedCount >= 0 AND PassedCount <= AppearedCount AND
                    (PreviousAppearedCount IS NULL OR PreviousAppearedCount >= 0)
                ),
            CONSTRAINT CK_TeacherSubjectBoardResults_BoardPercentage
                CHECK(BoardPassPercentage BETWEEN 0 AND 100)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.TeacherSubjectBoardResults')
          AND name = N'UX_TeacherSubjectBoardResults_Record'
    )
        CREATE UNIQUE NONCLUSTERED INDEX UX_TeacherSubjectBoardResults_Record
            ON dbo.TeacherSubjectBoardResults(BoardResultSessionID, TeacherID, SubjectID);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.TeacherSubjectBoardResults')
          AND name = N'IX_TeacherSubjectBoardResults_Performance'
    )
        CREATE NONCLUSTERED INDEX IX_TeacherSubjectBoardResults_Performance
            ON dbo.TeacherSubjectBoardResults(BoardResultSessionID, SchoolPassPercentage)
            INCLUDE(TeacherID, SubjectID, AppearedCount, PassedCount, BoardPassPercentage);

    IF OBJECT_ID(N'dbo.BoardResultPositionHolders', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.BoardResultPositionHolders
        (
            PositionHolderID INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_BoardResultPositionHolders PRIMARY KEY,
            BoardResultSessionID INT NOT NULL,
            PositionNumber TINYINT NOT NULL,
            StudentID INT NULL,
            StudentNameSnapshot NVARCHAR(160) NOT NULL,
            FatherNameSnapshot NVARCHAR(160) NULL,
            AddressSnapshot NVARCHAR(350) NULL,
            ContactSnapshot NVARCHAR(40) NULL,
            ObtainedMarks DECIMAL(10,2) NOT NULL,
            TotalMarks DECIMAL(10,2) NOT NULL,
            Notes NVARCHAR(300) NULL,
            CreatedAtUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_BoardResultPositionHolders_CreatedAt DEFAULT(SYSUTCDATETIME()),
            UpdatedAtUtc DATETIME2(0) NOT NULL
                CONSTRAINT DF_BoardResultPositionHolders_UpdatedAt DEFAULT(SYSUTCDATETIME()),
            CONSTRAINT FK_BoardResultPositionHolders_Session
                FOREIGN KEY(BoardResultSessionID) REFERENCES dbo.BoardResultSessions(BoardResultSessionID),
            CONSTRAINT FK_BoardResultPositionHolders_Student
                FOREIGN KEY(StudentID) REFERENCES dbo.Students(StudentID),
            CONSTRAINT CK_BoardResultPositionHolders_Position
                CHECK(PositionNumber BETWEEN 1 AND 3),
            CONSTRAINT CK_BoardResultPositionHolders_Marks
                CHECK(ObtainedMarks >= 0 AND TotalMarks > 0 AND ObtainedMarks <= TotalMarks)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.BoardResultPositionHolders')
          AND name = N'UX_BoardResultPositionHolders_Position'
    )
        CREATE UNIQUE NONCLUSTERED INDEX UX_BoardResultPositionHolders_Position
            ON dbo.BoardResultPositionHolders(BoardResultSessionID, PositionNumber);

    COMMIT TRANSACTION;
    PRINT 'Board result performance database update completed successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
