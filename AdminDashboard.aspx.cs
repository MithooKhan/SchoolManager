using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class WebForm17 : Page
    {
        private string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try { SchoolLifecycleService.EnsureSchema(); }
                catch (Exception lifecycleError) { System.Diagnostics.Trace.TraceWarning("[AdminDashboard] Lifecycle schema: {0}", lifecycleError.Message); }
                LoadDashboard();
                InitializeFundMonitor();
                InitializeFeeMonitor();
                InitializeStudentAttendanceMonitor();
                InitializeExamResultMonitor();
            }
        }

        private void InitializeFundMonitor()
        {
            bool isAdministrator = SystemUserSecurity.IsAdministrator(Context);
            pnlFundSummaryLogin.Visible = !isAdministrator;
            pnlFundSummaryAdmin.Visible = isAdministrator;
            pnlFundSummaryUnavailable.Visible = false;

            if (!isAdministrator)
                return;

            try
            {
                FundManagementService.EnsureSchema();
                FundAccountSummary summary = FundManagementService.GetSummary();
                lblDashboardNsbBalance.Text = FormatMoney(summary.NsbBalance);
                lblDashboardFtfWallet.Text = FormatMoney(summary.FtfCashInHand);
                lblDashboardFtfBalance.Text = FormatMoney(summary.FtfAccountBalance);

                DateTime today = DateTime.Today;
                DateTime firstDay = new DateTime(today.Year, today.Month, 1);
                FtfCollectionPerformance performance = FundManagementService.GetFtfPerformance(firstDay, today);
                lblDashboardFtfMonth.Text = FormatMoney(performance.ActualCollected);
                lblDashboardFtfTarget.Text = performance.CollectionPercentage.ToString("N2", CultureInfo.InvariantCulture) + "% of target";

                BudgetManagementService.EnsureSchema();
                DataTable budgets = BudgetManagementService.GetBudgets(string.Empty);
                if (budgets.Rows.Count > 0)
                {
                    int budgetId = Convert.ToInt32(budgets.Rows[0]["BudgetID"], CultureInfo.InvariantCulture);
                    BudgetTotals budgetTotals = BudgetManagementService.GetTotals(budgetId);
                    lblDashboardBudgetEstimate.Text = FormatMoney(budgetTotals.GrandTotal);
                    lblDashboardBudgetName.Text = Convert.ToString(budgets.Rows[0]["BudgetName"]);
                }

                try
                {
                    MonthlyExpenditureService.EnsureSchema();
                    MonthlyExpenditureDashboardSummary expenditure = MonthlyExpenditureService.GetDashboardSummary();
                    if (expenditure.StatementID > 0)
                    {
                        lblDashboardMonthlyExpenditure.Text = FormatMoney(expenditure.CurrentMonthAmount);
                        lblDashboardMonthlyExpenditureName.Text = expenditure.StatementName;
                    }
                }
                catch (Exception expenditureError)
                {
                    System.Diagnostics.Trace.TraceWarning("[AdminDashboard] Monthly expenditure summary failed: {0}", expenditureError.Message);
                    lblDashboardMonthlyExpenditure.Text = "Unavailable";
                    lblDashboardMonthlyExpenditureName.Text = "Open the module to initialize its database tables";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[AdminDashboard] Fund summary failed: {0}", ex.Message);
                pnlFundSummaryUnavailable.Visible = true;
                lblFundSummaryUnavailable.Text = "The fund summary is unavailable. Run the Funds, Budget, and Monthly Expenditure database update scripts and verify database permissions.";
            }
        }

        private void InitializeExamResultMonitor()
        {
            bool isAdministrator = SystemUserSecurity.IsAdministrator(Context);
            pnlResultSummaryLogin.Visible = !isAdministrator;
            pnlResultSummaryAdmin.Visible = isAdministrator;
            pnlResultSummaryUnavailable.Visible = false;

            if (!isAdministrator)
            {
                return;
            }

            try
            {
                BindLatestExamResultSummary();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning(
                    "[AdminDashboard] Examination result summary failed: {0}",
                    ex.Message);
                pnlResultSummaryUnavailable.Visible = true;
                lblResultSummaryUnavailable.Text =
                    "The examination summary is unavailable. Run StudentResults_Database_Update.sql and verify database permissions.";
                rptClassTopStudents.DataSource = null;
                rptClassTopStudents.DataBind();
                pnlNoClassTopStudents.Visible = true;
                lblExamAppearedStudents.Text = "0";
                lblExamPassPercentage.Text = "0.00%";
                lblExamClassesReported.Text = "0";
            }
        }

        private void BindLatestExamResultSummary()
        {
            const string latestExamSql = @"
SELECT TOP (1)
       e.ExamID,
       e.ExamName,
       e.StartDate,
       e.EndDate
FROM dbo.Exams e
INNER JOIN dbo.StudentExamResults r ON r.ExamID = e.ExamID
GROUP BY e.ExamID, e.ExamName, e.StartDate, e.EndDate
ORDER BY e.StartDate DESC, e.ExamID DESC;";

            int examId = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(latestExamSql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        examId = Convert.ToInt32(reader["ExamID"], CultureInfo.InvariantCulture);
                        lblDashboardResultExam.Text = Convert.ToString(reader["ExamName"]);
                        DateTime startDate = Convert.ToDateTime(reader["StartDate"], CultureInfo.InvariantCulture);
                        DateTime endDate = reader["EndDate"] == DBNull.Value
                            ? startDate
                            : Convert.ToDateTime(reader["EndDate"], CultureInfo.InvariantCulture);
                        lblDashboardResultDate.Text = startDate.Date == endDate.Date
                            ? startDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)
                            : startDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + " - " +
                              endDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
                    }
                }
            }

            if (examId <= 0)
            {
                lblDashboardResultExam.Text = "No examination results entered";
                lblDashboardResultDate.Text = "Enter student marks to activate the performance summary.";
                lblExamAppearedStudents.Text = "0";
                lblExamPassPercentage.Text = "0.00%";
                lblExamClassesReported.Text = "0";
                rptClassTopStudents.DataSource = null;
                rptClassTopStudents.DataBind();
                pnlNoClassTopStudents.Visible = true;
                return;
            }

            const string summarySql = @"
