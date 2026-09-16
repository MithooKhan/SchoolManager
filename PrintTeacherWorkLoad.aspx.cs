using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web;
using System.Web.Script.Serialization;

namespace DigitalSchoolManager
{
    public partial class WebForm23 : System.Web.UI.Page
    {
        // A normal class is used instead of System.ValueTuple so this page also
        // runs on Windows installations that only provide the .NET 4.7.2 runtime.
        private sealed class WorkloadPeriod
        {
            internal int BellID { get; set; }
            internal string Name { get; set; }
        }

        private string CS => ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    SchoolLifecycleService.EnsureSchema();
                    EnsureSchema();
                  
                    LoadFilterDropDowns();
                    BindAll();
                }
            }
            catch (SqlException ex) { ShowMsg("Database error on load: " + ex.Message, false); }
            catch (Exception ex) { ShowMsg("Page load error: " + ex.Message, false); }
        }

        // ════════════════════════════════════════════════════════════════════
        //  DROPDOWN LOADERS
        // ════════════════════════════════════════════════════════════════════

        /// <summary>Loads entry-form dropdowns (Teacher, Class, Subject, Bell).</summary>
      

        /// <summary>Loads filter bar dropdowns (Teacher, Designation, Subject).</summary>
        private void LoadFilterDropDowns()
        {
            // Save currently selected values so a postback reset keeps them
            string fT = ddlFltTeacher.SelectedValue;
            string fD = ddlFltDesig.SelectedValue;
            string fS = ddlFltSubject.SelectedValue;

            FillDDL(ddlFltTeacher,
                "SELECT TeacherID, Name FROM Teachers WHERE ISNULL(IsActive,1)=1 ORDER BY Name",
                "TeacherID", "Name", "All Teachers");

            // Designations from TeachingVacancyPosition
            FillDDL(ddlFltDesig,
                "SELECT PostID, Description FROM TeachingVacancyPosition ORDER BY BPS DESC",
                "PostID", "Description", "All Designations");

            FillDDL(ddlFltSubject,
                "SELECT SubjectID, SubjectName FROM Subjects ORDER BY SubjectName",
                "SubjectID", "SubjectName", "All Subjects");

            TrySetDDL(ddlFltTeacher, fT);
            TrySetDDL(ddlFltDesig, fD);
            TrySetDDL(ddlFltSubject, fS);
        }

        // ════════════════════════════════════════════════════════════════════
        //  BIND ALL - master refresh
        // ════════════════════════════════════════════════════════════════════
        private void BindAll()
        {
            var workloadDt = LoadWorkloadData();
            BindWorkloadGrid(workloadDt);
            BuildKpiCards(workloadDt);
            BuildSubjectChart();
            BuildPeriodChart();
            BuildHeatmap();
            BuildTeacherJson(workloadDt);
            litPrintDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy  hh:mm tt");
        }

        // ════════════════════════════════════════════════════════════════════
        //  WORKLOAD DATA QUERY
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Aggregated workload per teacher with Designation / BPS.
        /// Applies filter bar values.
        /// Returns columns:
        ///   TeacherID, TeacherName, Designation, BPS,
        ///   PeriodCount, ClassCount, SubjectList, SubjectCount,
        ///   PeriodSlots (pipe-sep), BarPct
        /// </summary>
        private DataTable LoadWorkloadData()
        {
            try
            {
                // ── Build WHERE conditions ───────────────────────────────────
                var where = new List<string>();

                if (int.TryParse(ddlFltTeacher.SelectedValue, out int fTid) && fTid > 0)
                    where.Add($"tt.TeacherID = {fTid}");

                if (int.TryParse(ddlFltDesig.SelectedValue, out int fDid) && fDid > 0)
                    where.Add($"t.PostID = {fDid}");

                if (int.TryParse(ddlFltSubject.SelectedValue, out int fSid) && fSid > 0)
                    where.Add($"tt.SubjectID = {fSid}");

                string whereClause = where.Count > 0
                    ? "WHERE " + string.Join(" AND ", where)
                    : "";

                // ── Min-periods filter is applied after aggregation ──────────
                int minPeriods = 0;
                int.TryParse(txtFltMinP.Text.Trim(), out minPeriods);

                string sql = $@"
                    SELECT
                        t.TeacherID,
                        t.Name                                          AS TeacherName,
                        ISNULL(vp.Description, '')                      AS Designation,
                        ISNULL(CAST(vp.BPS AS NVARCHAR(10)), '')        AS BPS,
                        COUNT(DISTINCT tt.TimeTableID)                  AS PeriodCount,
                        COUNT(DISTINCT tt.ClassID)                      AS ClassCount,
                        -- comma-separated unique subjects
                        STUFF((
                            SELECT DISTINCT ', ' + s2.SubjectName
                            FROM   TimeTable tt2
                            INNER  JOIN Subjects s2 ON s2.SubjectID = tt2.SubjectID
                            WHERE  tt2.TeacherID = t.TeacherID
                            FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 2, '')
                                                                        AS SubjectList,
                        -- comma-separated class names
                        STUFF((
                            SELECT DISTINCT ', ' + c2.ClassName
                            FROM   TimeTable tt3
                            INNER  JOIN Classes c2 ON c2.ClassID = tt3.ClassID
                            WHERE  tt3.TeacherID = t.TeacherID
                            FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 2, '')
                                                                        AS ClassNames,
                        -- pipe-separated period slots
                        STUFF((
                            SELECT DISTINCT '|' + b2.PeriodName
                            FROM   TimeTable tt4
                            INNER  JOIN BellTimeTable b2 ON b2.BellID = tt4.BellID
                            WHERE  tt4.TeacherID = t.TeacherID
                            FOR XML PATH(''), TYPE).value('.','NVARCHAR(MAX)'), 1, 1, '')
                                                                        AS PeriodSlots
                    FROM   TimeTable tt
                    INNER  JOIN Teachers t              ON t.TeacherID  = tt.TeacherID AND ISNULL(t.IsActive,1)=1
                    INNER  JOIN Subjects s              ON s.SubjectID  = tt.SubjectID
                    INNER  JOIN Classes  c              ON c.ClassID    = tt.ClassID
                    INNER  JOIN BellTimeTable b         ON b.BellID     = tt.BellID
                    LEFT   JOIN TeachingVacancyPosition vp ON vp.PostID = t.PostID
                    {whereClause}
                    GROUP  BY t.TeacherID, t.Name, vp.Description, vp.BPS
                    ORDER  BY ISNULL(vp.BPS, 0) DESC, COUNT(DISTINCT tt.TimeTableID) DESC, t.Name";

                var dt = new DataTable();
                using (var con = new SqlConnection(CS))
                using (var da = new SqlDataAdapter(sql, con))
                    da.Fill(dt);

                // ── Add computed columns ─────────────────────────────────────
                dt.Columns.Add("BarPct", typeof(double));
                dt.Columns.Add("SubjectCount", typeof(int));

                // Max periods for bar scaling
                int maxP = 1;
                foreach (DataRow r in dt.Rows)
                    if (Convert.ToInt32(r["PeriodCount"]) > maxP)
                        maxP = Convert.ToInt32(r["PeriodCount"]);

                // Apply min-periods filter and compute bar pct
                var toRemove = new List<DataRow>();
                foreach (DataRow r in dt.Rows)
                {
                    int pc = Convert.ToInt32(r["PeriodCount"]);
                    if (minPeriods > 0 && pc < minPeriods) { toRemove.Add(r); continue; }
                    r["BarPct"] = Math.Round((double)pc / maxP * 100, 1);
                    string sl = r["SubjectList"].ToString();
                    r["SubjectCount"] = string.IsNullOrEmpty(sl) ? 0 : sl.Split(',').Length;
                }
                foreach (var r in toRemove) dt.Rows.Remove(r);

                return dt;
            }
            catch (SqlException ex)
            {
                ShowMsg("Error loading workload data: " + ex.Message, false);
                return new DataTable();
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  GRIDVIEW BIND
        // ════════════════════════════════════════════════════════════════════
        private void BindWorkloadGrid(DataTable dt)
        {
            gvWorkload.DataSource = dt;
            gvWorkload.DataBind();
            lblRowCount.Text = dt.Rows.Count + " teacher" + (dt.Rows.Count != 1 ? "s" : "");
        }

        protected void gvWorkload_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            var drv = e.Row.DataItem as DataRowView;
            if (drv == null) return;

            int pc = Convert.ToInt32(drv["PeriodCount"]);
            double avgLoad = GetAverageLoad();
            var litStatus = e.Row.FindControl("litStatus") as Literal;
            if (litStatus == null) return;

            if (pc == 0)
                litStatus.Text = "<span class='badge badge-grey'>Unassigned</span>";
            else if (avgLoad > 0 && pc >= avgLoad * 1.5)
                litStatus.Text = "<span class='badge badge-red'> Overloaded</span>";
            else if (avgLoad > 0 && pc >= avgLoad * 1.2)
                litStatus.Text = "<span class='badge badge-amber'>High Load</span>";
            else
                litStatus.Text = "<span class='badge badge-green'> Balanced</span>";
        }

        protected void gvWorkload_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(e.CommandArgument?.ToString(), out int teacherID)) return;
           
        }

        // ════════════════════════════════════════════════════════════════════
        //  KPI CARDS
        // ════════════════════════════════════════════════════════════════════
        private void BuildKpiCards(DataTable dt)
        {
            int teacherCount = dt.Rows.Count;
            int totalPeriods = 0;
            int maxP = 0;
            string maxName = "-";
            int overloaded = 0;
            double avg = 0;

            foreach (DataRow r in dt.Rows)
            {
                int pc = Convert.ToInt32(r["PeriodCount"]);
                totalPeriods += pc;
                if (pc > maxP) { maxP = pc; maxName = r["TeacherName"].ToString(); }
            }

            if (teacherCount > 0)
            {
                avg = Math.Round((double)totalPeriods / teacherCount, 1);
                foreach (DataRow r in dt.Rows)
                    if (Convert.ToInt32(r["PeriodCount"]) >= avg * 1.5) overloaded++;
            }

          

            // Summary cards
            lblCardTeachers.Text = teacherCount.ToString();
            lblCardPeriods.Text = totalPeriods.ToString();
            lblCardAvg.Text = avg.ToString("0.0");
            lblCardMax.Text = maxP.ToString();
            lblCardMaxName.Text = maxName.Length > 20 ? maxName.Substring(0, 18) + "..." : maxName;
            lblCardOverloaded.Text = overloaded.ToString();
        }

        // ════════════════════════════════════════════════════════════════════
        //  SUBJECT DISTRIBUTION CHART
        // ════════════════════════════════════════════════════════════════════
        private void BuildSubjectChart()
        {
            try
            {
                string sql = @"
                    SELECT s.SubjectName, COUNT(*) AS Cnt
                    FROM   TimeTable tt
                    INNER  JOIN Subjects s ON s.SubjectID = tt.SubjectID
                    GROUP  BY s.SubjectName
                    ORDER  BY Cnt DESC";

                var dt = new DataTable();
                using (var con = new SqlConnection(CS))
                using (var da = new SqlDataAdapter(sql, con))
                    da.Fill(dt);

                if (dt.Rows.Count == 0) { litSubjChart.Text = "<div style='color:#94a3b8;font-size:13px'>No data.</div>"; return; }

                int maxCnt = Convert.ToInt32(dt.Rows[0]["Cnt"]);
                if (maxCnt < 1) maxCnt = 1;

                var sb = new StringBuilder();
                foreach (DataRow r in dt.Rows)
                {
                    string name = H(r["SubjectName"].ToString());
                    int cnt = Convert.ToInt32(r["Cnt"]);
                    double pct = Math.Round((double)cnt / maxCnt * 100, 1);
                    sb.Append($@"
<div class='subj-row'>
  <div class='subj-name' title='{name}'>{TruncStr(r["SubjectName"].ToString(), 16)}</div>
  <div class='subj-bar-wrap'>
    <div class='subj-bar' data-pct='{pct}' style='width:0%'></div>
  </div>
  <div class='subj-count'>{cnt}</div>
</div>");
                }
                litSubjChart.Text = sb.ToString();
            }
            catch { litSubjChart.Text = "<div style='color:#dc2626;font-size:12px'>Chart unavailable.</div>"; }
        }

        // ════════════════════════════════════════════════════════════════════
        //  PERIOD UTILISATION CHART
        // ════════════════════════════════════════════════════════════════════
        private void BuildPeriodChart()
        {
            try
            {
                string sql = @"
                    SELECT b.PeriodName, COUNT(DISTINCT tt.TeacherID) AS TeacherCount
                    FROM   BellTimeTable b
                    LEFT   JOIN TimeTable tt ON tt.BellID = b.BellID
                    GROUP  BY b.BellID, b.PeriodName
                    ORDER  BY b.BellID";

                var dt = new DataTable();
                using (var con = new SqlConnection(CS))
                using (var da = new SqlDataAdapter(sql, con))
                    da.Fill(dt);

                if (dt.Rows.Count == 0) { litPeriodChart.Text = "<div style='color:#94a3b8;font-size:13px'>No data.</div>"; return; }

                int maxTC = 1;
                foreach (DataRow r in dt.Rows)
                    if (Convert.ToInt32(r["TeacherCount"]) > maxTC)
                        maxTC = Convert.ToInt32(r["TeacherCount"]);

                var sb = new StringBuilder();
                foreach (DataRow r in dt.Rows)
                {
                    string pname = H(r["PeriodName"].ToString());
                    int tc = Convert.ToInt32(r["TeacherCount"]);
                    double pct = Math.Round((double)tc / maxTC * 100, 1);
                    sb.Append($@"
<div class='pu-row'>
  <div class='pu-label'>{TruncStr(r["PeriodName"].ToString(), 10)}</div>
  <div class='pu-track'>
    <div class='pu-fill' data-pct='{pct}' style='width:0%'></div>
    <span class='pu-count'>{tc}</span>
  </div>
  <div class='pu-num'>{tc}</div>
</div>");
                }
                litPeriodChart.Text = sb.ToString();
            }
            catch { litPeriodChart.Text = "<div style='color:#dc2626;font-size:12px'>Chart unavailable.</div>"; }
        }

        // ════════════════════════════════════════════════════════════════════
        //  SCHEDULE HEATMAP  (Teacher rows x Period columns)
        // ════════════════════════════════════════════════════════════════════
        private void BuildHeatmap()
        {
            try
            {
                // Load all assignments: TeacherName, BellID, PeriodName, ClassName
                string sql = @"
                    SELECT t.Name AS TeacherName, b.BellID, b.PeriodName,
                           c.ClassName, s.SubjectName
                    FROM   TimeTable tt
                    INNER  JOIN Teachers      t  ON t.TeacherID  = tt.TeacherID AND ISNULL(t.IsActive,1)=1
                    INNER  JOIN BellTimeTable b  ON b.BellID     = tt.BellID
                    INNER  JOIN Classes       c  ON c.ClassID    = tt.ClassID
                    INNER  JOIN Subjects      s  ON s.SubjectID  = tt.SubjectID
                    ORDER  BY t.Name, b.BellID";

                var dt = new DataTable();
                using (var con = new SqlConnection(CS))
                using (var da = new SqlDataAdapter(sql, con))
                    da.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    litHeatmap.Text = "<div style='color:#94a3b8;padding:20px;font-size:13px'>No schedule data available.</div>";
                    return;
                }

                // Collect periods and teachers
                var periods = new List<WorkloadPeriod>();
                var seenB = new HashSet<int>();
                var teachers = new List<string>();
                var seenT = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (DataRow r in dt.Rows)
                {
                    int bid = Convert.ToInt32(r["BellID"]);
                    if (seenB.Add(bid))
                    {
                        periods.Add(new WorkloadPeriod
                        {
                            BellID = bid,
                            Name = r["PeriodName"].ToString()
                        });
                    }
                    string tn = r["TeacherName"].ToString();
                    if (seenT.Add(tn)) teachers.Add(tn);
                }
                periods.Sort((a, b) => a.BellID.CompareTo(b.BellID));

                // Build cell lookup: teacher|bellID -> "ClassName\nSubject"
                var cells = new Dictionary<string, List<string>>();
                foreach (DataRow r in dt.Rows)
                {
                    string key = r["TeacherName"] + "|" + r["BellID"];
                    if (!cells.ContainsKey(key)) cells[key] = new List<string>();
                    cells[key].Add(r["ClassName"] + "\n" + r["SubjectName"]);
                }

                // Render heat table
                var sb = new StringBuilder();
                sb.Append("<table class='heat-table'><thead><tr><th></th>");
                foreach (var p in periods)
                    sb.Append($"<th>{H(TruncStr(p.Name, 8))}</th>");
                sb.Append("</tr></thead><tbody>");

                foreach (string tname in teachers)
                {
                    sb.Append($"<tr><td class='heat-name'>{H(TruncStr(tname, 22))}</td>");
                    foreach (var p in periods)
                    {
                        string key = tname + "|" + p.BellID;
                        if (cells.TryGetValue(key, out var entries) && entries.Count > 0)
                        {
                            // Show class + subject in tooltip-style title
                            string tip = string.Join("; ", entries).Replace("\"", "'");
                            // Abbreviate display: first class only to keep cell compact
                            string display = entries[0].Replace("\n", " · ");
                            if (entries.Count > 1) display += $" +{entries.Count - 1}";
                            string cls2 = entries.Count > 1 ? "heat-busy-2" : "heat-busy";
                            sb.Append($"<td class='heat-cell {cls2}' title=\"{H(tip)}\">{H(TruncStr(display, 10))}</td>");
                        }
                        else
                            sb.Append("<td class='heat-cell heat-empty'>-</td>");
                    }
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table>");
                litHeatmap.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                litHeatmap.Text = $"<div style='color:#dc2626;padding:12px;font-size:12px'>Heatmap error: {H(ex.Message)}</div>";
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  TEACHER JSON  (for modal detail popup)
        // ════════════════════════════════════════════════════════════════════
        private void BuildTeacherJson(DataTable dt)
        {
            try
            {
                double avg = GetAverageLoadFromDt(dt);
                var list = new List<Dictionary<string, object>>();

                foreach (DataRow r in dt.Rows)
                {
                    int pc = Convert.ToInt32(r["PeriodCount"]);
                    string status = pc == 0 ? "Unassigned"
                                  : avg > 0 && pc >= avg * 1.5 ? "Overloaded"
                                  : avg > 0 && pc >= avg * 1.2 ? "High Load"
                                  : "Balanced";

                    // Convert pipe-separated slots to readable list
                    string slots = r["PeriodSlots"].ToString().Replace("|", ", ");

                    list.Add(new Dictionary<string, object>
                    {
                        ["id"] = r["TeacherID"],
                        ["name"] = r["TeacherName"].ToString(),
                        ["desig"] = r["Designation"].ToString(),
                        ["bps"] = r["BPS"].ToString(),
                        ["periods"] = pc,
                        ["classes"] = r["ClassNames"].ToString(),
                        ["subjects"] = r["SubjectList"].ToString(),
                        ["slots"] = slots,
                        ["status"] = status
                    });
                }

                var js = new JavaScriptSerializer();
                hfTeacherJson.Value = js.Serialize(list);
            }
            catch { hfTeacherJson.Value = "[]"; }
        }   
     
      
    
        
        protected void Filter_Changed(object sender, EventArgs e) { BindAll(); }

        protected void btnApplyFilter_Click(object sender, EventArgs e) { BindAll(); }

        protected void btnResetFilter_Click(object sender, EventArgs e)
        {
            ddlFltTeacher.SelectedValue = "";
            ddlFltDesig.SelectedValue = "";
            ddlFltSubject.SelectedValue = "";
            txtFltMinP.Text = "";
            BindAll();
        }

      
        private SqlCommand BuildCmd(SqlConnection con, string sql, int t, int c, int s, int b, int x)
        {
            var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@T", t);
            cmd.Parameters.AddWithValue("@C", c);
            cmd.Parameters.AddWithValue("@S", s);
            cmd.Parameters.AddWithValue("@B", b);
            cmd.Parameters.AddWithValue("@X", x);
            return cmd;
        }

        // ════════════════════════════════════════════════════════════════════
        //  ASPX CODE-ACCESSIBLE HELPERS  (called from <%# %> in ASPX)
        // ════════════════════════════════════════════════════════════════════

        /// <summary>Returns initials from a full name (up to 2 chars).</summary>
        public string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        /// <summary>Returns CSS badge class based on BPS grade.</summary>
        public string GetBpsBadgeClass(string bps)
        {
            if (!int.TryParse(bps, out int b)) return "badge-grey";
            if (b >= 18) return "badge-purple";
            if (b >= 16) return "badge-teal";
            if (b >= 14) return "badge-green";
            if (b >= 11) return "badge-amber";
            return "badge-grey";
        }

        /// <summary>Returns bar CSS class based on period count vs average.</summary>
        public string GetBarClass(object periodCountObj)
        {
            if (periodCountObj == null) return "bar-low";
            int pc = Convert.ToInt32(periodCountObj);
            double avg = GetAverageLoad();
            if (avg <= 0) return "bar-fill";
            if (pc >= avg * 1.5) return "bar-high";
            if (pc >= avg * 1.2) return "bar-med";
            if (pc == 0) return "bar-low";
            return "";   // normal green gradient from base class
        }

        // ════════════════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ════════════════════════════════════════════════════════════════════

       

        private void ClearForm()
        {
            hfEditID.Value = "0";
           
        }

        private void SetMode(bool editing)
        {
           
        }

        private void ShowMsg(string msg, bool ok)
        {
            pnlMsg.Visible = true;
            alertBox.Attributes["class"] = ok ? "wl-alert wl-alert-ok" : "wl-alert wl-alert-err";
            litIco.Text = ok ? "" : "";
            litMsg.Text = msg;
            upMsg.Update();
        }

       
      

        private void FillDDL(DropDownList ddl, string sql, string valField, string txtField, string blank)
        {
            try
            {
                var dt = new DataTable();
                using (var con = new SqlConnection(CS))
                using (var da = new SqlDataAdapter(sql, con))
                    da.Fill(dt);

                ddl.Items.Clear();
                ddl.Items.Add(new ListItem(blank, ""));
                foreach (DataRow r in dt.Rows)
                    ddl.Items.Add(new ListItem(r[txtField].ToString(), r[valField].ToString()));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FillDDL error ({ddl.ID}): {ex.Message}");
            }
        }

        private void TrySetDDL(DropDownList ddl, string val)
        {
            if (!string.IsNullOrEmpty(val) && ddl.Items.FindByValue(val) != null)
                ddl.SelectedValue = val;
        }

        /// <summary>Returns average period count across all teachers in the TimeTable.</summary>
        private double GetAverageLoad()
        {
            try
            {
                string sql = @"SELECT AVG(CAST(cnt AS FLOAT)) FROM
                               (SELECT COUNT(*) AS cnt FROM TimeTable GROUP BY TeacherID) AS T";
                using (var con = new SqlConnection(CS))
                {
                    var cmd = new SqlCommand(sql, con);
                    con.Open();
                    var val = cmd.ExecuteScalar();
                    return val == DBNull.Value || val == null ? 0 : Convert.ToDouble(val);
                }
            }
            catch { return 0; }
        }

        private double GetAverageLoadFromDt(DataTable dt)
        {
            if (dt.Rows.Count == 0) return 0;
            double total = 0;
            foreach (DataRow r in dt.Rows) total += Convert.ToInt32(r["PeriodCount"]);
            return total / dt.Rows.Count;
        }

        private static string H(string s)
            => System.Web.HttpUtility.HtmlEncode(s ?? "");

        private static string TruncStr(string s, int maxLen)
            => (s ?? "").Length > maxLen ? s.Substring(0, maxLen - 1) + "..." : s;

        // ════════════════════════════════════════════════════════════════════
        //  ENSURE SCHEMA
        //  Creates tables if they don't exist and patches Teachers with PostID.
        // ════════════════════════════════════════════════════════════════════
        private void EnsureSchema()
        {
            try
            {
                string sql = @"
-- TeachingVacancyPosition
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='TeachingVacancyPosition')
BEGIN
  CREATE TABLE TeachingVacancyPosition(
    PostID      INT IDENTITY(1,1) PRIMARY KEY,
    Description NVARCHAR(150) NOT NULL,
    BPS         INT NOT NULL);
  INSERT INTO TeachingVacancyPosition(Description, BPS) VALUES
    ('Professor',              21),
    ('Associate Professor',    20),
    ('Assistant Professor',    18),
    ('Lecturer (SSS)',         17),
    ('Senior Subject Specialist', 17),
    ('Subject Specialist',     16),
    ('Senior Teacher (High)',  16),
    ('Senior Teacher',         15),
    ('Teacher (High School)',  14),
    ('Teacher (Middle)',       12),
    ('Primary School Teacher', 9);
END;

-- Classes
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Classes')
BEGIN
  CREATE TABLE Classes(ClassID INT IDENTITY(1,1) PRIMARY KEY, ClassName NVARCHAR(50) NOT NULL UNIQUE);
  INSERT INTO Classes(ClassName) VALUES
    ('6th'),('7th'),('8th'),('9th'),('10th'),
    ('11th Pre-Medical'),('11th Pre-Engineering'),('11th General Science'),
    ('12th Pre-Medical'),('12th Pre-Engineering');
END;

-- Teachers
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Teachers')
BEGIN
  CREATE TABLE Teachers(
    TeacherID INT IDENTITY(1,1) PRIMARY KEY,
    Name      NVARCHAR(100) NOT NULL,
    PostID    INT NULL REFERENCES TeachingVacancyPosition(PostID));
  INSERT INTO Teachers(Name, PostID) VALUES
    ('Muhammad Tahir',      3),
    ('Ghulam Kazim',        4),
    ('Altaf Hussain',       5),
    ('Muhammad Islam',      6),
    ('Usman Haider',        7),
    ('Mithoo Khan',         8),
    ('Qaswar Abbas',        9),
    ('Sabir Ali',           10),
    ('Muhammad Safdar',     10),
    ('Ghulam Yaseen Gauher',10);
END;

-- Add PostID to Teachers if missing
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME='Teachers' AND COLUMN_NAME='PostID')
  ALTER TABLE Teachers ADD PostID INT NULL
    REFERENCES TeachingVacancyPosition(PostID);

-- Subjects
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Subjects')
BEGIN
  CREATE TABLE Subjects(SubjectID INT IDENTITY(1,1) PRIMARY KEY, SubjectName NVARCHAR(100) NOT NULL UNIQUE);
  INSERT INTO Subjects(SubjectName) VALUES
    ('Mathematics'),('English Language'),('Urdu'),('Islamiat'),
    ('Pakistan Studies'),('Physics'),('Chemistry'),('Biology'),
    ('Computer Science'),('General Science'),('Holy Quran Translation'),
    ('History & Geography');
END;

-- BellTimeTable
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='BellTimeTable')
BEGIN
  CREATE TABLE BellTimeTable(
    BellID          INT IDENTITY(1,1) PRIMARY KEY,
    PeriodName      NVARCHAR(50) NOT NULL UNIQUE,
    PeriodStartTime NVARCHAR(10) NOT NULL,
    PeriodEndTime   NVARCHAR(10) NOT NULL);
  INSERT INTO BellTimeTable(PeriodName, PeriodStartTime, PeriodEndTime) VALUES
    ('Period 1','09:00','09:40'),
    ('Period 2','09:40','10:10'),
    ('Period 3','10:10','10:50'),
    ('Period 4','10:50','11:20'),
    ('Period 5','11:20','11:50'),
    ('Period 6','11:50','12:20'),
    ('Period 7 (Friday)','12:20','12:50'),
    ('Period 8','12:50','13:20');
END;

-- TimeTable
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='TimeTable')
BEGIN
  CREATE TABLE TimeTable(
    TimeTableID INT IDENTITY(1,1) PRIMARY KEY,
    ClassID     INT NOT NULL REFERENCES Classes(ClassID),
    TeacherID   INT NOT NULL REFERENCES Teachers(TeacherID),
    SubjectID   INT NOT NULL REFERENCES Subjects(SubjectID),
    BellID      INT NOT NULL REFERENCES BellTimeTable(BellID));
END;

-- Rename legacy PK column if needed
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
          WHERE TABLE_NAME='TimeTable' AND COLUMN_NAME='TTID')
  AND NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                 WHERE TABLE_NAME='TimeTable' AND COLUMN_NAME='TimeTableID')
  EXEC sp_rename 'TimeTable.TTID','TimeTableID','COLUMN';

-- TeachersClass (for class incharge)
IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='TeachersClass')
BEGIN
  CREATE TABLE TeachersClass(
    SecID       INT IDENTITY(1,1) PRIMARY KEY,
    ClassID     INT NOT NULL REFERENCES Classes(ClassID),
    InchargeID  INT NOT NULL REFERENCES Teachers(TeacherID));
END;";

                using (var con = new SqlConnection(CS))
                using (var cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EnsureSchema error: " + ex.Message);
            }
        }

    }
}
