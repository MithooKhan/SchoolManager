using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class BoardResultManagement : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SystemUserSecurity.IsAdministrator(Context))
            {
                Response.Redirect(ResolveUrl("~/PortalLogin.aspx?ReturnUrl=%2fBoardResultManagement.aspx"), true);
                return;
            }

            if (IsPostBack) return;

            TryAction(delegate
            {
                SchoolLifecycleService.EnsureSchema();
                StudentClassEnrollmentService.EnsureSchema();
                BoardResultService.EnsureSchema(this);
                BindClasses();
                BindTeachers();
                ResetSessionEditor();
                BindHistory();
                BindStatistics();
            }, "The board result workspace could not be initialized. Run BoardResult_Database_Update.sql and verify the SchoolDB connection.");
        }

        protected void ddlClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            int classId = SelectedInt(ddlClass);
            int classLevel = BoardResultService.GetClassLevel(ddlClass.SelectedItem == null ? string.Empty : ddlClass.SelectedItem.Text);
            ApplyClassConfiguration(classLevel, true);
            BindSubjects(classId);
            BindStudents(classId);
        }

        protected void btnNewSession_Click(object sender, EventArgs e)
        {
            HideMessage();
            ResetSessionEditor();
        }

        protected void btnClearSession_Click(object sender, EventArgs e)
        {
            HideMessage();
            ResetSessionEditor();
        }

        protected void btnSaveSession_Click(object sender, EventArgs e)
        {
            TryAction(delegate
            {
                int sessionId = HiddenInt(hfSessionID);
                int classId = SelectedInt(ddlClass);
                if (classId <= 0) throw new InvalidOperationException("Select a class.");
                string className = ddlClass.SelectedItem.Text.Trim();
                int classLevel = BoardResultService.GetClassLevel(className);
                if (classLevel < 9 || classLevel > 12)
                    throw new InvalidOperationException("Board result records are available only for Classes 9th, 10th, 11th and 12th.");
                if (sessionId > 0)
                {
                    DataRow existingSession = LoadSessionRow(sessionId);
                    int existingClassId = Convert.ToInt32(existingSession["ClassID"], CultureInfo.InvariantCulture);
                    if (existingClassId != classId)
                    {
                        DataTable related = BoardResultService.Fill(@"
SELECT
 (SELECT COUNT(*) FROM dbo.TeacherSubjectBoardResults WHERE BoardResultSessionID=@ID) +
 (SELECT COUNT(*) FROM dbo.BoardResultPositionHolders WHERE BoardResultSessionID=@ID) AS RelatedCount;",
                            P("@ID", SqlDbType.Int, sessionId));
                        if (Convert.ToInt32(related.Rows[0]["RelatedCount"], CultureInfo.InvariantCulture) > 0)
                            throw new InvalidOperationException("The class cannot be changed after teacher-subject or position-holder records have been added. Create a new result file for the other class.");
                    }
                }

                string studyGroup = Required(txtStudyGroup.Text, "field / study group", 100);
                if ((classLevel == 9 || classLevel == 10) && string.IsNullOrWhiteSpace(studyGroup))
                    studyGroup = "General";
                string examTitle = Required(txtExamTitle.Text, "examination title", 160);
                int examYear = RequiredYear(txtExamYear.Text, "result year");
                DateTime resultDate = RequiredDate(txtResultDate.Text, "result date");
                string boardName = Required(txtBoardName.Text, "board name", 160);
                int headTeacherId = SelectedInt(ddlHeadTeacher);
                if (headTeacherId <= 0) throw new InvalidOperationException("Select the responsible Head Teacher / Incharge.");
                DataRow head = LoadTeacherSnapshot(headTeacherId);
                DateTime? headFrom = OptionalDate(txtHeadPeriodFrom.Text, "head responsibility start date");
                DateTime? headTo = OptionalDate(txtHeadPeriodTo.Text, "head responsibility end date");
                ValidateDateRange(headFrom, headTo, "Head Teacher responsibility");

                int registered = NonNegativeInt(txtRegisteredCount.Text, "registered students");
                int registeredYear = RequiredYear(txtRegisteredYear.Text, "registration year");
                int? previousAppeared = null;
                int? previousYear = null;
                if (UsesPreviousClass(classLevel))
                {
                    previousAppeared = NonNegativeInt(txtPreviousAppearedCount.Text, "students appeared in the previous class");
                    previousYear = RequiredYear(txtPreviousAppearedYear.Text, "previous class result year");
                }
                int appeared = NonNegativeInt(txtAppearedCount.Text, "students appeared");
                int passed = NonNegativeInt(txtPassedCount.Text, "students passed");
                if (passed > appeared) throw new InvalidOperationException("Students passed cannot exceed students appeared.");
                decimal boardPercentage = Percentage(txtBoardPassPercentage.Text, "board pass percentage");

                int gradeAPlus = NonNegativeInt(txtGradeAPlus.Text, "A+ grade count");
                int gradeA = NonNegativeInt(txtGradeA.Text, "A grade count");
                int gradeB = NonNegativeInt(txtGradeB.Text, "B grade count");
                int gradeC = NonNegativeInt(txtGradeC.Text, "C grade count");
                int gradeD = NonNegativeInt(txtGradeD.Text, "D grade count");
                int gradeE = NonNegativeInt(txtGradeE.Text, "E grade count");
                int gradeTotal = gradeAPlus + gradeA + gradeB + gradeC + gradeD + gradeE;
                if (gradeTotal > passed)
                    throw new InvalidOperationException("The grade-wise total cannot exceed the number of students passed.");

                SqlParameter[] parameters =
                {
                    P("@ClassID", SqlDbType.Int, classId),
                    P("@ClassName", SqlDbType.NVarChar, className, 150),
                    P("@ClassLevel", SqlDbType.TinyInt, classLevel),
                    P("@StudyGroup", SqlDbType.NVarChar, studyGroup, 100),
                    P("@ExamTitle", SqlDbType.NVarChar, examTitle, 160),
                    P("@ExamYear", SqlDbType.SmallInt, examYear),
                    P("@ResultDate", SqlDbType.Date, resultDate.Date),
                    P("@BoardName", SqlDbType.NVarChar, boardName, 160),
                    P("@EMISCode", SqlDbType.NVarChar, Optional(txtEmisCode.Text, 30), 30),
                    P("@BISECode", SqlDbType.NVarChar, Optional(txtBiseCode.Text, 30), 30),
                    P("@HeadTeacherID", SqlDbType.Int, headTeacherId),
                    P("@HeadName", SqlDbType.NVarChar, Text(head, "TeacherName"), 160),
                    P("@HeadDesignation", SqlDbType.NVarChar, DbText(head, "Designation"), 120),
                    P("@HeadScale", SqlDbType.NVarChar, DbText(head, "ScaleNo"), 30),
                    P("@HeadMobile", SqlDbType.NVarChar, DbText(head, "MobileNo"), 40),
                    P("@HeadFrom", SqlDbType.Date, headFrom.HasValue ? (object)headFrom.Value.Date : DBNull.Value),
                    P("@HeadTo", SqlDbType.Date, headTo.HasValue ? (object)headTo.Value.Date : DBNull.Value),
                    P("@Registered", SqlDbType.Int, registered),
                    P("@RegisteredYear", SqlDbType.SmallInt, registeredYear),
                    P("@PreviousAppeared", SqlDbType.Int, previousAppeared.HasValue ? (object)previousAppeared.Value : DBNull.Value),
                    P("@PreviousYear", SqlDbType.SmallInt, previousYear.HasValue ? (object)previousYear.Value : DBNull.Value),
                    P("@Appeared", SqlDbType.Int, appeared),
                    P("@Passed", SqlDbType.Int, passed),
                    P("@BoardPercentage", SqlDbType.Decimal, boardPercentage, 0, 6, 2),
                    P("@APlus", SqlDbType.Int, gradeAPlus),
                    P("@A", SqlDbType.Int, gradeA),
                    P("@B", SqlDbType.Int, gradeB),
                    P("@C", SqlDbType.Int, gradeC),
                    P("@D", SqlDbType.Int, gradeD),
                    P("@E", SqlDbType.Int, gradeE),
                    P("@Remarks", SqlDbType.NVarChar, Optional(txtSessionRemarks.Text, 500), 500),
                    P("@UserID", SqlDbType.Int, CurrentUserId())
                };

                if (sessionId <= 0)
                {
                    const string insert = @"
INSERT dbo.BoardResultSessions
(
 ClassID,ClassNameSnapshot,ClassLevel,StudyGroup,ExamTitle,ExamYear,ResultDate,BoardName,
 EMISCode,BISECode,HeadTeacherID,HeadNameSnapshot,HeadDesignationSnapshot,HeadScaleSnapshot,
 HeadMobileSnapshot,HeadPeriodFrom,HeadPeriodTo,RegisteredCount,RegisteredYear,
 PreviousAppearedCount,PreviousAppearedYear,AppearedCount,PassedCount,BoardPassPercentage,
 GradeAPlusCount,GradeACount,GradeBCount,GradeCCount,GradeDCount,GradeECount,Remarks,CreatedByUserID
)
VALUES
(
 @ClassID,@ClassName,@ClassLevel,@StudyGroup,@ExamTitle,@ExamYear,@ResultDate,@BoardName,
 @EMISCode,@BISECode,@HeadTeacherID,@HeadName,@HeadDesignation,@HeadScale,@HeadMobile,
 @HeadFrom,@HeadTo,@Registered,@RegisteredYear,@PreviousAppeared,@PreviousYear,@Appeared,
 @Passed,@BoardPercentage,@APlus,@A,@B,@C,@D,@E,@Remarks,@UserID
);
SELECT CONVERT(INT,SCOPE_IDENTITY());";
                    sessionId = BoardResultService.ExecuteIdentity(insert, parameters);
                }
                else
                {
                    const string update = @"
UPDATE dbo.BoardResultSessions SET
 ClassID=@ClassID,ClassNameSnapshot=@ClassName,ClassLevel=@ClassLevel,StudyGroup=@StudyGroup,
 ExamTitle=@ExamTitle,ExamYear=@ExamYear,ResultDate=@ResultDate,BoardName=@BoardName,
 EMISCode=@EMISCode,BISECode=@BISECode,HeadTeacherID=@HeadTeacherID,HeadNameSnapshot=@HeadName,
 HeadDesignationSnapshot=@HeadDesignation,HeadScaleSnapshot=@HeadScale,HeadMobileSnapshot=@HeadMobile,
 HeadPeriodFrom=@HeadFrom,HeadPeriodTo=@HeadTo,RegisteredCount=@Registered,RegisteredYear=@RegisteredYear,
 PreviousAppearedCount=@PreviousAppeared,PreviousAppearedYear=@PreviousYear,AppearedCount=@Appeared,
 PassedCount=@Passed,BoardPassPercentage=@BoardPercentage,GradeAPlusCount=@APlus,GradeACount=@A,
 GradeBCount=@B,GradeCCount=@C,GradeDCount=@D,GradeECount=@E,Remarks=@Remarks,UpdatedAtUtc=SYSUTCDATETIME()
WHERE BoardResultSessionID=@SessionID;";
                    SqlParameter[] updateParameters = Add(parameters, P("@SessionID", SqlDbType.Int, sessionId));
                    if (BoardResultService.Execute(update, updateParameters) == 0)
                        throw new InvalidOperationException("The selected result file was not found.");
                }

                LoadSession(sessionId);
                BindHistory();
                BindStatistics();
                ShowMessage("The board result file was saved. Add or update the teacher-subject entries below.", true);
            }, "The board result file could not be saved.");
        }

        protected void btnSaveSubjectResult_Click(object sender, EventArgs e)
        {
            TryAction(delegate
            {
                int sessionId = HiddenInt(hfSessionID);
                if (sessionId <= 0) throw new InvalidOperationException("Save or open a result file before adding subject results.");
                DataRow session = LoadSessionRow(sessionId);
                int classLevel = Convert.ToInt32(session["ClassLevel"], CultureInfo.InvariantCulture);
                int teacherId = SelectedInt(ddlSubjectTeacher);
                int subjectId = SelectedInt(ddlSubject);
                if (teacherId <= 0) throw new InvalidOperationException("Select the responsible teacher.");
                if (subjectId <= 0) throw new InvalidOperationException("Select a subject.");
                DataRow teacher = LoadTeacherSnapshot(teacherId);
                DateTime? periodFrom = OptionalDate(txtResponsibilityFrom.Text, "teacher responsibility start date");
                DateTime? periodTo = OptionalDate(txtResponsibilityTo.Text, "teacher responsibility end date");
                ValidateDateRange(periodFrom, periodTo, "Teacher responsibility");

                int registered = NonNegativeInt(txtSubjectRegistered.Text, "subject registered students");
                int? previousAppeared = UsesPreviousClass(classLevel)
                    ? (int?)NonNegativeInt(txtSubjectPreviousAppeared.Text, "subject previous-class appeared students")
                    : null;
                int appeared = NonNegativeInt(txtSubjectAppeared.Text, "subject appeared students");
                int passed = NonNegativeInt(txtSubjectPassed.Text, "subject passed students");
                if (passed > appeared) throw new InvalidOperationException("Subject passed students cannot exceed appeared students.");
                decimal boardPercentage = Percentage(txtSubjectBoardPercentage.Text, "subject board percentage");
                int resultId = HiddenInt(hfSubjectResultID);

                SqlParameter[] parameters =
                {
                    P("@SessionID", SqlDbType.Int, sessionId),
                    P("@TeacherID", SqlDbType.Int, teacherId),
                    P("@SubjectID", SqlDbType.Int, subjectId),
                    P("@TeacherName", SqlDbType.NVarChar, Text(teacher, "TeacherName"), 160),
                    P("@Designation", SqlDbType.NVarChar, DbText(teacher, "Designation"), 120),
                    P("@Scale", SqlDbType.NVarChar, DbText(teacher, "ScaleNo"), 30),
                    P("@Mobile", SqlDbType.NVarChar, DbText(teacher, "MobileNo"), 40),
                    P("@SubjectName", SqlDbType.NVarChar, ddlSubject.SelectedItem.Text.Trim(), 140),
                    P("@PeriodFrom", SqlDbType.Date, periodFrom.HasValue ? (object)periodFrom.Value.Date : DBNull.Value),
                    P("@PeriodTo", SqlDbType.Date, periodTo.HasValue ? (object)periodTo.Value.Date : DBNull.Value),
                    P("@Registered", SqlDbType.Int, registered),
                    P("@PreviousAppeared", SqlDbType.Int, previousAppeared.HasValue ? (object)previousAppeared.Value : DBNull.Value),
                    P("@Appeared", SqlDbType.Int, appeared),
                    P("@Passed", SqlDbType.Int, passed),
                    P("@BoardPercentage", SqlDbType.Decimal, boardPercentage, 0, 6, 2),
                    P("@Notes", SqlDbType.NVarChar, Optional(txtSubjectNotes.Text, 500), 500)
                };

                if (resultId <= 0)
                {
                    const string insert = @"
INSERT dbo.TeacherSubjectBoardResults
(BoardResultSessionID,TeacherID,SubjectID,TeacherNameSnapshot,DesignationSnapshot,ScaleSnapshot,
 MobileSnapshot,SubjectNameSnapshot,ResponsibilityFrom,ResponsibilityTo,RegisteredCount,
 PreviousAppearedCount,AppearedCount,PassedCount,BoardPassPercentage,Notes)
VALUES
(@SessionID,@TeacherID,@SubjectID,@TeacherName,@Designation,@Scale,@Mobile,@SubjectName,
 @PeriodFrom,@PeriodTo,@Registered,@PreviousAppeared,@Appeared,@Passed,@BoardPercentage,@Notes);";
                    BoardResultService.Execute(insert, parameters);
                }
                else
                {
                    const string update = @"
UPDATE dbo.TeacherSubjectBoardResults SET
 TeacherID=@TeacherID,SubjectID=@SubjectID,TeacherNameSnapshot=@TeacherName,
 DesignationSnapshot=@Designation,ScaleSnapshot=@Scale,MobileSnapshot=@Mobile,
 SubjectNameSnapshot=@SubjectName,ResponsibilityFrom=@PeriodFrom,ResponsibilityTo=@PeriodTo,
 RegisteredCount=@Registered,PreviousAppearedCount=@PreviousAppeared,AppearedCount=@Appeared,
 PassedCount=@Passed,BoardPassPercentage=@BoardPercentage,Notes=@Notes,UpdatedAtUtc=SYSUTCDATETIME()
WHERE TeacherSubjectResultID=@ResultID AND BoardResultSessionID=@SessionID;";
                    if (BoardResultService.Execute(update, Add(parameters, P("@ResultID", SqlDbType.Int, resultId))) == 0)
                        throw new InvalidOperationException("The selected teacher-subject result was not found.");
                }

                ClearSubjectEditor();
                BindSubjectResults(sessionId);
                BindHistory();
                BindStatistics();
                ShowMessage("The teacher and subject-wise result was saved.", true);
            }, "The teacher and subject-wise result could not be saved.");
        }

        protected void btnCancelSubjectEdit_Click(object sender, EventArgs e)
        {
            HideMessage();
            ClearSubjectEditor();
        }

        protected void gvSubjectResults_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "EditResult", StringComparison.OrdinalIgnoreCase)) return;
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id) || id <= 0) return;
            TryAction(delegate { LoadSubjectEditor(id); }, "The selected subject result could not be loaded.");
        }

        protected void btnSavePosition_Click(object sender, EventArgs e)
        {
            TryAction(delegate
            {
                int sessionId = HiddenInt(hfSessionID);
                if (sessionId <= 0) throw new InvalidOperationException("Save or open a result file before adding a position holder.");
                int studentId = SelectedInt(ddlPositionStudent);
                if (studentId <= 0) throw new InvalidOperationException("Select a student.");
                int position = SelectedInt(ddlPositionNumber);
                decimal obtained = NonNegativeDecimal(txtPositionObtained.Text, "obtained marks");
                decimal total = PositiveDecimal(txtPositionTotal.Text, "total marks");
                if (obtained > total) throw new InvalidOperationException("Obtained marks cannot exceed total marks.");
                DataRow student = LoadStudentSnapshot(studentId);

                const string sql = @"
IF EXISTS(SELECT 1 FROM dbo.BoardResultPositionHolders WHERE BoardResultSessionID=@SessionID AND PositionNumber=@Position)
 UPDATE dbo.BoardResultPositionHolders SET
  StudentID=@StudentID,StudentNameSnapshot=@StudentName,FatherNameSnapshot=@FatherName,
  AddressSnapshot=@Address,ContactSnapshot=@Contact,ObtainedMarks=@Obtained,TotalMarks=@Total,
  Notes=@Notes,UpdatedAtUtc=SYSUTCDATETIME()
 WHERE BoardResultSessionID=@SessionID AND PositionNumber=@Position;
ELSE
 INSERT dbo.BoardResultPositionHolders
 (BoardResultSessionID,PositionNumber,StudentID,StudentNameSnapshot,FatherNameSnapshot,
  AddressSnapshot,ContactSnapshot,ObtainedMarks,TotalMarks,Notes)
 VALUES(@SessionID,@Position,@StudentID,@StudentName,@FatherName,@Address,@Contact,@Obtained,@Total,@Notes);";
                BoardResultService.Execute(sql,
                    P("@SessionID", SqlDbType.Int, sessionId),
                    P("@Position", SqlDbType.TinyInt, position),
                    P("@StudentID", SqlDbType.Int, studentId),
                    P("@StudentName", SqlDbType.NVarChar, Text(student, "StudentName"), 160),
                    P("@FatherName", SqlDbType.NVarChar, DbText(student, "FatherName"), 160),
                    P("@Address", SqlDbType.NVarChar, DbText(student, "Address"), 350),
                    P("@Contact", SqlDbType.NVarChar, DbText(student, "ContactNo"), 40),
                    P("@Obtained", SqlDbType.Decimal, obtained, 0, 10, 2),
                    P("@Total", SqlDbType.Decimal, total, 0, 10, 2),
                    P("@Notes", SqlDbType.NVarChar, Optional(txtPositionNotes.Text, 300), 300));

                ClearPositionEditor();
                BindPositionHolders(sessionId);
                ShowMessage("The position holder was saved.", true);
            }, "The position holder could not be saved.");
        }

        protected void gvPositionHolders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "EditPosition", StringComparison.OrdinalIgnoreCase)) return;
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id) || id <= 0) return;
            TryAction(delegate { LoadPositionEditor(id); }, "The selected position holder could not be loaded.");
        }

        protected void btnSearchHistory_Click(object sender, EventArgs e)
        {
            TryAction(delegate { BindHistory(); }, "The saved result files could not be searched.");
        }

        protected void gvSessions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "OpenSession", StringComparison.OrdinalIgnoreCase)) return;
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id) || id <= 0) return;
            TryAction(delegate
            {
                LoadSession(id);
                ShowMessage("The result file is open for review, editing and printing.", true);
            }, "The selected result file could not be opened.");
        }

        protected string GetBoardReportUrl(object sessionId)
        {
            return "BoardResultReport.aspx?sessionId=" +
                Convert.ToString(sessionId, CultureInfo.InvariantCulture) + "&view=complete";
        }

        private void BindClasses()
        {
            DataTable source = BoardResultService.Fill("SELECT ClassID,ClassName FROM dbo.Classes ORDER BY ClassName;");
            DataTable boardClasses = source.Clone();
            foreach (DataRow row in source.Rows)
                if (BoardResultService.GetClassLevel(Convert.ToString(row["ClassName"], CultureInfo.InvariantCulture)) >= 9)
                    boardClasses.ImportRow(row);
            BindList(ddlClass, boardClasses, "ClassName", "ClassID", "Select Class 9th to 12th", "0");
            BindList(ddlHistoryClass, boardClasses, "ClassName", "ClassID", "All board classes", "0");
        }

        private void BindTeachers()
        {
            const string sql = @"
SELECT t.teacherid AS TeacherID,
       ISNULL(t.Name,N'Teacher') +
       CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(v.Description,N''))),N'') IS NULL
            THEN N'' ELSE N' - ' + v.Description END AS TeacherDisplay
