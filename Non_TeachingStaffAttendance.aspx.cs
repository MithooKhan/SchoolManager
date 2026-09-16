using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SchoolManagement
{
    public partial class NonTeachingStaffAttendance : Page
    {
        private string ConnStr
        {
            get
            {
                return ConfigurationManager
                    .ConnectionStrings["SchoolDB"]
                    .ConnectionString;
            }
        }

        protected bool IsHistoryMode
        {
            get
            {
                object value = ViewState["AttendanceHistoryMode"];
                return value != null && Convert.ToBoolean(value);
            }
            set
            {
                ViewState["AttendanceHistoryMode"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                DigitalSchoolManager.AttendanceCalendarService.EnsureSchema();
                if (!IsPostBack)
                {
                    DigitalSchoolManager.SchoolLifecycleService.EnsureSchema();
                    txtAttendanceDate.Text =
                        DateTime.Today.ToString("yyyy-MM-dd");
                    txtAttendanceDate.Attributes["max"] = DateTime.Today.ToString("yyyy-MM-dd");
                    txtNonTeachingHolidayStartDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                    txtNonTeachingHolidayEndDate.Text = DateTime.Today.ToString("yyyy-MM-dd");

                    IsHistoryMode = false;

                    BindAllStaffForMarking();
                    BindNonTeachingHolidays();
                    ApplyModeUI();
                }
                ConfigureHolidayRangeInputs(
                    txtNonTeachingHolidayStartDate,
                    txtNonTeachingHolidayEndDate);
            }
            catch (Exception ex)
            {
                ShowError("Page load error: " + ex.Message);
                LogError("Page_Load", ex);
            }
        }

        // =====================================================================
        // UI / ALERTS
        // =====================================================================
        private void ShowSuccess(string message)
        {
            pnlAlert.Visible = true;
            pnlError.Visible = false;
            lblAlert.Text = message;
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            pnlAlert.Visible = false;
            lblError.Text = message;
        }

        private void HideAlerts()
        {
            pnlAlert.Visible = false;
            pnlError.Visible = false;
        }

        private void ApplyModeUI()
        {
            pnlMarkMode.Visible = !IsHistoryMode;
            pnlSubmit.Visible = !IsHistoryMode;
            pnlHistoryNote.Visible = IsHistoryMode;

            btnBackToMarking.Visible = IsHistoryMode;

            txtSearchName.Enabled = !IsHistoryMode;
            btnSearchStaff.Enabled = !IsHistoryMode;
            btnShowAllStaff.Visible = !IsHistoryMode;

            if (!IsHistoryMode)
            {
                DateTime attendanceDate;
                if (DateTime.TryParse(txtAttendanceDate.Text, out attendanceDate))
                {
                    ApplyNonTeachingDateAvailability(attendanceDate.Date);
                }
            }
        }

        protected void txtAttendanceDate_TextChanged(object sender, EventArgs e)
        {
            HideAlerts();
            DateTime attendanceDate;
            if (!DateTime.TryParse(txtAttendanceDate.Text, out attendanceDate))
            {
                ShowError("Please select a valid attendance date.");
                return;
            }
            if (attendanceDate.Date > DateTime.Today)
            {
                ShowError("Non-teaching staff attendance cannot be marked for a future date.");
                return;
            }

            IsHistoryMode = false;
            BindAllStaffForMarking(txtSearchName.Text.Trim());
            if (ApplyNonTeachingDateAvailability(attendanceDate.Date))
            {
                HideAlerts();
            }
        }

        private void LogError(string context, Exception ex)
        {
            try
            {
                string dir = Server.MapPath("~/App_Data/Logs/");
                Directory.CreateDirectory(dir);

                File.AppendAllText(
                    Path.Combine(dir, "error_log.txt"),
                    "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
                    "] [" + context + "] " +
                    ex.GetType().Name + ": " +
                    ex.Message +
                    Environment.NewLine);
            }
            catch
            {
            }
        }

        // =====================================================================
        // DEFAULT MARKING SHEET - LOAD EVERY NON-TEACHING STAFF MEMBER
        // =====================================================================
        private void BindAllStaffForMarking(string nameFilter = "")
        {
            try
            {
                IsHistoryMode = false;

                StringBuilder sql = new StringBuilder(@"
                    SELECT
                        s.StaffID,
                        s.Name AS StaffName,
                        ISNULL(s.ContactNo, '-') AS ContactNo,
                        s.Image,
                        CAST(NULL AS varchar(20)) AS AttendanceStatus,
                        CAST(NULL AS date) AS AttendanceDate,

                        (
                            SELECT COUNT(*)
                            FROM NonTeachingStaffAttendance x
                            WHERE x.StaffID = s.StaffID
                              AND x.AttendanceStatus = 'Absent'
                              AND (DATEDIFF(DAY, CONVERT(date, '19000107', 112), x.AttendanceDate) % 7) <> 0
                              AND NOT EXISTS (SELECT 1 FROM dbo.SchoolAttendanceHolidays h WHERE h.HolidayDate <= x.AttendanceDate AND h.HolidayEndDate >= x.AttendanceDate)
                        ) AS TotalAbsents,

                        (
                            SELECT COUNT(*)
                            FROM NonTeachingStaffAttendance x
                            WHERE x.StaffID = s.StaffID
                              AND x.AttendanceStatus = 'Casual Leave'
                              AND (DATEDIFF(DAY, CONVERT(date, '19000107', 112), x.AttendanceDate) % 7) <> 0
                              AND NOT EXISTS (SELECT 1 FROM dbo.SchoolAttendanceHolidays h WHERE h.HolidayDate <= x.AttendanceDate AND h.HolidayEndDate >= x.AttendanceDate)
                        ) AS TotalCasualLeaves

                    FROM NonTeachingStaff s
                    WHERE ISNULL(s.IsActive,1)=1 ");

                List<SqlParameter> parameters =
                    new List<SqlParameter>();

                if (!string.IsNullOrWhiteSpace(nameFilter))
                {
                    sql.Append(" AND s.Name LIKE @Name");

                    parameters.Add(
                        new SqlParameter(
                            "@Name",
                            "%" + nameFilter.Trim() + "%"));
                }

                sql.Append(" ORDER BY s.Name");

                DataTable dt =
                    ExecuteQuery(sql.ToString(), parameters.ToArray());

                gvAttendance.DataSource = dt;
                gvAttendance.DataBind();

                lblRecordCount.Text =
                    dt.Rows.Count +
                    (dt.Rows.Count == 1
                        ? " staff member"
                        : " staff members");

                ApplyModeUI();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Failed to load non-teaching staff: " +
                    ex.Message);

                LogError("BindAllStaffForMarking", ex);
            }
        }

        // =====================================================================
        // DATE RANGE HISTORY - READ ONLY
        // =====================================================================
        private void BindAttendanceHistory(
            DateTime fromDate,
            DateTime toDate)
        {
            try
            {
                IsHistoryMode = true;

                const string sql = @"
                    SELECT
                        a.AttendanceID,
                        a.StaffID,
                        s.Name AS StaffName,
                        ISNULL(s.ContactNo, '-') AS ContactNo,
                        s.Image,
                        a.AttendanceStatus,
                        a.AttendanceDate,

                        (
                            SELECT COUNT(*)
                            FROM NonTeachingStaffAttendance x
                            WHERE x.StaffID = a.StaffID
                              AND x.AttendanceStatus = 'Absent'
                              AND (DATEDIFF(DAY, CONVERT(date, '19000107', 112), x.AttendanceDate) % 7) <> 0
                              AND NOT EXISTS (SELECT 1 FROM dbo.SchoolAttendanceHolidays h WHERE h.HolidayDate <= x.AttendanceDate AND h.HolidayEndDate >= x.AttendanceDate)
                        ) AS TotalAbsents,

                        (
                            SELECT COUNT(*)
                            FROM NonTeachingStaffAttendance x
                            WHERE x.StaffID = a.StaffID
                              AND x.AttendanceStatus = 'Casual Leave'
                              AND (DATEDIFF(DAY, CONVERT(date, '19000107', 112), x.AttendanceDate) % 7) <> 0
                              AND NOT EXISTS (SELECT 1 FROM dbo.SchoolAttendanceHolidays h WHERE h.HolidayDate <= x.AttendanceDate AND h.HolidayEndDate >= x.AttendanceDate)
                        ) AS TotalCasualLeaves

                    FROM NonTeachingStaffAttendance a

                    INNER JOIN NonTeachingStaff s
                        ON s.StaffID = a.StaffID

                    WHERE a.AttendanceDate >= @FromDate
                      AND a.AttendanceDate <= @ToDate

                    ORDER BY
                        a.AttendanceDate DESC,
                        s.Name";

                DataTable dt = ExecuteQuery(
                    sql,
                    new SqlParameter(
                        "@FromDate",
                        SqlDbType.Date)
                    {
                        Value = fromDate.Date
                    },
                    new SqlParameter(
                        "@ToDate",
                        SqlDbType.Date)
                    {
                        Value = toDate.Date
                    });

                dt = ApplyClosedDaysToNonTeachingTable(dt, fromDate, toDate);

                gvAttendance.DataSource = dt;
                gvAttendance.DataBind();

                lblRecordCount.Text =
                    dt.Rows.Count +
                    (dt.Rows.Count == 1
                        ? " attendance record"
                        : " attendance records");

                ApplyModeUI();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Failed to search attendance records: " +
                    ex.Message);

                LogError("BindAttendanceHistory", ex);
            }
        }

        // =====================================================================
        // BULK SUBMIT - SAVE ALL ROWS IN ONE TRANSACTION
        // =====================================================================
        protected void btnSubmitAttendance_Click(
            object sender,
            EventArgs e)
        {
            HideAlerts();

            if (IsHistoryMode)
            {
                ShowError(
                    "Attendance search results are read-only.");
                return;
            }

            DateTime attendanceDate;

            if (!DateTime.TryParse(
                txtAttendanceDate.Text.Trim(),
                out attendanceDate))
            {
                ShowError(
                    "Please select a valid attendance date.");
                return;
            }

            if (attendanceDate.Date > DateTime.Today)
            {
                ShowError(
                    "Attendance date cannot be in the future.");
                return;
            }

            string closedDayName;
            if (DigitalSchoolManager.AttendanceCalendarService.TryGetClosedDay(
                attendanceDate.Date,
                out closedDayName))
            {
                ApplyNonTeachingDateAvailability(attendanceDate.Date);
                ShowError(
                    attendanceDate.ToString("dd MMMM yyyy") + " is " + closedDayName +
                    ". Non-teaching staff attendance cannot be marked on a school-off day.");
                return;
            }

            if (gvAttendance.Rows.Count == 0)
            {
                ShowError(
                    "There are no staff members in the attendance table.");
                return;
            }

            int savedCount = 0;

            try
            {
                using (SqlConnection con =
                    new SqlConnection(ConnStr))
                {
                    con.Open();

                    using (SqlTransaction tran =
                        con.BeginTransaction())
                    {
                        try
                        {
                            foreach (GridViewRow row
                                in gvAttendance.Rows)
                            {
                                if (row.RowType !=
                                    DataControlRowType.DataRow)
                                    continue;

                                HiddenField hfStaffID =
                                    row.FindControl("hfStaffID")
                                    as HiddenField;

                                RadioButton rbPresent =
                                    row.FindControl("rbPresent")
                                    as RadioButton;

                                RadioButton rbAbsent =
                                    row.FindControl("rbAbsent")
                                    as RadioButton;

                                RadioButton rbCasual =
                                    row.FindControl("rbCasual")
                                    as RadioButton;

                                if (hfStaffID == null)
                                    continue;

                                int staffID;

                                if (!int.TryParse(
                                    hfStaffID.Value,
                                    out staffID))
                                    continue;

                                string status = "Present";

                                if (rbAbsent != null &&
                                    rbAbsent.Checked)
                                {
                                    status = "Absent";
                                }
                                else if (rbCasual != null &&
                                         rbCasual.Checked)
                                {
                                    status = "Casual Leave";
                                }
                                else if (rbPresent != null &&
                                         rbPresent.Checked)
                                {
                                    status = "Present";
                                }

                                // Upsert prevents duplicate records for
                                // the same staff member and date.
                                const string saveSql = @"
                                    IF EXISTS
                                    (
                                        SELECT 1
                                        FROM NonTeachingStaffAttendance
                                        WHERE StaffID = @StaffID
                                          AND AttendanceDate = @AttendanceDate
                                    )
                                    BEGIN
                                        UPDATE NonTeachingStaffAttendance
                                        SET AttendanceStatus = @Status
                                        WHERE StaffID = @StaffID
                                          AND AttendanceDate = @AttendanceDate
                                    END
                                    ELSE
                                    BEGIN
                                        INSERT INTO NonTeachingStaffAttendance
                                        (
                                            StaffID,
                                            AttendanceStatus,
                                            AttendanceDate
                                        )
                                        VALUES
                                        (
                                            @StaffID,
                                            @Status,
                                            @AttendanceDate
                                        )
                                    END";

                                using (SqlCommand cmd =
                                    new SqlCommand(
                                        saveSql,
                                        con,
                                        tran))
                                {
                                    cmd.Parameters.Add(
                                        "@StaffID",
                                        SqlDbType.Int)
                                        .Value = staffID;

                                    cmd.Parameters.Add(
                                        "@Status",
                                        SqlDbType.VarChar,
                                        20)
                                        .Value = status;

                                    cmd.Parameters.Add(
                                        "@AttendanceDate",
                                        SqlDbType.Date)
                                        .Value = attendanceDate.Date;

                                    cmd.ExecuteNonQuery();
                                }

                                savedCount++;
                            }

                            tran.Commit();
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }

                txtSearchName.Text = "";

                BindAllStaffForMarking();

                ShowSuccess(
                    "Attendance submitted successfully for " +
                    savedCount +
                    " staff member(s) for " +
                    attendanceDate.ToString("dd-MM-yyyy") +
                    ".");
            }
            catch (Exception ex)
            {
                ShowError(
                    "Attendance submission failed: " +
                    ex.Message);

                LogError(
                    "btnSubmitAttendance_Click",
                    ex);
            }
        }

        protected void btnSaveNonTeachingHoliday_Click(object sender, EventArgs e)
        {
            HideAlerts();
            DateTime holidayStartDate;
            DateTime holidayEndDate;
            if (!DateTime.TryParse(txtNonTeachingHolidayStartDate.Text, out holidayStartDate))
            {
                ShowError("Please select a valid holiday start date.");
                return;
            }
            if (!DateTime.TryParse(txtNonTeachingHolidayEndDate.Text, out holidayEndDate))
            {
                ShowError("Please select a valid holiday end date.");
                return;
            }
            if (holidayEndDate.Date < holidayStartDate.Date)
            {
                ShowError("The holiday end date cannot be earlier than the start date.");
                return;
            }

            try
            {
                DigitalSchoolManager.AttendanceCalendarService.SaveHoliday(
                    holidayStartDate,
                    holidayEndDate,
                    txtNonTeachingHolidayName.Text,
                    GetCurrentSystemUserId());
                txtNonTeachingHolidayName.Text = string.Empty;
                txtNonTeachingHolidayStartDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                txtNonTeachingHolidayEndDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                ConfigureHolidayRangeInputs(
                    txtNonTeachingHolidayStartDate,
                    txtNonTeachingHolidayEndDate);
                BindNonTeachingHolidays();
                RefreshNonTeachingAttendanceAfterCalendarChange();
                ShowSuccess("The school holiday period was saved for student, teaching and non-teaching attendance.");
            }
            catch (Exception ex)
            {
                ShowError("The school holiday period could not be saved. " + ex.Message);
                LogError("btnSaveNonTeachingHoliday_Click", ex);
            }
        }

        protected void gvNonTeachingHolidays_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "DeleteHoliday", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int holidayId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out holidayId))
            {
                ShowError("The selected holiday period is invalid.");
                return;
            }

            try
            {
                bool removed = DigitalSchoolManager.AttendanceCalendarService.DeleteHoliday(holidayId);
                BindNonTeachingHolidays();
                RefreshNonTeachingAttendanceAfterCalendarChange();
                ShowSuccess(removed
                    ? "The school holiday period was removed."
                    : "The holiday period was already removed.");
            }
            catch (Exception ex)
            {
                ShowError("The school holiday period could not be removed. " + ex.Message);
                LogError("gvNonTeachingHolidays_RowCommand", ex);
            }
        }

        private void BindNonTeachingHolidays()
        {
            gvNonTeachingHolidays.DataSource =
                DigitalSchoolManager.AttendanceCalendarService.LoadHolidayList();
            gvNonTeachingHolidays.DataBind();
        }

        private static void ConfigureHolidayRangeInputs(TextBox startDate, TextBox endDate)
        {
            if (startDate == null || endDate == null)
            {
                return;
            }
            endDate.Attributes["min"] = startDate.Text;
            startDate.Attributes["onchange"] =
                "var e=document.getElementById('" + endDate.ClientID +
                "');if(e){e.min=this.value;if(!e.value||e.value<this.value){e.value=this.value;}}";
        }

        private void RefreshNonTeachingAttendanceAfterCalendarChange()
        {
            if (IsHistoryMode)
            {
                DateTime fromDate;
                DateTime toDate;
                if (DateTime.TryParse(txtFromDate.Text, out fromDate) &&
                    DateTime.TryParse(txtToDate.Text, out toDate))
                {
                    BindAttendanceHistory(fromDate, toDate);
                }
                return;
            }

            BindAllStaffForMarking(txtSearchName.Text.Trim());
        }

        private bool ApplyNonTeachingDateAvailability(DateTime attendanceDate)
        {
            string closedDayName;
            bool isClosed = DigitalSchoolManager.AttendanceCalendarService.TryGetClosedDay(
                attendanceDate.Date,
                out closedDayName);
            pnlNonTeachingClosedDay.Visible = isClosed;
            lblNonTeachingClosedDay.Text = isClosed
                ? attendanceDate.ToString("dddd, dd MMMM yyyy") + " - " + closedDayName
                : string.Empty;
            if (!IsHistoryMode)
            {
                pnlSubmit.Visible = !isClosed;
                btnSubmitAttendance.Enabled = !isClosed && gvAttendance.Rows.Count > 0;
                gvAttendance.Enabled = !isClosed;
            }
            return !isClosed;
        }

        private int? GetCurrentSystemUserId()
        {
            int userId;
            return Session != null && int.TryParse(Convert.ToString(Session["SystemUserID"]), out userId)
                ? (int?)userId
                : null;
        }

        // =====================================================================
        // STAFF NAME SEARCH - LIVE MARKING MODE
        // =====================================================================
        protected void btnSearchStaff_Click(
            object sender,
            EventArgs e)
        {
            HideAlerts();

            if (IsHistoryMode)
            {
                ShowError(
                    "Return to Attendance Marking before searching staff.");
                return;
            }

            BindAllStaffForMarking(
                txtSearchName.Text.Trim());
        }

        protected void btnShowAllStaff_Click(
            object sender,
            EventArgs e)
        {
            HideAlerts();

            txtSearchName.Text = "";

            BindAllStaffForMarking();
        }

        // =====================================================================
        // DATE RANGE SEARCH - READ ONLY
        // =====================================================================
        protected void btnSearchDates_Click(
            object sender,
            EventArgs e)
        {
            HideAlerts();

            DateTime fromDate;
            DateTime toDate;

            if (!DateTime.TryParse(
                txtFromDate.Text.Trim(),
                out fromDate))
            {
                ShowError(
                    "Please select the From date.");
                return;
            }

            if (!DateTime.TryParse(
                txtToDate.Text.Trim(),
                out toDate))
            {
                ShowError(
                    "Please select the To date.");
                return;
            }

            if (fromDate.Date > toDate.Date)
            {
                ShowError(
                    "From date cannot be later than To date.");
                return;
            }

            BindAttendanceHistory(
                fromDate,
                toDate);

            ShowSuccess(
                "Attendance records loaded from " +
                fromDate.ToString("dd-MM-yyyy") +
                " to " +
                toDate.ToString("dd-MM-yyyy") +
                ". These records are read-only.");
        }

        protected void btnBackToMarking_Click(
            object sender,
            EventArgs e)
        {
            HideAlerts();

            txtFromDate.Text = "";
            txtToDate.Text = "";
            txtSearchName.Text = "";

            IsHistoryMode = false;

            BindAllStaffForMarking();
        }

        // =====================================================================
        // PRINT CURRENT SAVED ATTENDANCE VIEW
        // =====================================================================
        protected void btnPrint_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                HideAlerts();

                DataTable dt =
                    GetCurrentReportData();

                StringBuilder html =
                    new StringBuilder();

                html.Append(
                    @"<html>
                    <head>
                    <title>Non-Teaching Staff Attendance</title>
                    <style>
                        body{
                            font-family:'Times New Roman',Times,serif;
                            font-size:13px;
                            color:#111;
                        }
                        h2{
                            text-align:center;
                            margin-bottom:12px;
                        }
                        p{text-align:center;}
                        table{
                            width:100%;
                            border-collapse:collapse;
                        }
                        th,td{
                            border:1px solid #111;
                            padding:7px;
                            text-align:left;
                        }
                        .present{color:#0ea63b;}
                        .absent{color:#ef1d24;}
                        .casual{color:#00a8ee;}
                        .closed{color:#0b5a36;font-weight:bold;background:#eef8f2;}
                    </style>
                    </head>
                    <body>");

                html.Append(
                    "<h2>Non-Teaching Staff Attendance Report</h2>");

                if (IsHistoryMode)
                {
                    html.Append(
                        "<p>From: " +
                        HttpUtility.HtmlEncode(
                            txtFromDate.Text) +
                        " &nbsp; To: " +
                        HttpUtility.HtmlEncode(
                            txtToDate.Text) +
                        "</p>");
                }
                else
                {
                    html.Append(
                        "<p>Attendance Date: " +
                        HttpUtility.HtmlEncode(
                            txtAttendanceDate.Text) +
                        "</p>");
                }

                html.Append(
                    "<table><tr>" +
                    "<th>Staff Name</th>" +
                    "<th>Contact No</th>" +
                    "<th>Attendance Status</th>" +
                    "<th>Attendance Date</th>" +
                    "<th>Total Absents</th>" +
                    "<th>Total Casual Leaves</th>" +
                    "</tr>");

                foreach (DataRow row in dt.Rows)
                {
                    string status =
                        row["AttendanceStatus"]
                        .ToString();

                    string cssClass =
                        status == "Present"
                            ? "present"
                            : status == "Absent"
                                ? "absent"
                                : status == "Casual Leave"
                                    ? "casual"
                                    : "closed";

                    html.Append("<tr>");

                    html.Append(
                        "<td>" +
                        HttpUtility.HtmlEncode(
                            row["StaffName"].ToString()) +
                        "</td>");

                    html.Append(
                        "<td>" +
                        HttpUtility.HtmlEncode(
                            row["ContactNo"].ToString()) +
                        "</td>");

                    html.Append(
                        "<td class='" +
                        cssClass +
                        "'>" +
                        HttpUtility.HtmlEncode(status) +
                        "</td>");

                    html.Append(
                        "<td>" +
                        Convert.ToDateTime(
                            row["AttendanceDate"])
                        .ToString("dd-MM-yyyy") +
                        "</td>");

                    html.Append(
                        "<td>" +
                        row["TotalAbsents"] +
                        "</td>");

                    html.Append(
                        "<td>" +
                        row["TotalCasualLeaves"] +
                        "</td>");

                    html.Append("</tr>");
                }

                html.Append(
                    "</table></body></html>");

                string escaped =
                    html.ToString()
                    .Replace("\\", "\\\\")
                    .Replace("'", "\\'")
                    .Replace("\r", "")
                    .Replace("\n", "");

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "printAttendance",
                    "var w=window.open('','_blank');" +
                    "w.document.write('" +
                    escaped +
                    "');" +
                    "w.document.close();" +
                    "w.focus();" +
                    "w.print();",
                    true);
            }
            catch (Exception ex)
            {
                ShowError(
                    "Print failed: " +
                    ex.Message);

                LogError(
                    "btnPrint_Click",
                    ex);
            }
        }

        // =====================================================================
        // EXPORT CURRENT SAVED ATTENDANCE VIEW
        // =====================================================================
        protected void btnExportExcel_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                HideAlerts();

                DataTable dt =
                    GetCurrentReportData();

                StringBuilder excel =
                    new StringBuilder();

                excel.Append(
                    "<?xml version=\"1.0\"?>");

                excel.Append(
                    "<?mso-application progid=\"Excel.Sheet\"?>");

                excel.Append(
                    "<Workbook " +
                    "xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" " +
                    "xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");

                excel.Append(
                    "<Worksheet ss:Name=\"Attendance\">" +
                    "<Table>");

                excel.Append("<Row>");

                string[] headings =
                {
                    "Staff Name",
                    "Contact No",
                    "Attendance Status",
                    "Attendance Date",
                    "Total Absents",
                    "Total Casual Leaves"
                };

                foreach (string heading in headings)
                {
                    AddExcelStringCell(
                        excel,
                        heading);
                }

                excel.Append("</Row>");

                foreach (DataRow row in dt.Rows)
                {
                    excel.Append("<Row>");

                    AddExcelStringCell(
                        excel,
                        row["StaffName"].ToString());

                    AddExcelStringCell(
                        excel,
                        row["ContactNo"].ToString());

                    AddExcelStringCell(
                        excel,
                        row["AttendanceStatus"].ToString());

                    AddExcelStringCell(
                        excel,
                        Convert.ToDateTime(
                            row["AttendanceDate"])
                        .ToString("dd-MM-yyyy"));

                    AddExcelNumberCell(
                        excel,
                        row["TotalAbsents"]);

                    AddExcelNumberCell(
                        excel,
                        row["TotalCasualLeaves"]);

                    excel.Append("</Row>");
                }

                excel.Append(
                    "</Table>" +
                    "</Worksheet>" +
                    "</Workbook>");

                string fileName =
                    "NonTeachingStaffAttendance_" +
                    DateTime.Now.ToString(
                        "yyyyMMdd_HHmm") +
                    ".xls";

                Response.Clear();
                Response.Buffer = true;

                Response.ContentType =
                    "application/vnd.ms-excel";

                Response.ContentEncoding =
                    Encoding.UTF8;

                Response.AddHeader(
                    "Content-Disposition",
                    "attachment; filename=" +
                    fileName);

                Response.Write(
                    excel.ToString());

                Response.Flush();

                HttpContext
                    .Current
                    .ApplicationInstance
                    .CompleteRequest();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Excel export failed: " +
                    ex.Message);

                LogError(
                    "btnExportExcel_Click",
                    ex);
            }
        }

        // =====================================================================
        // REPORT DATA
        // =====================================================================
        private DataTable GetCurrentReportData()
        {
            if (IsHistoryMode)
            {
                DateTime fromDate;
                DateTime toDate;

                if (!DateTime.TryParse(
                    txtFromDate.Text.Trim(),
                    out fromDate) ||
                    !DateTime.TryParse(
                        txtToDate.Text.Trim(),
                        out toDate))
                {
                    throw new Exception(
                        "Select a valid date range first.");
                }

                DataTable history = ExecuteQuery(
                    @"
                    SELECT
                        s.Name AS StaffName,
                        ISNULL(s.ContactNo, '-') AS ContactNo,
                        a.AttendanceStatus,
                        a.AttendanceDate,

                        (
                            SELECT COUNT(*)
                            FROM NonTeachingStaffAttendance x
                            WHERE x.StaffID = a.StaffID
                              AND x.AttendanceStatus = 'Absent'
                              AND (DATEDIFF(DAY, CONVERT(date, '19000107', 112), x.AttendanceDate) % 7) <> 0
                              AND NOT EXISTS (SELECT 1 FROM dbo.SchoolAttendanceHolidays h WHERE h.HolidayDate <= x.AttendanceDate AND h.HolidayEndDate >= x.AttendanceDate)
                        ) AS TotalAbsents,

                        (
                            SELECT COUNT(*)
                            FROM NonTeachingStaffAttendance x
                            WHERE x.StaffID = a.StaffID
                              AND x.AttendanceStatus = 'Casual Leave'
                              AND (DATEDIFF(DAY, CONVERT(date, '19000107', 112), x.AttendanceDate) % 7) <> 0
                              AND NOT EXISTS (SELECT 1 FROM dbo.SchoolAttendanceHolidays h WHERE h.HolidayDate <= x.AttendanceDate AND h.HolidayEndDate >= x.AttendanceDate)
                        ) AS TotalCasualLeaves

                    FROM NonTeachingStaffAttendance a

                    INNER JOIN NonTeachingStaff s
                        ON s.StaffID = a.StaffID

                    WHERE a.AttendanceDate >= @FromDate
                      AND a.AttendanceDate <= @ToDate

                    ORDER BY
                        a.AttendanceDate DESC,
                        s.Name",

                    new SqlParameter(
                        "@FromDate",
                        SqlDbType.Date)
                    {
                        Value = fromDate.Date
                    },

                    new SqlParameter(
                        "@ToDate",
                        SqlDbType.Date)
                    {
                        Value = toDate.Date
                    });

                return ApplyClosedDaysToNonTeachingTable(history, fromDate, toDate);
            }

            DateTime attendanceDate;

            if (!DateTime.TryParse(
                txtAttendanceDate.Text.Trim(),
                out attendanceDate))
            {
                throw new Exception(
                    "Select a valid attendance date.");
            }

            DataTable current = ExecuteQuery(
                @"
                SELECT
                    s.Name AS StaffName,
                    ISNULL(s.ContactNo, '-') AS ContactNo,
                    a.AttendanceStatus,
                    a.AttendanceDate,

                    (
                        SELECT COUNT(*)
                        FROM NonTeachingStaffAttendance x
                        WHERE x.StaffID = a.StaffID
                          AND x.AttendanceStatus = 'Absent'
                          AND (DATEDIFF(DAY, CONVERT(date, '19000107', 112), x.AttendanceDate) % 7) <> 0
                          AND NOT EXISTS (SELECT 1 FROM dbo.SchoolAttendanceHolidays h WHERE h.HolidayDate <= x.AttendanceDate AND h.HolidayEndDate >= x.AttendanceDate)
                    ) AS TotalAbsents,

                    (
                        SELECT COUNT(*)
                        FROM NonTeachingStaffAttendance x
                        WHERE x.StaffID = a.StaffID
                          AND x.AttendanceStatus = 'Casual Leave'
                          AND (DATEDIFF(DAY, CONVERT(date, '19000107', 112), x.AttendanceDate) % 7) <> 0
                          AND NOT EXISTS (SELECT 1 FROM dbo.SchoolAttendanceHolidays h WHERE h.HolidayDate <= x.AttendanceDate AND h.HolidayEndDate >= x.AttendanceDate)
                    ) AS TotalCasualLeaves

                FROM NonTeachingStaffAttendance a

                INNER JOIN NonTeachingStaff s
                    ON s.StaffID = a.StaffID

                WHERE a.AttendanceDate = @AttendanceDate

                ORDER BY s.Name",

                new SqlParameter(
                    "@AttendanceDate",
                    SqlDbType.Date)
                {
                    Value = attendanceDate.Date
                });

            return ApplyClosedDaysToNonTeachingTable(
                current,
                attendanceDate.Date,
                attendanceDate.Date);
        }

        private static DataTable ApplyClosedDaysToNonTeachingTable(
            DataTable table,
            DateTime fromDate,
            DateTime toDate)
        {
            Dictionary<DateTime, string> closedDays =
                DigitalSchoolManager.AttendanceCalendarService.GetClosedDays(
                    fromDate.Date,
                    toDate.Date);
            if (closedDays.Count == 0)
            {
                return table;
            }

            for (int index = table.Rows.Count - 1; index >= 0; index--)
            {
                if (table.Columns.Contains("AttendanceDate") &&
                    table.Rows[index]["AttendanceDate"] != DBNull.Value)
                {
                    DateTime date = Convert.ToDateTime(table.Rows[index]["AttendanceDate"]).Date;
                    if (closedDays.ContainsKey(date))
                    {
                        table.Rows.RemoveAt(index);
                    }
                }
            }

            foreach (KeyValuePair<DateTime, string> closedDay in closedDays)
            {
                DataRow row = table.NewRow();
                SetColumnValue(row, "AttendanceID", DBNull.Value);
                SetColumnValue(row, "StaffID", DBNull.Value);
                SetColumnValue(row, "StaffName", "School Closed");
                SetColumnValue(row, "ContactNo", "-");
                SetColumnValue(row, "Image", DBNull.Value);
                SetColumnValue(row, "AttendanceStatus", closedDay.Value);
                SetColumnValue(row, "AttendanceDate", closedDay.Key);
                SetColumnValue(row, "TotalAbsents", 0);
                SetColumnValue(row, "TotalCasualLeaves", 0);
                table.Rows.Add(row);
            }

            DataView view = table.DefaultView;
            view.Sort = "AttendanceDate DESC, StaffName ASC";
            return view.ToTable();
        }

        private static void SetColumnValue(DataRow row, string columnName, object value)
        {
            if (row.Table.Columns.Contains(columnName))
            {
                row[columnName] = value ?? DBNull.Value;
            }
        }

        // =====================================================================
        // UI HELPERS
        // =====================================================================
        protected string GetAvatarHtml(
            object imageValue,
            string name)
        {
            string safeName =
                HttpUtility.HtmlAttributeEncode(
                    name ?? "");

            string initial =
                string.IsNullOrWhiteSpace(name)
                    ? "?"
                    : HttpUtility.HtmlEncode(
                        name.Substring(0, 1)
                        .ToUpperInvariant());

            if (imageValue != null &&
                imageValue != DBNull.Value)
            {
                byte[] bytes =
                    imageValue as byte[];

                if (bytes != null &&
                    bytes.Length > 0)
                {
                    return
                        "<img class='staff-avatar' alt='" +
                        safeName +
                        "' src='data:image/jpeg;base64," +
                        Convert.ToBase64String(bytes) +
                        "' />";
                }

                string path =
                    imageValue.ToString();

                if (!string.IsNullOrWhiteSpace(path) &&
                    !path.Equals(
                        "System.Byte[]",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return
                        "<img class='staff-avatar' alt='" +
                        safeName +
                        "' src='" +
                        HttpUtility.HtmlAttributeEncode(path) +
                        "' />";
                }
            }

            return
                "<div class='staff-avatar-placeholder'>" +
                initial +
                "</div>";
        }

        protected string GetReadOnlyStatusHtml(
            object value)
        {
            string status =
                value == null
                    ? ""
                    : value.ToString();

            if (status == "Present")
            {
                return
                    "<span class='read-status read-present'>" +
                    "<span class='read-dot'></span>" +
                    "Present</span>";
            }

            if (status == "Absent")
            {
                return
                    "<span class='read-status read-absent'>" +
                    "<span class='read-dot'></span>" +
                    "Absent</span>";
            }

            if (status == "Casual Leave")
            {
                return
                    "<span class='read-status read-casual'>" +
                    "<span class='read-dot'></span>" +
                    "Casual Leave</span>";
            }

            return
                "<span class='read-status read-closed'>" +
                "<span class='read-dot'></span>" +
                HttpUtility.HtmlEncode(status) + "</span>";
        }

        private static string XmlEncode(
            string value)
        {
            return
                System.Security.SecurityElement
                .Escape(value ?? "")
                ?? "";
        }

        private static void AddExcelStringCell(
            StringBuilder excel,
            string value)
        {
            excel.Append(
                "<Cell><Data ss:Type=\"String\">" +
                XmlEncode(value) +
                "</Data></Cell>");
        }

        private static void AddExcelNumberCell(
            StringBuilder excel,
            object value)
        {
            int number = 0;

            if (value != null &&
                value != DBNull.Value)
            {
                int.TryParse(
                    value.ToString(),
                    out number);
            }

            excel.Append(
                "<Cell><Data ss:Type=\"Number\">" +
                number +
                "</Data></Cell>");
        }

        // =====================================================================
        // DATABASE
        // =====================================================================
        private DataTable ExecuteQuery(
            string sql,
            params SqlParameter[] parameters)
        {
            DataTable dt =
                new DataTable();

            using (SqlConnection con =
                new SqlConnection(ConnStr))
            using (SqlCommand cmd =
                new SqlCommand(sql, con))
            {
                if (parameters != null &&
                    parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(
                        parameters);
                }

                con.Open();

                using (SqlDataAdapter da =
                    new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }
    }
}