WITH StudentScores AS
(
    SELECT r.StudentID,
           r.ClassID,
           SUM(r.ObtainedMarks) AS ObtainedMarks,
           SUM(r.TotalMarks) AS TotalMarks
    FROM dbo.StudentExamResults r
    WHERE r.ExamID = @ExamID
    GROUP BY r.StudentID, r.ClassID
)
SELECT COUNT(*) AS AppearedStudents,
       COUNT(DISTINCT ClassID) AS ClassesReported,
       SUM(CASE WHEN TotalMarks > 0 AND (ObtainedMarks * 100.0) / TotalMarks >= 40
                THEN 1 ELSE 0 END) AS PassedStudents
FROM StudentScores;";

            int appeared = 0;
            int passed = 0;
            int classesReported = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(summarySql, connection))
            {
                command.Parameters.Add("@ExamID", SqlDbType.Int).Value = examId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        appeared = reader["AppearedStudents"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["AppearedStudents"], CultureInfo.InvariantCulture);
                        passed = reader["PassedStudents"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["PassedStudents"], CultureInfo.InvariantCulture);
                        classesReported = reader["ClassesReported"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["ClassesReported"], CultureInfo.InvariantCulture);
                    }
                }
            }

            decimal passPercentage = appeared <= 0
                ? 0
                : decimal.Round(passed * 100m / appeared, 2);
            lblExamAppearedStudents.Text = appeared.ToString("N0", CultureInfo.InvariantCulture);
            lblExamPassPercentage.Text = passPercentage.ToString("0.00", CultureInfo.InvariantCulture) + "%";
            lblExamClassesReported.Text = classesReported.ToString("N0", CultureInfo.InvariantCulture);

            BindClassTopStudents(examId);
        }

        private void BindClassTopStudents(int examId)
        {
            const string sql = @"
WITH StudentScores AS
(
    SELECT r.StudentID,
           r.ClassID,
           SUM(r.ObtainedMarks) AS ObtainedMarks,
           SUM(r.TotalMarks) AS TotalMarks,
           CAST(CASE WHEN SUM(r.TotalMarks) = 0 THEN 0
                     ELSE SUM(r.ObtainedMarks) * 100.0 / SUM(r.TotalMarks)
                END AS DECIMAL(8,2)) AS Percentage
    FROM dbo.StudentExamResults r
    WHERE r.ExamID = @ExamID
    GROUP BY r.StudentID, r.ClassID
), Ranked AS
(
    SELECT scores.*,
           ROW_NUMBER() OVER
           (
               PARTITION BY scores.ClassID
               ORDER BY scores.Percentage DESC,
                        scores.ObtainedMarks DESC,
                        scores.StudentID
           ) AS ClassRank
    FROM StudentScores scores
)
SELECT ranked.StudentID,
       ranked.ClassID,
       ranked.ClassRank,
       ranked.ObtainedMarks,
       ranked.TotalMarks,
       ranked.Percentage,
       ISNULL(s.Name, '') AS StudentName,
       ISNULL(s.FatherName, '') AS FatherName,
       c.ClassName,
       ISNULL(CONVERT(NVARCHAR(20), roll.StudentRollNo), '-') AS StudentRollNo
FROM Ranked ranked
INNER JOIN dbo.Students s ON s.StudentID = ranked.StudentID
INNER JOIN dbo.Classes c ON c.ClassID = ranked.ClassID
OUTER APPLY
(
    SELECT TOP (1) sc.StudentRollNo
    FROM dbo.StudentClass sc
    WHERE sc.StudentID = ranked.StudentID
      AND sc.ClassID = ranked.ClassID
) roll
WHERE ranked.ClassRank <= 3
ORDER BY c.ClassName, ranked.ClassRank;";

            var table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@ExamID", SqlDbType.Int).Value = examId;
                adapter.Fill(table);
            }

            table.Columns.Add("StudentPhotoUrl", typeof(string));
            foreach (DataRow row in table.Rows)
            {
                row["StudentPhotoUrl"] = ResolveUrl("~/StudentImage.ashx?id=" +
                    Convert.ToString(row["StudentID"], CultureInfo.InvariantCulture));
            }

            rptClassTopStudents.DataSource = table;
            rptClassTopStudents.DataBind();
            pnlNoClassTopStudents.Visible = table.Rows.Count == 0;
        }

        private void InitializeStudentAttendanceMonitor()
        {
            bool isAdministrator = SystemUserSecurity.IsAdministrator(Context);
            pnlStudentAttendanceLogin.Visible = !isAdministrator;
            pnlStudentAttendanceAdmin.Visible = isAdministrator;
            pnlStudentAttendanceUnavailable.Visible = false;

            if (!isAdministrator)
            {
                return;
            }

            try
            {
                StudentAttendanceService.EnsureSchema();
                AttendanceCalendarService.EnsureSchema();
                BindStudentAttendanceMonitor();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[AdminDashboard] Student attendance monitor failed: {0}", ex.Message);
                pnlStudentAttendanceUnavailable.Visible = true;
                lblStudentAttendanceUnavailable.Text =
                    "Student attendance is unavailable. Run StudentAttendance_Database_Update.sql and verify database permissions.";
                rptDashboardClassAttendance.DataSource = null;
                rptDashboardClassAttendance.DataBind();
                rptDashboardShortAttendance.DataSource = null;
                rptDashboardShortAttendance.DataBind();
                pnlNoDashboardClassAttendance.Visible = true;
                pnlNoDashboardShortAttendance.Visible = true;
            }
        }

        private void BindStudentAttendanceMonitor()
        {
            const string classSql = @"
SELECT c.ClassID,
       c.ClassName,
       COUNT(DISTINCT s.StudentID) AS StudentCount,
       COUNT(DISTINCT CASE WHEN a.AttendanceDate = @Today THEN a.StudentID END) AS MarkedToday,
       COUNT(DISTINCT CASE WHEN a.AttendanceDate = @Today AND a.AttendanceStatus = N'Present' THEN a.StudentID END) AS PresentToday,
       CAST(CASE WHEN COUNT(a.AttendanceID) = 0 THEN 0
                 ELSE SUM(CASE WHEN a.AttendanceStatus = N'Present' THEN 1.0 ELSE 0 END) * 100.0 / COUNT(a.AttendanceID)
            END AS DECIMAL(5,1)) AS AttendancePercentage
FROM dbo.Classes c
INNER JOIN dbo.StudentClass sc ON sc.ClassID = c.ClassID
INNER JOIN dbo.Students s ON s.StudentID = sc.StudentID
LEFT JOIN dbo.StudentAttendance a
       ON a.StudentID = s.StudentID
      AND a.ClassID = c.ClassID
      AND a.AttendanceDate >= DATEADD(DAY, -29, @Today)
      AND a.AttendanceDate <= @Today
      AND (DATEDIFF(DAY, CONVERT(DATE, '19000107', 112), a.AttendanceDate) % 7) <> 0
      AND NOT EXISTS
          (SELECT 1 FROM dbo.SchoolAttendanceHolidays h
           WHERE h.HolidayDate <= a.AttendanceDate
             AND h.HolidayEndDate >= a.AttendanceDate)
WHERE LTRIM(RTRIM(ISNULL(s.Isactive, 'Active'))) IN ('Active', 'True', '1')
GROUP BY c.ClassID, c.ClassName
ORDER BY c.ClassName;";

            var classTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(classSql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@Today", SqlDbType.Date).Value = DateTime.Today;
                adapter.Fill(classTable);
            }

            rptDashboardClassAttendance.DataSource = classTable;
            rptDashboardClassAttendance.DataBind();
            pnlNoDashboardClassAttendance.Visible = classTable.Rows.Count == 0;

            int presentToday = 0;
            int markedToday = 0;
            foreach (DataRow row in classTable.Rows)
            {
                presentToday += Convert.ToInt32(row["PresentToday"], CultureInfo.InvariantCulture);
                markedToday += Convert.ToInt32(row["MarkedToday"], CultureInfo.InvariantCulture);
            }
            lblStudentPresentToday.Text = presentToday.ToString("N0", CultureInfo.InvariantCulture) + " / " +
                                          markedToday.ToString("N0", CultureInfo.InvariantCulture);

            const string shortSql = @"
WITH StudentProgress AS
(
    SELECT s.StudentID,
           s.Name AS StudentName,
           c.ClassName,
           sc.StudentRollNo,
           SUM(CASE WHEN a.AttendanceStatus = N'Absent' THEN 1 ELSE 0 END) AS AbsentDays,
           COUNT(a.AttendanceID) AS MarkedDays,
           CAST(CASE WHEN COUNT(a.AttendanceID) = 0 THEN 0
                     ELSE SUM(CASE WHEN a.AttendanceStatus = N'Present' THEN 1.0 ELSE 0 END) * 100.0 / COUNT(a.AttendanceID)
                END AS DECIMAL(5,1)) AS AttendancePercentage
    FROM dbo.StudentClass sc
    INNER JOIN dbo.Students s ON s.StudentID = sc.StudentID
    INNER JOIN dbo.Classes c ON c.ClassID = sc.ClassID
    LEFT JOIN dbo.StudentAttendance a
           ON a.StudentID = s.StudentID
          AND a.ClassID = c.ClassID
          AND a.AttendanceDate >= DATEADD(DAY, -29, @Today)
          AND a.AttendanceDate <= @Today
          AND (DATEDIFF(DAY, CONVERT(DATE, '19000107', 112), a.AttendanceDate) % 7) <> 0
          AND NOT EXISTS
              (SELECT 1 FROM dbo.SchoolAttendanceHolidays h
               WHERE h.HolidayDate <= a.AttendanceDate
                 AND h.HolidayEndDate >= a.AttendanceDate)
    WHERE LTRIM(RTRIM(ISNULL(s.Isactive, 'Active'))) IN ('Active', 'True', '1')
    GROUP BY s.StudentID, s.Name, c.ClassName, sc.StudentRollNo
), ShortAttendance AS
(
    SELECT * FROM StudentProgress WHERE MarkedDays > 0 AND AttendancePercentage < 75
)
SELECT TOP (8) StudentID, StudentName, ClassName,
       ISNULL(CONVERT(NVARCHAR(20), StudentRollNo), '-') AS StudentRollNo,
       AbsentDays, MarkedDays, AttendancePercentage,
       COUNT(*) OVER() AS TotalShortStudents
FROM ShortAttendance
ORDER BY AttendancePercentage, AbsentDays DESC, ClassName, StudentRollNo;";

            var shortTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(shortSql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@Today", SqlDbType.Date).Value = DateTime.Today;
                adapter.Fill(shortTable);
            }

            rptDashboardShortAttendance.DataSource = shortTable;
            rptDashboardShortAttendance.DataBind();
            pnlNoDashboardShortAttendance.Visible = shortTable.Rows.Count == 0;
            lblShortAttendanceCount.Text = shortTable.Rows.Count == 0
                ? "0"
                : Convert.ToInt32(shortTable.Rows[0]["TotalShortStudents"], CultureInfo.InvariantCulture)
                    .ToString("N0", CultureInfo.InvariantCulture);
        }

        private void InitializeFeeMonitor()
        {
            bool isAdministrator = SystemUserSecurity.IsAdministrator(Context);
            pnlFeeDefaultersLogin.Visible = !isAdministrator;
            pnlFeeDefaultersAdmin.Visible = isAdministrator;
            pnlFeeReminderMessage.Visible = false;

            if (!isAdministrator)
            {
                return;
            }

            LoadFeePeriodLists();
            try
            {
                FeeReminderService.EnsureSchema();
                BindFeeDefaulters();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[AdminDashboard] Fee monitor could not be initialized: {0}", ex.Message);
                ShowFeeReminderMessage(
                    "The outstanding-fee monitor is unavailable. Run FeeCollection_Database_Update.sql and verify database permissions.",
                    false);
            }
        }

        private void LoadFeePeriodLists()
        {
            ddlDefaulterMonth.Items.Clear();
            for (int month = 1; month <= 12; month++)
            {
                ddlDefaulterMonth.Items.Add(new ListItem(
                    CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month),
                    month.ToString(CultureInfo.InvariantCulture)));
            }

            int currentYear = DateTime.Today.Year;
            ddlDefaulterYear.Items.Clear();
            for (int year = currentYear - 2; year <= currentYear + 1; year++)
            {
                ddlDefaulterYear.Items.Add(new ListItem(
                    year.ToString(CultureInfo.InvariantCulture),
                    year.ToString(CultureInfo.InvariantCulture)));
            }

            ddlDefaulterMonth.SelectedValue = DateTime.Today.Month.ToString(CultureInfo.InvariantCulture);
            ddlDefaulterYear.SelectedValue = currentYear.ToString(CultureInfo.InvariantCulture);
        }

        protected void btnRefreshDefaulters_Click(object sender, EventArgs e)
        {
            if (!RequireAdministrator())
            {
                return;
            }

            pnlFeeReminderMessage.Visible = false;
            BindFeeDefaulters();
        }

        protected void rptFeeDefaulters_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "SendReminder", StringComparison.OrdinalIgnoreCase) ||
                !RequireAdministrator())
            {
                return;
            }

            int studentId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out studentId) || studentId <= 0)
            {
                ShowFeeReminderMessage("The selected student reminder is invalid.", false);
                return;
            }

            try
            {
                ReminderDispatchResult result = FeeReminderService.SendOrQueue(
                    studentId,
                    GetSelectedFeeMonth(),
                    GetSelectedFeeYear(),
                    GetCurrentSystemUserId());
                ShowFeeReminderMessage(result.Message, result.Success);
                BindFeeDefaulters();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[AdminDashboard] Fee reminder failed: {0}", ex.Message);
                ShowFeeReminderMessage("The parent reminder could not be processed. Verify the database and SMS settings.", false);
            }
        }

        protected void btnSendAllReminders_Click(object sender, EventArgs e)
        {
            if (!RequireAdministrator())
            {
                return;
            }

            try
            {
                DataTable defaulters = LoadFeeDefaulters(GetSelectedFeeMonth(), GetSelectedFeeYear());
                int sent = 0;
                int queued = 0;
                int failed = 0;

                foreach (DataRow row in defaulters.Rows)
                {
                    ReminderDispatchResult result = FeeReminderService.SendOrQueue(
                        Convert.ToInt32(row["StudentID"], CultureInfo.InvariantCulture),
                        GetSelectedFeeMonth(),
                        GetSelectedFeeYear(),
                        GetCurrentSystemUserId());

                    if (!result.Success)
                    {
                        failed++;
                    }
                    else if (result.IsQueued)
                    {
                        queued++;
                    }
                    else
                    {
                        sent++;
                    }
                }

                ShowFeeReminderMessage(
                    "Reminder run completed: " + sent.ToString(CultureInfo.InvariantCulture) + " sent, " +
                    queued.ToString(CultureInfo.InvariantCulture) + " queued, " +
                    failed.ToString(CultureInfo.InvariantCulture) + " failed.",
                    failed == 0);
                BindFeeDefaulters();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[AdminDashboard] Bulk fee reminders failed: {0}", ex.Message);
                ShowFeeReminderMessage("The bulk reminder run could not be completed.", false);
            }
        }

        private void BindFeeDefaulters()
        {
            DataTable table = LoadFeeDefaulters(GetSelectedFeeMonth(), GetSelectedFeeYear());
            rptFeeDefaulters.DataSource = table;
            rptFeeDefaulters.DataBind();
            lblFeeDefaulterCount.Text = table.Rows.Count.ToString("N0", CultureInfo.InvariantCulture);
            pnlNoFeeDefaulters.Visible = table.Rows.Count == 0;
            btnSendAllReminders.Enabled = table.Rows.Count > 0;
        }

        private DataTable LoadFeeDefaulters(int feeMonth, int feeYear)
        {
            const string sql = @"
WITH CurrentClass AS
(
    SELECT sc.StudentID,
           sc.ClassID,
           sc.StudentRollNo,
           ROW_NUMBER() OVER (PARTITION BY sc.StudentID ORDER BY sc.ClassID DESC) AS RowNumber
    FROM dbo.StudentClass sc
)
SELECT s.StudentID,
       ISNULL(s.Name, '') AS StudentName,
       ISNULL(s.FatherName, '') AS FatherName,
       ISNULL(s.Regno, '') AS Regno,
       ISNULL(NULLIF(LTRIM(RTRIM(s.ContactNo)), ''), 'Not saved') AS ContactNo,
       ISNULL(c.ClassName, 'Not assigned') AS ClassName,
       ISNULL(CONVERT(NVARCHAR(20), cc.StudentRollNo), 'Not assigned') AS StudentRollNo,
       ISNULL((SELECT TOP (1) r.Status
               FROM dbo.FeeReminderOutbox r
               WHERE r.StudentID = s.StudentID
                 AND r.FeeMonth = @FeeMonth
                 AND r.FeeYear = @FeeYear
               ORDER BY r.CreatedAtUtc DESC), 'Not generated') AS ReminderStatus
FROM dbo.Students s
INNER JOIN CurrentClass cc ON cc.StudentID = s.StudentID AND cc.RowNumber = 1
INNER JOIN dbo.Classes c ON c.ClassID = cc.ClassID
WHERE EXISTS
(
    SELECT 1 FROM dbo.FeeSchoolCharges rate WHERE rate.ClassID = cc.ClassID
)
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.FundsCollection paid
    WHERE paid.StudentID = s.StudentID
      AND paid.FeeMonth = @FeeMonth
      AND paid.FeeYear = @FeeYear
      AND paid.PaidAmount > 0
)
ORDER BY c.ClassName, cc.StudentRollNo, s.Name;";

            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@FeeMonth", SqlDbType.TinyInt).Value = feeMonth;
                command.Parameters.Add("@FeeYear", SqlDbType.SmallInt).Value = feeYear;
                adapter.Fill(table);
            }

            return table;
        }

        private bool RequireAdministrator()
        {
            if (SystemUserSecurity.IsAdministrator(Context))
            {
                return true;
            }

            pnlFeeDefaultersAdmin.Visible = false;
            pnlFeeDefaultersLogin.Visible = true;
            ShowFeeReminderMessage("Administrator login is required to manage parent reminders.", false);
            return false;
        }

        private int GetSelectedFeeMonth()
        {
            int month;
            return int.TryParse(ddlDefaulterMonth.SelectedValue, out month) && month >= 1 && month <= 12
                ? month
                : DateTime.Today.Month;
        }

        private int GetSelectedFeeYear()
        {
            int year;
            return int.TryParse(ddlDefaulterYear.SelectedValue, out year) && year >= 2000 && year <= 2100
                ? year
                : DateTime.Today.Year;
        }

        private int? GetCurrentSystemUserId()
        {
            int userId;
            return Session != null && int.TryParse(Convert.ToString(Session["SystemUserID"]), out userId)
                ? (int?)userId
                : null;
        }

        private void ShowFeeReminderMessage(string message, bool success)
        {
            pnlFeeReminderMessage.Visible = true;
            pnlFeeReminderMessage.CssClass = success
                ? "dashboard-reminder-message reminder-success"
                : "dashboard-reminder-message reminder-warning";
            lblFeeReminderMessage.Text = message;
        }

        private void LoadDashboard()
        {
            lblCurrentDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy", CultureInfo.InvariantCulture);
            SetUnavailableDefaults();

            var warnings = new List<string>();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    int totalStudents = ExecuteCount(connection, "SELECT COUNT(*) FROM dbo.Students WHERE LTRIM(RTRIM(ISNULL(Isactive, 'Active'))) IN ('Active','True','1')", warnings, "Students");
                    int totalTeachers = ExecuteCount(connection, "SELECT COUNT(*) FROM dbo.Teachers WHERE ISNULL(IsActive,1)=1", warnings, "Teachers");
                    int totalNonTeaching = ExecuteCount(connection, "SELECT COUNT(*) FROM dbo.NonTeachingStaff WHERE ISNULL(IsActive,1)=1", warnings, "NonTeachingStaff");
                    int totalClasses = ExecuteCount(connection, "SELECT COUNT(*) FROM dbo.Classes", warnings, "Classes");

                    lblTotalStudents.Text = FormatMetric(totalStudents);
                    lblTotalTeachers.Text = FormatMetric(totalTeachers);
                    lblNonTeachingStaff.Text = FormatMetric(totalNonTeaching);
                    lblTotalClasses.Text = FormatMetric(totalClasses);

                    var todayParameter = new SqlParameter("@Today", SqlDbType.Date) { Value = DateTime.Today };
                    int todayAdmissions = ExecuteCount(
                        connection,
                        "SELECT COUNT(*) FROM dbo.Students WHERE DateofAdmission >= @Today AND DateofAdmission < DATEADD(DAY, 1, @Today) AND LTRIM(RTRIM(ISNULL(Isactive, 'Active'))) IN ('Active','True','1')",
                        warnings,
                        "TodayAdmissions",
                        todayParameter);
                    lblTodayAdmissions.Text = FormatMetric(todayAdmissions);

                    int teachingPresent = ExecuteCount(
                        connection,
                        "SELECT COUNT(*) FROM dbo.TeachingStaffAttendance WHERE AttendanceDate = @Today AND AttendanceStatus = 'Present'",
                        warnings,
                        "TeachingAttendance",
                        new SqlParameter("@Today", SqlDbType.Date) { Value = DateTime.Today });

                    int nonTeachingPresent = ExecuteCount(
                        connection,
                        "SELECT COUNT(*) FROM dbo.NonTeachingStaffAttendance WHERE AttendanceDate = @Today AND AttendanceStatus = 'Present'",
                        warnings,
                        "NonTeachingAttendance",
                        new SqlParameter("@Today", SqlDbType.Date) { Value = DateTime.Today });

                    lblTeachingAttendance.Text = FormatAttendance(teachingPresent, totalTeachers);
                    lblNonTeachingAttendance.Text = FormatAttendance(nonTeachingPresent, totalNonTeaching);
                    lblStaffPresent.Text = teachingPresent < 0 || nonTeachingPresent < 0
                        ? "-"
                        : (teachingPresent + nonTeachingPresent).ToString("N0", CultureInfo.InvariantCulture);

                    int upcomingExams = ExecuteCount(
                        connection,
                        "SELECT COUNT(*) FROM dbo.Exams WHERE EndDate >= @Today",
                        warnings,
                        "UpcomingExams",
                        new SqlParameter("@Today", SqlDbType.Date) { Value = DateTime.Today });
                    lblUpcomingExams.Text = FormatMetric(upcomingExams);

                    int teachingVacancies = ExecuteCount(
                        connection,
                        "SELECT ISNULL(SUM(Vacant), 0) FROM dbo.TeachingVacancyPosition",
                        warnings,
                        "TeachingVacancies");
                    int nonTeachingVacancies = ExecuteCount(
                        connection,
                        "SELECT ISNULL(SUM(Vacant), 0) FROM dbo.Non_TeachingVacancyPosition",
                        warnings,
                        "NonTeachingVacancies");
                    lblVacantPosts.Text = teachingVacancies < 0 || nonTeachingVacancies < 0
                        ? "-"
                        : (teachingVacancies + nonTeachingVacancies).ToString("N0", CultureInfo.InvariantCulture);

                    BindRecentStudents(connection, warnings);
                    BindUpcomingExams(connection, warnings);
                }

                if (warnings.Count == 0)
                {
                    SetStatus("Dashboard is connected and showing current school records.", false);
                }
                else
                {
                    SetStatus("Some dashboard figures could not be loaded. The available school records are shown.", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AdminDashboard] Database connection failed: {0}", ex);
                pnlNoRecentStudents.Visible = true;
                pnlNoUpcomingExams.Visible = true;
                SetStatus("School summary is temporarily unavailable. Verify the SchoolDB connection in Web.config.", true);
            }
        }

        private static int ExecuteCount(
            SqlConnection connection,
            string sql,
            ICollection<string> warnings,
            string metricName,
            params SqlParameter[] parameters)
        {
            try
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    object result = command.ExecuteScalar();
                    return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                warnings.Add(metricName);
                System.Diagnostics.Trace.TraceWarning("[AdminDashboard] {0} could not be loaded: {1}", metricName, ex.Message);
                return -1;
            }
        }

        private void BindRecentStudents(SqlConnection connection, ICollection<string> warnings)
        {
            const string sql = @"
                SELECT TOP (6)
                    s.Name,
                    s.RegNo,
                    ISNULL(c.ClassName, 'Not assigned') AS ClassName,
                    ISNULL(CONVERT(varchar(20), sc.StudentRollNo), '-') AS StudentRollNo,
                    s.DateofAdmission
                FROM dbo.Students s
                LEFT JOIN dbo.StudentClass sc ON sc.StudentID = s.StudentID
                LEFT JOIN dbo.Classes c ON c.ClassID = sc.ClassID
                ORDER BY s.DateofAdmission DESC, s.StudentID DESC;";

            try
            {
                using (var adapter = new SqlDataAdapter(sql, connection))
                {
                    var table = new DataTable();
                    adapter.Fill(table);
                    rptRecentStudents.DataSource = table;
                    rptRecentStudents.DataBind();
                    pnlNoRecentStudents.Visible = table.Rows.Count == 0;
                }
            }
            catch (Exception ex)
            {
                warnings.Add("RecentStudents");
                System.Diagnostics.Trace.TraceWarning("[AdminDashboard] Recent students could not be loaded: {0}", ex.Message);
                pnlNoRecentStudents.Visible = true;
            }
        }

        private void BindUpcomingExams(SqlConnection connection, ICollection<string> warnings)
        {
            const string sql = @"
                SELECT TOP (5) ExamName, StartDate, EndDate
                FROM dbo.Exams
                WHERE EndDate >= @Today
                ORDER BY StartDate, ExamID;";

            try
            {
                using (var command = new SqlCommand(sql, connection))
                using (var adapter = new SqlDataAdapter(command))
                {
                    command.Parameters.Add("@Today", SqlDbType.Date).Value = DateTime.Today;
                    var table = new DataTable();
                    adapter.Fill(table);
                    rptUpcomingExams.DataSource = table;
                    rptUpcomingExams.DataBind();
                    pnlNoUpcomingExams.Visible = table.Rows.Count == 0;
                }
            }
            catch (Exception ex)
            {
                warnings.Add("UpcomingExamList");
                System.Diagnostics.Trace.TraceWarning("[AdminDashboard] Upcoming exams could not be loaded: {0}", ex.Message);
                pnlNoUpcomingExams.Visible = true;
            }
        }

        private void SetUnavailableDefaults()
        {
            lblTotalStudents.Text = "-";
            lblTotalTeachers.Text = "-";
            lblNonTeachingStaff.Text = "-";
            lblTotalClasses.Text = "-";
            lblTodayAdmissions.Text = "-";
            lblStaffPresent.Text = "-";
            lblUpcomingExams.Text = "-";
            lblVacantPosts.Text = "-";
            lblTeachingAttendance.Text = "-";
            lblNonTeachingAttendance.Text = "-";
            pnlNoRecentStudents.Visible = false;
            pnlNoUpcomingExams.Visible = false;
        }

        private void SetStatus(string message, bool warning)
        {
            lblDashboardStatus.Text = message;
            pnlDashboardStatus.CssClass = warning
                ? "dashboard-status dashboard-status-warning"
                : "dashboard-status dashboard-status-success";
        }

        private static string FormatMetric(int value)
        {
            return value < 0 ? "-" : value.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string FormatMoney(decimal value)
        {
            return "Rs. " + value.ToString("N2", CultureInfo.InvariantCulture);
        }

        private static string FormatAttendance(int present, int total)
        {
            if (present < 0 || total < 0)
            {
                return "-";
            }

            return present.ToString("N0", CultureInfo.InvariantCulture) + " / " +
                   total.ToString("N0", CultureInfo.InvariantCulture);
        }
    }
}