FROM dbo.Teachers t
LEFT JOIN dbo.TeachingVacancyPosition v ON v.postID=t.postID
WHERE ISNULL(t.IsActive,1)=1
ORDER BY t.Name,t.teacherid;";
            DataTable table = BoardResultService.Fill(sql);
            BindList(ddlHeadTeacher, table, "TeacherDisplay", "TeacherID", "Select Head Teacher / Incharge", "0");
            BindList(ddlSubjectTeacher, table.Copy(), "TeacherDisplay", "TeacherID", "Select Teacher", "0");
        }

        private void BindSubjects(int classId)
        {
            const string sql = @"
SELECT DISTINCT s.SubjectID,s.SubjectName
FROM dbo.Subjects s
WHERE @ClassID=0
   OR EXISTS(SELECT 1 FROM dbo.ClassSubjects cs WHERE cs.ClassID=@ClassID AND cs.SubjectID=s.SubjectID)
   OR NOT EXISTS(SELECT 1 FROM dbo.ClassSubjects cs0 WHERE cs0.ClassID=@ClassID)
ORDER BY s.SubjectName;";
            DataTable table = BoardResultService.Fill(sql, P("@ClassID", SqlDbType.Int, classId));
            BindList(ddlSubject, table, "SubjectName", "SubjectID", "Select Subject", "0");
        }

        private void BindStudents(int classId)
        {
            const string sql = @"
SELECT s.StudentID,
       ISNULL(s.Name,N'Student') +
       CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(s.FatherName,N''))),N'') IS NULL
            THEN N'' ELSE N' - Father: ' + s.FatherName END AS StudentDisplay
