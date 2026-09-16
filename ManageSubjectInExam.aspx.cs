using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class WebForm19 : System.Web.UI.Page
    {


        private readonly string _connStr =
    ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    LoadDropDownLists();
                    LoadGridView();
                    LoadDashboardStats();
                }
            }
            catch (Exception ex)
            {
                ShowError("Page load error: " + ex.Message);
            }

        }

        // ═══════════════════════════════════════════════════════════════════════
        //  DROP-DOWN LOADERS
        // ═══════════════════════════════════════════════════════════════════════
        private void LoadDropDownLists()
        {
            try
            {
                LoadExams();
                LoadSubjects();
                
                LoadFilterClasses();
            }
            catch (SqlException sqlex)
            {
                ShowError("Database error while loading dropdowns: " + sqlex.Message);
            }
            catch (Exception ex)
            {
                ShowError("Error loading dropdowns: " + ex.Message);
            }
        }

        private void LoadExams()
        {
            const string sql = @"
                SELECT ExamID, ExamName
                FROM   Exams
                ORDER  BY ExamName";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                ddlExam.DataSource = dt;
                ddlExam.DataTextField = "ExamName";
                ddlExam.DataValueField = "ExamID";
                ddlExam.DataBind();
                ddlExam.Items.Insert(0, new ListItem("- Select Exam -", ""));
            }
        }

        private void LoadSubjects()
        {
            const string sql = @"
                SELECT SubjectID, SubjectName
                FROM   Subjects
                ORDER  BY SubjectName";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                ddlSubject.DataSource = dt;
                ddlSubject.DataTextField = "SubjectName";
                ddlSubject.DataValueField = "SubjectID";
                ddlSubject.DataBind();
                ddlSubject.Items.Insert(0, new ListItem("- Select Subject -", ""));
            }
        }
               
        private void LoadFilterClasses()
        {
            const string sql = @"
                SELECT ClassID, ClassName
                FROM   Classes
                ORDER  BY ClassName";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());

                ddlFilterClass.DataSource = dt;
                ddlFilterClass.DataTextField = "ClassName";
                ddlFilterClass.DataValueField = "ClassID";
                ddlFilterClass.DataBind();
                ddlFilterClass.Items.Insert(0, new ListItem("All Classes", "0"));
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  DASHBOARD STATS
        // ═══════════════════════════════════════════════════════════════════════
        private void LoadDashboardStats()
        {
            try
            {
                const string sql = @"
                    SELECT
                        COUNT(*)                          AS TotalSubjects,
                        COUNT(DISTINCT cs.ClassID)         AS TotalClasses,
                        COUNT(DISTINCT es.ExamID)         AS TotalExams,
                        ISNULL(AVG(es.TotalMarks), 0)     AS AvgMarks
                    FROM  ExamSubjects es
                    LEFT JOIN Subjects s ON s.SubjectID = es.SubjectID
                    LEFT JOIN ClassSubjects cs on cs.SubjectID = s.SubjectID
                    LEFT JOIN Exams    e ON e.ExamID    = es.ExamID";

                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            lblTotalSubjects.Text = rdr["TotalSubjects"].ToString();
                            lblTotalClasses.Text = rdr["TotalClasses"].ToString();
                            lblTotalExams.Text = rdr["TotalExams"].ToString();
                            lblAvgMarks.Text = rdr["AvgMarks"].ToString();
                        }
                    }
                }
            }
            catch (SqlException sqlex)
            {
                ShowError("Stats load error: " + sqlex.Message);
            }
            catch (Exception ex)
            {
                ShowError("Stats load error: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  GRIDVIEW LOADER
        // ═══════════════════════════════════════════════════════════════════════
        private void LoadGridView(int classFilter = 0)
        {
            try
            {
                string sql = @"
                    SELECT
                        es.ExamSubjectID,
                        e.ExamName,
                      
                        ISNULL(c.ClassName, '')             AS ClassName,
                        s.SubjectName,
                        es.TotalMarks
                       
                    FROM  ExamSubjects es
                    INNER JOIN Exams    e ON e.ExamID    = es.ExamID
                    INNER JOIN Subjects s ON s.SubjectID = es.SubjectID
                    INNER JOIN ClassSubjects cs on cs.SubjectID = s.SubjectID
                    INNER JOIN Classes c on c.ClassID = cs.ClassID
                    WHERE (@ClassID = 0 OR cs.ClassID = @ClassID)
                    ORDER BY e.ExamName, s.SubjectName";

                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classFilter);
                    conn.Open();
                    var dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    gvExamSubjects.DataSource = dt;
                    gvExamSubjects.DataBind();
                    lblRecordCount.Text = dt.Rows.Count.ToString();
                }
            }
            catch (SqlException sqlex)
            {
                ShowError("Grid load error: " + sqlex.Message);
            }
            catch (Exception ex)
            {
                ShowError("Grid load error: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  SAVE  (INSERT)
        // ═══════════════════════════════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                // Duplicate check
                if (RecordExists(int.Parse(ddlExam.SelectedValue),
                                 int.Parse(ddlSubject.SelectedValue), 0))
                {
                    ShowError("This subject is already assigned to the selected exam.");
                    return;
                }

                const string sql = @"
                    INSERT INTO ExamSubjects (ExamID, SubjectID, TotalMarks)
                    VALUES (@ExamID, @SubjectID, @TotalMarks)";

                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ExamID", int.Parse(ddlExam.SelectedValue));
                    cmd.Parameters.AddWithValue("@SubjectID", int.Parse(ddlSubject.SelectedValue));
                    cmd.Parameters.AddWithValue("@TotalMarks", int.Parse(txtTotalMarks.Text.Trim()));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                ShowSuccess("Exam subject saved successfully.");
                ClearForm();
                LoadGridView(int.Parse(ddlFilterClass.SelectedValue));
                LoadDashboardStats();
            }
            catch (SqlException sqlex)
            {
                ShowError("Database error while saving: " + sqlex.Message);
            }
            catch (FormatException)
            {
                ShowError("Invalid number format. Please check marks and year fields.");
            }
            catch (Exception ex)
            {
                ShowError("Unexpected error while saving: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  UPDATE
        // ═══════════════════════════════════════════════════════════════════════
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int id;
            if (!int.TryParse(hfExamSubjectID.Value, out id) || id == 0)
            {
                ShowError("No record selected for update.");
                return;
            }

            try
            {
                // Duplicate check (excluding current record)
                if (RecordExists(int.Parse(ddlExam.SelectedValue),
                                 int.Parse(ddlSubject.SelectedValue), id))
                {
                    ShowError("Another record already has this exam-subject combination.");
                    return;
                }

                const string sql = @"
                    UPDATE ExamSubjects
                    SET    ExamID       = @ExamID,
                           SubjectID   = @SubjectID,
                           TotalMarks  = @TotalMarks
                         
                    WHERE  ExamSubjectID = @ID";

                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    cmd.Parameters.AddWithValue("@ExamID", int.Parse(ddlExam.SelectedValue));
                    cmd.Parameters.AddWithValue("@SubjectID", int.Parse(ddlSubject.SelectedValue));
                    cmd.Parameters.AddWithValue("@TotalMarks", int.Parse(txtTotalMarks.Text.Trim()));

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        ShowSuccess("Record updated successfully.");
                        ClearForm();
                        LoadGridView(int.Parse(ddlFilterClass.SelectedValue));
                        LoadDashboardStats();
                    }
                    else
                    {
                        ShowError("No record was updated. It may have been deleted.");
                    }
                }
            }
            catch (SqlException sqlex)
            {
                ShowError("Database error while updating: " + sqlex.Message);
            }
            catch (FormatException)
            {
                ShowError("Invalid number format. Please check marks and year fields.");
            }
            catch (Exception ex)
            {
                ShowError("Unexpected error while updating: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  DELETE
        // ═══════════════════════════════════════════════════════════════════════
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(hfExamSubjectID.Value, out id) || id == 0)
            {
                ShowError("No record selected for deletion.");
                return;
            }

            try
            {
                const string sql = "DELETE FROM ExamSubjects WHERE ExamSubjectID = @ID";

                using (var conn = new SqlConnection(_connStr))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        ShowSuccess("Record deleted successfully.");
                        ClearForm();
                        LoadGridView(int.Parse(ddlFilterClass.SelectedValue));
                        LoadDashboardStats();
                    }
                    else
                    {
                        ShowError("Record not found or already deleted.");
                    }
                }
            }
            catch (SqlException sqlex)
            {
                // FK violation - record referenced elsewhere
                if (sqlex.Number == 547)
                    ShowError("Cannot delete: this record is referenced by other data.");
                else
                    ShowError("Database error while deleting: " + sqlex.Message);
            }
            catch (Exception ex)
            {
                ShowError("Unexpected error while deleting: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  CLEAR FORM
        // ═══════════════════════════════════════════════════════════════════════
        protected void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                ClearForm();
                HideAlerts();
            }
            catch (Exception ex)
            {
                ShowError("Error clearing form: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            ddlExam.SelectedIndex = 0;
            ddlSubject.SelectedIndex = 0;
           
            txtTotalMarks.Text = string.Empty;
            hfExamSubjectID.Value = "0";

            btnSave.Visible = true;
            btnUpdate.Visible = false;
            btnDelete.Visible = false;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  GRIDVIEW EVENTS
        // ═══════════════════════════════════════════════════════════════════════
        protected void gvExamSubjects_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvExamSubjects.PageIndex = e.NewPageIndex;
                LoadGridView(int.Parse(ddlFilterClass.SelectedValue));
            }
            catch (Exception ex)
            {
                ShowError("Paging error: " + ex.Message);
            }
        }

        protected void gvExamSubjects_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditRow") return;

            try
            {
                int id = int.Parse(e.CommandArgument.ToString());
                LoadRecordForEdit(id);
            }
            catch (Exception ex)
            {
                ShowError("Error loading record for edit: " + ex.Message);
            }
        }

        private void LoadRecordForEdit(int id)
        {
            const string sql = @"
                SELECT ExamID, SubjectID, TotalMarks
                FROM   ExamSubjects
                WHERE  ExamSubjectID = @ID";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        hfExamSubjectID.Value = id.ToString();
                        ddlExam.SelectedValue = rdr["ExamID"].ToString();
                        ddlSubject.SelectedValue = rdr["SubjectID"].ToString();
                        txtTotalMarks.Text = rdr["TotalMarks"].ToString();

                        btnSave.Visible = false;
                        btnUpdate.Visible = true;
                        btnDelete.Visible = true;

                        // Scroll to form
                        ScriptManager.RegisterStartupScript(this, GetType(), "scroll",
                            "window.scrollTo({top:0,behavior:'smooth'});", true);
                    }
                }
            }
        }

        // ── Filter by class ────────────────────────────────────────────────────
        protected void ddlFilterClass_Changed(object sender, EventArgs e)
        {
            try
            {
                gvExamSubjects.PageIndex = 0;
                LoadGridView(int.Parse(ddlFilterClass.SelectedValue));
            }
            catch (Exception ex)
            {
                ShowError("Filter error: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  HELPER METHODS
        // ═══════════════════════════════════════════════════════════════════════
        private bool RecordExists(int examId, int subjectId, int excludeId)
        {
            const string sql = @"
                SELECT COUNT(*) FROM ExamSubjects
                WHERE  ExamID     = @ExamID
                AND    SubjectID  = @SubjectID
                AND    ExamSubjectID <> @ExcludeID";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ExamID", examId);
                cmd.Parameters.AddWithValue("@SubjectID", subjectId);
                cmd.Parameters.AddWithValue("@ExcludeID", excludeId);
                conn.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private void ShowSuccess(string message)
        {
            pnlSuccess.Visible = true;
            pnlError.Visible = false;
            lblSuccess.Text = message;
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            pnlSuccess.Visible = false;
            lblError.Text = message;
        }

        private void HideAlerts()
        {
            pnlSuccess.Visible = false;
            pnlError.Visible = false;
        }

        // ═══════════════════════════════════════════════════════════════════════
        //  TEMPLATE FIELD HELPERS  (called from .aspx markup)
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>Returns % width for the marks progress bar (max baseline = 1000).</summary>
        protected string GetMarksPercent(object marks)
        {
            try
            {
                int m = Convert.ToInt32(marks);
                int pct = Math.Min((int)Math.Round(m / 1000.0 * 100), 100);
                return pct.ToString();
            }
            catch { return "0"; }
        }

        protected string GetStatus(string year)
        {
            try
            {
                int y = int.Parse(year);
                int cur = DateTime.Now.Year;
                if (y == cur) return "Current";
                if (y > cur) return "Upcoming";
                return "Past";
            }
            catch { return "-"; }
        }

        protected string GetStatusBadge(string year)
        {
            string status = GetStatus(year);
            switch (status)
            {
                case "Current": return "badge-green";
                case "Upcoming": return "badge-blue";
                default: return "badge-orange";
            }
        }
    }
}
