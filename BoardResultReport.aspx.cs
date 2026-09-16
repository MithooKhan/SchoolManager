using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;

namespace DigitalSchoolManager
{
    public partial class BoardResultReport : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SystemUserSecurity.IsAdministrator(Context))
            {
                string returnUrl = HttpUtility.UrlEncode(Request.RawUrl);
                Response.Redirect(ResolveUrl("~/PortalLogin.aspx?ReturnUrl=" + returnUrl), true);
                return;
            }
            if (IsPostBack) return;

            try
            {
                BoardResultService.EnsureSchema(this);
                int sessionId;
                if (!int.TryParse(Request.QueryString["sessionId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out sessionId) || sessionId <= 0)
                    throw new InvalidOperationException("Select a saved board result file before opening the report.");

                string view = (Request.QueryString["view"] ?? "complete").Trim().ToLowerInvariant();
                if (view != "head" && view != "teacher" && view != "complete") view = "complete";

                DataRow session = LoadSession(sessionId);
                DataTable subjects = LoadSubjects(sessionId);
                DataTable positions = LoadPositions(sessionId);
                lblReportTitle.Text = H(Text(session, "ClassNameSnapshot") + " - " + Text(session, "ExamTitle") + " " + Text(session, "ExamYear"));
                lblReportMeta.Text = H(Text(session, "StudyGroup") + " | " + Text(session, "BoardName"));
                litReport.Text = BuildReport(session, subjects, positions, view);
                pnlReport.Visible = true;
                pnlReportError.Visible = false;
            }
            catch (Exception ex)
            {
                pnlReport.Visible = false;
                pnlReportError.Visible = true;
                lblReportError.Text = Server.HtmlEncode("The board result report could not be prepared. " + ex.Message);
            }
        }

        private static string BuildReport(DataRow session, DataTable subjects, DataTable positions, string view)
        {
            StringBuilder html = new StringBuilder(20000);
            if (view == "head" || view == "complete")
                html.Append(BuildHeadReport(session, positions, view == "complete"));
            if (view == "teacher" || view == "complete")
                html.Append(BuildTeacherReport(session, subjects));
            return html.ToString();
        }

        private static string BuildHeadReport(DataRow row, DataTable positions, bool pageBreak)
        {
            int level = Convert.ToInt32(row["ClassLevel"], CultureInfo.InvariantCulture);
            bool previous = level == 10 || level == 12;
            string currentClass = Ordinal(level);
            string registeredClass = Ordinal(previous ? level - 1 : level);
            string css = pageBreak ? "official-sheet page-break" : "official-sheet";
            int gradeTotal = I(row, "GradeAPlusCount") + I(row, "GradeACount") + I(row, "GradeBCount") +
                I(row, "GradeCCount") + I(row, "GradeDCount") + I(row, "GradeECount");

            StringBuilder html = new StringBuilder();
            html.Append("<section class='").Append(css).Append("'>");
            AppendHeader(html, "HEAD WISE RESULT " + currentClass.ToUpperInvariant() + " CLASS " +
                Text(row, "ExamTitle").ToUpperInvariant() + " " + Text(row, "ExamYear") + ".");
            AppendMeta(html, row);
            html.Append("<table class='official-grid head-grid'><thead><tr>")
                .Append("<th rowspan='2'>Sr. No.</th><th rowspan='2'>EMIS Code</th><th rowspan='2'>BISE Code</th>")
                .Append("<th rowspan='2'>Name of School</th><th rowspan='2'>Responsible Head Teacher / Incharge</th>")
                .Append("<th rowspan='2'>Designation</th><th rowspan='2'>Scale No.</th><th rowspan='2'>Mobile No.</th>")
                .Append("<th colspan='2'>Period of Responsible Head Teacher</th>")
                .Append("<th rowspan='2'>No. Registered in ").Append(H(registeredClass)).Append(" ").Append(H(Text(row, "RegisteredYear"))).Append("</th>");
            if (previous)
                html.Append("<th rowspan='2'>No. Appeared in ").Append(H(Ordinal(level - 1))).Append(" ").Append(H(DbText(row, "PreviousAppearedYear"))).Append("</th>");
            html.Append("<th rowspan='2'>No. Appeared in ").Append(H(currentClass)).Append(" ").Append(H(Text(row, "ExamYear"))).Append("</th>")
                .Append("<th rowspan='2'>No. Passed</th><th rowspan='2'>No. Failed</th><th rowspan='2'>School Pass %</th><th rowspan='2'>Board Pass %</th>")
                .Append("<th colspan='6'>Grade-Wise Passed Result</th><th rowspan='2'>Total</th></tr><tr>")
                .Append("<th>From</th><th>To</th><th>A+</th><th>A</th><th>B</th><th>C</th><th>D</th><th>E</th></tr></thead><tbody><tr>")
                .Append("<td>1</td><td>").Append(H(DbText(row, "EMISCode"))).Append("</td><td>").Append(H(DbText(row, "BISECode"))).Append("</td>")
                .Append("<td>").Append(H("GHSS Maankot")).Append("</td><td>").Append(H(Text(row, "HeadNameSnapshot"))).Append("</td>")
                .Append("<td>").Append(H(DbText(row, "HeadDesignationSnapshot"))).Append("</td><td>").Append(H(DbText(row, "HeadScaleSnapshot"))).Append("</td>")
                .Append("<td>").Append(H(DbText(row, "HeadMobileSnapshot"))).Append("</td><td>").Append(H(DateOrDash(row, "HeadPeriodFrom"))).Append("</td>")
                .Append("<td>").Append(H(DateOrDash(row, "HeadPeriodTo"))).Append("</td><td>").Append(I(row, "RegisteredCount")).Append("</td>");
            if (previous) html.Append("<td>").Append(H(DbText(row, "PreviousAppearedCount"))).Append("</td>");
            html.Append("<td>").Append(I(row, "AppearedCount")).Append("</td><td>").Append(I(row, "PassedCount")).Append("</td>")
                .Append("<td>").Append(I(row, "FailedCount")).Append("</td><td>").Append(Pct(row, "SchoolPassPercentage")).Append("</td>")
                .Append("<td>").Append(Pct(row, "BoardPassPercentage")).Append("</td><td>").Append(I(row, "GradeAPlusCount")).Append("</td>")
                .Append("<td>").Append(I(row, "GradeACount")).Append("</td><td>").Append(I(row, "GradeBCount")).Append("</td>")
                .Append("<td>").Append(I(row, "GradeCCount")).Append("</td><td>").Append(I(row, "GradeDCount")).Append("</td>")
                .Append("<td>").Append(I(row, "GradeECount")).Append("</td><td>").Append(gradeTotal).Append("</td></tr></tbody></table>");

            html.Append("<div class='official-subheading'><h2>Position Holder Result ")
                .Append(H(currentClass)).Append(" Class ").Append(H(Text(row, "ExamTitle"))).Append(" ").Append(H(Text(row, "ExamYear")))
                .Append("</h2><span>Stamp and signature</span></div>")
                .Append("<table class='official-grid position-grid'><thead><tr><th>Sr. No.</th><th>Name of Student</th><th>Father's Name</th><th>Address</th><th>Contact No.</th><th>Obtained Marks</th><th>Total Marks</th><th>Position</th><th>Name of Head Teacher</th><th>Contact No.</th><th>Name of Exam</th></tr></thead><tbody>");
            for (int position = 1; position <= 3; position++)
            {
                DataRow[] found = positions.Select("PositionNumber=" + position.ToString(CultureInfo.InvariantCulture));
                DataRow item = found.Length == 0 ? null : found[0];
                html.Append("<tr><td class='num'>").Append(position).Append("</td>");
                if (item == null)
                {
                    for (int c = 0; c < 7; c++) html.Append("<td>&nbsp;</td>");
                }
                else
                {
                    html.Append("<td>").Append(H(Text(item, "StudentNameSnapshot"))).Append("</td>")
                        .Append("<td>").Append(H(DbText(item, "FatherNameSnapshot"))).Append("</td>")
                        .Append("<td>").Append(H(DbText(item, "AddressSnapshot"))).Append("</td>")
                        .Append("<td>").Append(H(DbText(item, "ContactSnapshot"))).Append("</td>")
                        .Append("<td class='num'>").Append(Marks(item, "ObtainedMarks")).Append("</td>")
                        .Append("<td class='num'>").Append(Marks(item, "TotalMarks")).Append("</td>")
                        .Append("<td class='num'>").Append(H(PositionLabel(position))).Append("</td>");
                }
                html.Append("<td>").Append(H(Text(row, "HeadNameSnapshot"))).Append("</td>")
                    .Append("<td>").Append(H(DbText(row, "HeadMobileSnapshot"))).Append("</td>")
                    .Append("<td>").Append(H(Text(row, "ExamTitle") + " " + Text(row, "ExamYear"))).Append("</td></tr>");
            }
            html.Append("</tbody></table>");
            if (!string.IsNullOrWhiteSpace(DbText(row, "Remarks")))
                html.Append("<p class='official-note'><strong>Remarks:</strong> ").Append(H(DbText(row, "Remarks"))).Append("</p>");
            html.Append("</section>");
            return html.ToString();
        }

        private static string BuildTeacherReport(DataRow row, DataTable subjects)
        {
            int level = Convert.ToInt32(row["ClassLevel"], CultureInfo.InvariantCulture);
            bool previous = level == 10 || level == 12;
            string currentClass = Ordinal(level);
            string registeredClass = Ordinal(previous ? level - 1 : level);
            int columns = previous ? 18 : 17;
            StringBuilder html = new StringBuilder();
            html.Append("<section class='official-sheet'>");
            AppendHeader(html, "TEACHER AND SUBJECT WISE RESULT " + currentClass.ToUpperInvariant() + " CLASS " +
                Text(row, "ExamTitle").ToUpperInvariant() + " " + Text(row, "ExamYear") + ".");
            AppendMeta(html, row);
            html.Append("<table class='official-grid teacher-grid'><thead><tr>")
                .Append("<th>Sr. No.</th><th>Name of School</th><th>Name of Teacher</th><th>Designation</th><th>Scale No.</th><th>Mobile No.</th>")
                .Append("<th>Responsible From</th><th>Responsible To</th><th>Subject</th><th>Field / Group</th>")
                .Append("<th>No. Registered in ").Append(H(registeredClass)).Append(" ").Append(H(Text(row, "RegisteredYear"))).Append("</th>");
            if (previous)
                html.Append("<th>No. Appeared in ").Append(H(Ordinal(level - 1))).Append(" ").Append(H(DbText(row, "PreviousAppearedYear"))).Append("</th>");
            html.Append("<th>No. Appeared in ").Append(H(currentClass)).Append(" ").Append(H(Text(row, "ExamYear"))).Append("</th>");
            if (previous) html.Append("<th>No. Passed</th><th>No. Failed</th>");
            else html.Append("<th>No. Failed</th><th>No. Passed</th>");
            html.Append("<th>School Pass %</th><th>Board Pass %</th><th>Remarks</th></tr></thead><tbody>");
            if (subjects.Rows.Count == 0)
                html.Append("<tr><td colspan='").Append(columns).Append("'>No teacher and subject-wise result entries are available.</td></tr>");
            else
            {
                int serial = 1;
                foreach (DataRow item in subjects.Rows)
                {
                    decimal school = Convert.ToDecimal(item["SchoolPassPercentage"], CultureInfo.InvariantCulture);
                    decimal board = Convert.ToDecimal(item["BoardPassPercentage"], CultureInfo.InvariantCulture);
                    string status = BoardResultService.PerformanceStatus(school, board);
                    string statusClass = school > board ? "official-status-above" : school < board ? "official-status-below" : "official-status-equal";
                    html.Append("<tr><td>").Append(serial++).Append("</td><td>GHSS Maankot</td>")
                        .Append("<td>").Append(H(Text(item, "TeacherNameSnapshot"))).Append("</td>")
                        .Append("<td>").Append(H(DbText(item, "DesignationSnapshot"))).Append("</td>")
                        .Append("<td>").Append(H(DbText(item, "ScaleSnapshot"))).Append("</td>")
                        .Append("<td>").Append(H(DbText(item, "MobileSnapshot"))).Append("</td>")
                        .Append("<td>").Append(H(DateOrDash(item, "ResponsibilityFrom"))).Append("</td>")
                        .Append("<td>").Append(H(DateOrDash(item, "ResponsibilityTo"))).Append("</td>")
                        .Append("<td>").Append(H(Text(item, "SubjectNameSnapshot"))).Append("</td>")
                        .Append("<td>").Append(H(Text(row, "StudyGroup"))).Append("</td>")
                        .Append("<td>").Append(I(item, "RegisteredCount")).Append("</td>");
                    if (previous) html.Append("<td>").Append(H(DbText(item, "PreviousAppearedCount"))).Append("</td>");
                    html.Append("<td>").Append(I(item, "AppearedCount")).Append("</td>");
                    if (previous)
                        html.Append("<td>").Append(I(item, "PassedCount")).Append("</td><td>").Append(I(item, "FailedCount")).Append("</td>");
                    else
                        html.Append("<td>").Append(I(item, "FailedCount")).Append("</td><td>").Append(I(item, "PassedCount")).Append("</td>");
                    html.Append("<td><strong>").Append(Pct(item, "SchoolPassPercentage")).Append("</strong></td>")
                        .Append("<td><strong>").Append(Pct(item, "BoardPassPercentage")).Append("</strong></td>")
                        .Append("<td class='").Append(statusClass).Append("'>").Append(H(status)).Append("</td></tr>");
                }
            }
            html.Append("</tbody></table><div class='official-signature'>Signature and stamp of Head of Institution</div></section>");
            return html.ToString();
        }

        private static void AppendHeader(StringBuilder html, string reportTitle)
        {
            html.Append("<header class='official-header'><img src='images/SchoolLogo.png' alt='School logo' />")
                .Append("<div><h1>Office of the Principal Govt. Higher Secondary School Maankot Kabirwala</h1><p>Subject: ")
                .Append(H(reportTitle)).Append("</p></div><div class='official-file-mark'>GHSS<br/>RESULT</div></header>");
        }

        private static void AppendMeta(StringBuilder html, DataRow row)
        {
            html.Append("<div class='official-meta'><span>").Append(H(Text(row, "BoardName"))).Append("</span><span>")
                .Append(H(Text(row, "ClassNameSnapshot"))).Append("</span><span>")
                .Append(H(Text(row, "StudyGroup"))).Append("</span><span>Result date: ")
                .Append(H(DateOrDash(row, "ResultDate"))).Append("</span></div>");
        }

        private static DataRow LoadSession(int sessionId)
        {
            DataTable table = BoardResultService.Fill(
                "SELECT * FROM dbo.BoardResultSessions WHERE BoardResultSessionID=@ID;",
                new SqlParameter("@ID", SqlDbType.Int) { Value = sessionId });
            if (table.Rows.Count == 0) throw new InvalidOperationException("The selected board result file was not found.");
            return table.Rows[0];
        }

        private static DataTable LoadSubjects(int sessionId)
        {
            return BoardResultService.Fill(
                "SELECT * FROM dbo.TeacherSubjectBoardResults WHERE BoardResultSessionID=@ID ORDER BY SubjectNameSnapshot,TeacherNameSnapshot;",
                new SqlParameter("@ID", SqlDbType.Int) { Value = sessionId });
        }

        private static DataTable LoadPositions(int sessionId)
        {
            return BoardResultService.Fill(
                "SELECT * FROM dbo.BoardResultPositionHolders WHERE BoardResultSessionID=@ID ORDER BY PositionNumber;",
                new SqlParameter("@ID", SqlDbType.Int) { Value = sessionId });
        }

        private static string Ordinal(int level)
        {
            return level.ToString(CultureInfo.InvariantCulture) + "th";
        }

        private static string PositionLabel(int position)
        {
            return position == 1 ? "1st" : position == 2 ? "2nd" : "3rd";
        }

        private static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string Text(DataRow row, string column)
        {
            return Convert.ToString(row[column], CultureInfo.InvariantCulture);
        }

        private static string DbText(DataRow row, string column)
        {
            return row == null || row[column] == DBNull.Value ? string.Empty : Text(row, column);
        }

        private static int I(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? 0 : Convert.ToInt32(row[column], CultureInfo.InvariantCulture);
        }

        private static string Pct(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? "-" :
                Convert.ToDecimal(row[column], CultureInfo.InvariantCulture).ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static string Marks(DataRow row, string column)
        {
            return Convert.ToDecimal(row[column], CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string DateOrDash(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? "-" :
                Convert.ToDateTime(row[column], CultureInfo.InvariantCulture).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }
    }
}