FROM dbo.Students s
WHERE s.CurrentClassID=@ClassID
  AND LTRIM(RTRIM(ISNULL(s.Isactive,N'Active'))) IN(N'Active',N'True',N'1')
ORDER BY s.Name,s.StudentID;";
            DataTable table = classId <= 0 ? new DataTable() :
                BoardResultService.Fill(sql, P("@ClassID", SqlDbType.Int, classId));
            if (classId <= 0)
            {
                table.Columns.Add("StudentID", typeof(int));
                table.Columns.Add("StudentDisplay", typeof(string));
            }
            BindList(ddlPositionStudent, table, "StudentDisplay", "StudentID", "Select Student", "0");
        }

        private void BindStatistics()
        {
            const string sql = @"
SELECT
 (SELECT COUNT(*) FROM dbo.BoardResultSessions) AS SessionCount,
 (SELECT COUNT(*) FROM dbo.TeacherSubjectBoardResults) AS SubjectCount,
 (SELECT COUNT(*) FROM dbo.TeacherSubjectBoardResults WHERE SchoolPassPercentage>BoardPassPercentage) AS AboveBoardCount,
 (SELECT MAX(ExamYear) FROM dbo.BoardResultSessions) AS LatestYear;";
            DataTable table = BoardResultService.Fill(sql);
            DataRow row = table.Rows[0];
            lblSessionCount.Text = Text(row, "SessionCount");
            lblSubjectRecordCount.Text = Text(row, "SubjectCount");
            lblAboveBoardCount.Text = Text(row, "AboveBoardCount");
            lblLatestYear.Text = row["LatestYear"] == DBNull.Value ? "-" : Text(row, "LatestYear");
        }

        private void BindHistory()
        {
            int classId = SelectedInt(ddlHistoryClass);
            int year;
            if (!int.TryParse((txtHistoryYear.Text ?? string.Empty).Trim(), out year)) year = 0;
            string search = (txtHistorySearch.Text ?? string.Empty).Trim();
            const string sql = @"
SELECT r.BoardResultSessionID,r.ExamYear,r.ClassNameSnapshot,r.StudyGroup,r.ExamTitle,
       r.HeadNameSnapshot,r.AppearedCount,r.SchoolPassPercentage,
       (SELECT COUNT(*) FROM dbo.TeacherSubjectBoardResults t WHERE t.BoardResultSessionID=r.BoardResultSessionID) AS SubjectCount
FROM dbo.BoardResultSessions r
WHERE (@ClassID=0 OR r.ClassID=@ClassID)
  AND (@ExamYear=0 OR r.ExamYear=@ExamYear)
  AND
  (
    @Search=N'' OR r.ExamTitle LIKE @Pattern OR r.StudyGroup LIKE @Pattern OR
    r.HeadNameSnapshot LIKE @Pattern OR
    EXISTS
    (
      SELECT 1 FROM dbo.TeacherSubjectBoardResults ts
      WHERE ts.BoardResultSessionID=r.BoardResultSessionID
        AND (ts.TeacherNameSnapshot LIKE @Pattern OR ts.SubjectNameSnapshot LIKE @Pattern)
    )
  )
ORDER BY r.ExamYear DESC,r.ResultDate DESC,r.ClassNameSnapshot,r.StudyGroup;";
            gvSessions.DataSource = BoardResultService.Fill(sql,
                P("@ClassID", SqlDbType.Int, classId),
                P("@ExamYear", SqlDbType.SmallInt, year),
                P("@Search", SqlDbType.NVarChar, search, 100),
                P("@Pattern", SqlDbType.NVarChar, "%" + search + "%", 220));
            gvSessions.DataBind();
        }

        private void LoadSession(int sessionId)
        {
            DataRow row = LoadSessionRow(sessionId);
            hfSessionID.Value = sessionId.ToString(CultureInfo.InvariantCulture);
            SelectValue(ddlClass, Text(row, "ClassID"));
            int classId = Convert.ToInt32(row["ClassID"], CultureInfo.InvariantCulture);
            int classLevel = Convert.ToInt32(row["ClassLevel"], CultureInfo.InvariantCulture);
            ApplyClassConfiguration(classLevel, false);
            BindSubjects(classId);
            BindStudents(classId);
            txtStudyGroup.Text = Text(row, "StudyGroup");
            txtExamTitle.Text = Text(row, "ExamTitle");
            txtExamYear.Text = Text(row, "ExamYear");
            txtResultDate.Text = DateText(row, "ResultDate");
            txtBoardName.Text = Text(row, "BoardName");
            txtEmisCode.Text = DbText(row, "EMISCode");
            txtBiseCode.Text = DbText(row, "BISECode");
            SelectValue(ddlHeadTeacher, DbText(row, "HeadTeacherID"));
            txtHeadPeriodFrom.Text = DateText(row, "HeadPeriodFrom");
            txtHeadPeriodTo.Text = DateText(row, "HeadPeriodTo");
            txtRegisteredCount.Text = Text(row, "RegisteredCount");
            txtRegisteredYear.Text = Text(row, "RegisteredYear");
            txtPreviousAppearedCount.Text = DbText(row, "PreviousAppearedCount");
            txtPreviousAppearedYear.Text = DbText(row, "PreviousAppearedYear");
            txtAppearedCount.Text = Text(row, "AppearedCount");
            txtPassedCount.Text = Text(row, "PassedCount");
            txtBoardPassPercentage.Text = Convert.ToDecimal(row["BoardPassPercentage"], CultureInfo.InvariantCulture).ToString("0.00", CultureInfo.InvariantCulture);
            txtGradeAPlus.Text = Text(row, "GradeAPlusCount");
            txtGradeA.Text = Text(row, "GradeACount");
            txtGradeB.Text = Text(row, "GradeBCount");
            txtGradeC.Text = Text(row, "GradeCCount");
            txtGradeD.Text = Text(row, "GradeDCount");
            txtGradeE.Text = Text(row, "GradeECount");
            txtSessionRemarks.Text = DbText(row, "Remarks");
            lblSessionEditorTitle.Text = "Edit annual board result file";

            pnlSessionWorkspace.Visible = true;
            lblWorkspaceTitle.Text = Text(row, "ClassNameSnapshot") + " - " + Text(row, "ExamTitle") + " " + Text(row, "ExamYear");
            lblWorkspaceMeta.Text = Text(row, "StudyGroup") + " | School " +
                Convert.ToDecimal(row["SchoolPassPercentage"], CultureInfo.InvariantCulture).ToString("0.00", CultureInfo.InvariantCulture) +
                "% | Board " + Convert.ToDecimal(row["BoardPassPercentage"], CultureInfo.InvariantCulture).ToString("0.00", CultureInfo.InvariantCulture) + "%";
            string baseUrl = "BoardResultReport.aspx?sessionId=" + sessionId.ToString(CultureInfo.InvariantCulture) + "&view=";
            lnkHeadReport.NavigateUrl = baseUrl + "head";
            lnkTeacherReport.NavigateUrl = baseUrl + "teacher";
            lnkCompleteReport.NavigateUrl = baseUrl + "complete";
            BindSubjectResults(sessionId);
            BindPositionHolders(sessionId);
            ClearSubjectEditor();
            ClearPositionEditor();
        }

        private DataRow LoadSessionRow(int sessionId)
        {
            DataTable table = BoardResultService.Fill(
                "SELECT * FROM dbo.BoardResultSessions WHERE BoardResultSessionID=@ID;",
                P("@ID", SqlDbType.Int, sessionId));
            if (table.Rows.Count == 0) throw new InvalidOperationException("The selected result file was not found.");
            return table.Rows[0];
        }

        private void BindSubjectResults(int sessionId)
        {
            const string sql = @"
SELECT TeacherSubjectResultID,SubjectNameSnapshot,TeacherNameSnapshot,DesignationSnapshot,
       AppearedCount,PassedCount,FailedCount,SchoolPassPercentage,BoardPassPercentage,
       CASE WHEN SchoolPassPercentage>BoardPassPercentage THEN N'Above Board'
            WHEN SchoolPassPercentage<BoardPassPercentage THEN N'Below Board'
            ELSE N'Equal to Board' END AS PerformanceStatus,
       CASE WHEN SchoolPassPercentage>BoardPassPercentage THEN N'above'
            WHEN SchoolPassPercentage<BoardPassPercentage THEN N'below'
            ELSE N'equal' END AS PerformanceCss
FROM dbo.TeacherSubjectBoardResults
WHERE BoardResultSessionID=@SessionID
ORDER BY SubjectNameSnapshot,TeacherNameSnapshot;";
            gvSubjectResults.DataSource = BoardResultService.Fill(sql, P("@SessionID", SqlDbType.Int, sessionId));
            gvSubjectResults.DataBind();
        }

        private void BindPositionHolders(int sessionId)
        {
            const string sql = @"
SELECT PositionHolderID,
       CASE PositionNumber WHEN 1 THEN N'1st' WHEN 2 THEN N'2nd' ELSE N'3rd' END AS PositionLabel,
       StudentNameSnapshot,FatherNameSnapshot,ObtainedMarks,TotalMarks,
       CONVERT(DECIMAL(6,2),ObtainedMarks*100.0/NULLIF(TotalMarks,0)) AS Percentage
FROM dbo.BoardResultPositionHolders
WHERE BoardResultSessionID=@SessionID
ORDER BY PositionNumber;";
            gvPositionHolders.DataSource = BoardResultService.Fill(sql, P("@SessionID", SqlDbType.Int, sessionId));
            gvPositionHolders.DataBind();
        }

        private void LoadSubjectEditor(int resultId)
        {
            DataTable table = BoardResultService.Fill(
                "SELECT * FROM dbo.TeacherSubjectBoardResults WHERE TeacherSubjectResultID=@ID AND BoardResultSessionID=@SessionID;",
                P("@ID", SqlDbType.Int, resultId), P("@SessionID", SqlDbType.Int, HiddenInt(hfSessionID)));
            if (table.Rows.Count == 0) throw new InvalidOperationException("The selected subject result was not found.");
            DataRow row = table.Rows[0];
            hfSubjectResultID.Value = resultId.ToString(CultureInfo.InvariantCulture);
            SelectValue(ddlSubjectTeacher, Text(row, "TeacherID"));
            SelectValue(ddlSubject, Text(row, "SubjectID"));
            txtResponsibilityFrom.Text = DateText(row, "ResponsibilityFrom");
            txtResponsibilityTo.Text = DateText(row, "ResponsibilityTo");
            txtSubjectRegistered.Text = Text(row, "RegisteredCount");
            txtSubjectPreviousAppeared.Text = DbText(row, "PreviousAppearedCount");
            txtSubjectAppeared.Text = Text(row, "AppearedCount");
            txtSubjectPassed.Text = Text(row, "PassedCount");
            txtSubjectBoardPercentage.Text = Convert.ToDecimal(row["BoardPassPercentage"], CultureInfo.InvariantCulture).ToString("0.00", CultureInfo.InvariantCulture);
            txtSubjectNotes.Text = DbText(row, "Notes");
            lblSubjectEditorTitle.Text = "Edit teacher and subject result";
            btnSaveSubjectResult.Text = "Update Subject Result";
            btnCancelSubjectEdit.Visible = true;
        }

        private void LoadPositionEditor(int positionHolderId)
        {
            DataTable table = BoardResultService.Fill(
                "SELECT * FROM dbo.BoardResultPositionHolders WHERE PositionHolderID=@ID AND BoardResultSessionID=@SessionID;",
                P("@ID", SqlDbType.Int, positionHolderId), P("@SessionID", SqlDbType.Int, HiddenInt(hfSessionID)));
            if (table.Rows.Count == 0) throw new InvalidOperationException("The selected position holder was not found.");
            DataRow row = table.Rows[0];
            string studentId = DbText(row, "StudentID");
            if (!string.IsNullOrEmpty(studentId) && ddlPositionStudent.Items.FindByValue(studentId) == null)
                ddlPositionStudent.Items.Add(new ListItem(Text(row, "StudentNameSnapshot") + " - historical record", studentId));
            SelectValue(ddlPositionStudent, studentId);
            SelectValue(ddlPositionNumber, Text(row, "PositionNumber"));
            txtPositionObtained.Text = Convert.ToDecimal(row["ObtainedMarks"], CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture);
            txtPositionTotal.Text = Convert.ToDecimal(row["TotalMarks"], CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture);
            txtPositionNotes.Text = DbText(row, "Notes");
        }

        private DataRow LoadTeacherSnapshot(int teacherId)
        {
            const string sql = @"
SELECT t.teacherid AS TeacherID,ISNULL(t.Name,N'') AS TeacherName,
       ISNULL(v.Description,N'') AS Designation,
       ISNULL(CONVERT(NVARCHAR(30),v.BPS),N'') AS ScaleNo,
       ISNULL(t.contactno,N'') AS MobileNo
FROM dbo.Teachers t
LEFT JOIN dbo.TeachingVacancyPosition v ON v.postID=t.postID
WHERE t.teacherid=@ID;";
            DataTable table = BoardResultService.Fill(sql, P("@ID", SqlDbType.Int, teacherId));
            if (table.Rows.Count == 0) throw new InvalidOperationException("The selected teacher record was not found.");
            return table.Rows[0];
        }

        private DataRow LoadStudentSnapshot(int studentId)
        {
            const string sql = @"
SELECT StudentID,ISNULL(Name,N'') AS StudentName,ISNULL(FatherName,N'') AS FatherName,
       ISNULL(Address,N'') AS Address,ISNULL(ContactNo,N'') AS ContactNo
FROM dbo.Students WHERE StudentID=@ID;";
            DataTable table = BoardResultService.Fill(sql, P("@ID", SqlDbType.Int, studentId));
            if (table.Rows.Count == 0) throw new InvalidOperationException("The selected student record was not found.");
            return table.Rows[0];
        }

        private void ResetSessionEditor()
        {
            hfSessionID.Value = string.Empty;
            lblSessionEditorTitle.Text = "Create annual board result file";
            if (ddlClass.Items.Count > 0) ddlClass.SelectedIndex = 0;
            txtStudyGroup.Text = "General";
            txtExamTitle.Text = "Annual Examination";
            txtExamYear.Text = DateTime.Today.Year.ToString(CultureInfo.InvariantCulture);
            txtResultDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            txtBoardName.Text = "BISE Multan";
            txtEmisCode.Text = "36410027";
            txtBiseCode.Text = "101057";
            if (ddlHeadTeacher.Items.Count > 0) ddlHeadTeacher.SelectedIndex = 0;
            txtHeadPeriodFrom.Text = string.Empty;
            txtHeadPeriodTo.Text = string.Empty;
            txtRegisteredCount.Text = "0";
            txtRegisteredYear.Text = (DateTime.Today.Year - 1).ToString(CultureInfo.InvariantCulture);
            txtPreviousAppearedCount.Text = string.Empty;
            txtPreviousAppearedYear.Text = (DateTime.Today.Year - 1).ToString(CultureInfo.InvariantCulture);
            txtAppearedCount.Text = "0";
            txtPassedCount.Text = "0";
            txtBoardPassPercentage.Text = "0";
            txtGradeAPlus.Text = txtGradeA.Text = txtGradeB.Text = txtGradeC.Text = txtGradeD.Text = txtGradeE.Text = "0";
            txtSessionRemarks.Text = string.Empty;
            pnlPreviousCohort.Visible = pnlPreviousYear.Visible = pnlSubjectPrevious.Visible = false;
            lblClassGuidance.Text = "Only Classes 9th to 12th are listed.";
            pnlSessionWorkspace.Visible = false;
            BindSubjects(0);
            BindStudents(0);
        }

        private void ApplyClassConfiguration(int classLevel, bool setDefaultYears)
        {
            bool previous = UsesPreviousClass(classLevel);
            pnlPreviousCohort.Visible = previous;
            pnlPreviousYear.Visible = previous;
            pnlSubjectPrevious.Visible = previous;
            lblPreviousAppearedCaption.Text = classLevel > 0
                ? "Students appeared in " + (classLevel - 1).ToString(CultureInfo.InvariantCulture) + "th"
                : "Previous class appeared";
            lblClassGuidance.Text = classLevel > 0
                ? "Class level " + classLevel.ToString(CultureInfo.InvariantCulture) + " selected. " +
                  (previous ? "The previous-class cohort figures are required." : "No previous-class cohort column is required.")
                : "Only Classes 9th to 12th are listed.";
            if (!setDefaultYears) return;
            int examYear;
            if (!int.TryParse(txtExamYear.Text, out examYear)) examYear = DateTime.Today.Year;
            txtRegisteredYear.Text = (examYear - (previous ? 2 : 1)).ToString(CultureInfo.InvariantCulture);
            txtPreviousAppearedYear.Text = (examYear - 1).ToString(CultureInfo.InvariantCulture);
        }

        private void ClearSubjectEditor()
        {
            hfSubjectResultID.Value = string.Empty;
            lblSubjectEditorTitle.Text = "Add teacher and subject result";
            btnSaveSubjectResult.Text = "Save Subject Result";
            btnCancelSubjectEdit.Visible = false;
            if (ddlSubjectTeacher.Items.Count > 0) ddlSubjectTeacher.SelectedIndex = 0;
            if (ddlSubject.Items.Count > 0) ddlSubject.SelectedIndex = 0;
            txtResponsibilityFrom.Text = txtHeadPeriodFrom.Text;
            txtResponsibilityTo.Text = txtHeadPeriodTo.Text;
            txtSubjectRegistered.Text = string.IsNullOrWhiteSpace(txtRegisteredCount.Text) ? "0" : txtRegisteredCount.Text;
            txtSubjectPreviousAppeared.Text = txtPreviousAppearedCount.Text;
            txtSubjectAppeared.Text = string.IsNullOrWhiteSpace(txtAppearedCount.Text) ? "0" : txtAppearedCount.Text;
            txtSubjectPassed.Text = "0";
            txtSubjectBoardPercentage.Text = "0";
            txtSubjectNotes.Text = string.Empty;
        }

        private void ClearPositionEditor()
        {
            if (ddlPositionStudent.Items.Count > 0) ddlPositionStudent.SelectedIndex = 0;
            ddlPositionNumber.SelectedValue = "1";
            txtPositionObtained.Text = string.Empty;
            txtPositionTotal.Text = string.Empty;
            txtPositionNotes.Text = string.Empty;
        }

        private static bool UsesPreviousClass(int classLevel)
        {
            return classLevel == 10 || classLevel == 12;
        }

        private static void BindList(ListControl control, DataTable table, string textField, string valueField, string prompt, string promptValue)
        {
            control.Items.Clear();
            control.DataSource = table;
            control.DataTextField = textField;
            control.DataValueField = valueField;
            control.DataBind();
            control.Items.Insert(0, new ListItem(prompt, promptValue));
        }

        private void TryAction(Action action, string fallback)
        {
            try
            {
                BoardResultService.EnsureSchema(this);
                action();
            }
            catch (SqlException ex)
            {
                string message = ex.Number == 2601 || ex.Number == 2627
                    ? "A result file or teacher-subject entry with the same class, year, examination and group already exists. Open the saved record and update it."
                    : fallback + " " + ex.Message;
                ShowMessage(message, false);
            }
            catch (Exception ex)
            {
                ShowMessage(fallback + " " + ex.Message, false);
            }
        }

        private void ShowMessage(string message, bool success)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "br-message " + (success ? "success" : "error");
            lblMessage.Text = Server.HtmlEncode(message);
        }

        private void HideMessage()
        {
            pnlMessage.Visible = false;
            lblMessage.Text = string.Empty;
        }

        private int CurrentUserId()
        {
            int value;
            return Session != null && int.TryParse(Convert.ToString(Session["SystemUserID"], CultureInfo.InvariantCulture), out value) && value > 0 ? value : 0;
        }

        private static int SelectedInt(ListControl control)
        {
            int value;
            return int.TryParse(control.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;
        }

        private static int HiddenInt(HiddenField field)
        {
            int value;
            return int.TryParse(field.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;
        }

        private static int NonNegativeInt(string value, string label)
        {
            int number;
            if (!int.TryParse((value ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number) || number < 0)
                throw new InvalidOperationException("Enter a valid non-negative value for " + label + ".");
            return number;
        }

        private static int RequiredYear(string value, string label)
        {
            int year;
            if (!int.TryParse((value ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out year) || year < 2000 || year > 2100)
                throw new InvalidOperationException("Enter a valid " + label + " between 2000 and 2100.");
            return year;
        }

        private static decimal Percentage(string value, string label)
        {
            decimal number = NonNegativeDecimal(value, label);
            if (number > 100) throw new InvalidOperationException("The " + label + " cannot exceed 100.");
            return decimal.Round(number, 2);
        }

        private static decimal NonNegativeDecimal(string value, string label)
        {
            decimal number;
            if (!decimal.TryParse((value ?? string.Empty).Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out number) || number < 0)
                throw new InvalidOperationException("Enter a valid non-negative value for " + label + ".");
            return number;
        }

        private static decimal PositiveDecimal(string value, string label)
        {
            decimal number = NonNegativeDecimal(value, label);
            if (number <= 0) throw new InvalidOperationException("The " + label + " must be greater than zero.");
            return number;
        }

        private static DateTime RequiredDate(string value, string label)
        {
            DateTime date;
            if (!DateTime.TryParseExact((value ?? string.Empty).Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out date))
                throw new InvalidOperationException("Enter a valid " + label + ".");
            return date;
        }

        private static DateTime? OptionalDate(string value, string label)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return RequiredDate(value, label);
        }

        private static void ValidateDateRange(DateTime? from, DateTime? to, string label)
        {
            if (from.HasValue != to.HasValue)
                throw new InvalidOperationException("Enter both the start and end dates for " + label + ".");
            if (from.HasValue && from.Value.Date > to.Value.Date)
                throw new InvalidOperationException(label + " start date cannot be later than the end date.");
        }

        private static string Required(string value, string label, int maximum)
        {
            value = (value ?? string.Empty).Trim();
            if (value.Length == 0) throw new InvalidOperationException("Enter the " + label + ".");
            if (value.Length > maximum) throw new InvalidOperationException("The " + label + " is too long.");
            return value;
        }

        private static object Optional(string value, int maximum)
        {
            value = (value ?? string.Empty).Trim();
            if (value.Length > maximum) throw new InvalidOperationException("An entered value is too long.");
            return value.Length == 0 ? (object)DBNull.Value : value;
        }

        private static string Text(DataRow row, string column)
        {
            return Convert.ToString(row[column], CultureInfo.InvariantCulture);
        }

        private static string DbText(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? string.Empty : Text(row, column);
        }

        private static string DateText(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? string.Empty :
                Convert.ToDateTime(row[column], CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static void SelectValue(ListControl control, string value)
        {
            ListItem item = control.Items.FindByValue(value);
            if (item != null)
            {
                control.ClearSelection();
                item.Selected = true;
            }
        }

        private static SqlParameter P(string name, SqlDbType type, object value)
        {
            SqlParameter parameter = new SqlParameter(name, type);
            parameter.Value = value == null ? DBNull.Value : value;
            return parameter;
        }

        private static SqlParameter P(string name, SqlDbType type, object value, int size)
        {
            SqlParameter parameter = new SqlParameter(name, type, size);
            parameter.Value = value == null ? DBNull.Value : value;
            return parameter;
        }

        private static SqlParameter P(string name, SqlDbType type, object value, int size, byte precision, byte scale)
        {
            SqlParameter parameter = size > 0 ? new SqlParameter(name, type, size) : new SqlParameter(name, type);
            parameter.Precision = precision;
            parameter.Scale = scale;
            parameter.Value = value == null ? DBNull.Value : value;
            return parameter;
        }

        private static SqlParameter[] Add(SqlParameter[] source, SqlParameter extra)
        {
            SqlParameter[] result = new SqlParameter[source.Length + 1];
            Array.Copy(source, result, source.Length);
            result[source.Length] = extra;
            return result;
        }
    }
}
