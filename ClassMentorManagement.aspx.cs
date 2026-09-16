using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace DigitalSchoolManager
{
    public partial class WebForm12 : System.Web.UI.Page
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    SchoolLifecycleService.EnsureSchema();
                    LoadClassesDropDown();
                    LoadTeachersDropDown();
                    LoadAssignmentsGrid();
                    HideMessage();
                }
            }
            catch (SqlException sqlEx)
            {
                ShowMessage($"Database error on page load: {sqlEx.Message}", isError: true);
            }
            catch (Exception ex)
            {
                ShowMessage($"Unexpected error: {ex.Message}", isError: true);
            }

        }


        // ══════════════════════════════════════════════════════════════════════
        //  HELPER - Load Classes Dropdown
        // ══════════════════════════════════════════════════════════════════════
        private void LoadClassesDropDown()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    const string sql = @"
                        SELECT c.ClassID, c.ClassName
                        FROM   Classes c
                        ORDER  BY c.ClassName";

                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlClass.DataSource = dt;
                        ddlClass.DataTextField = "ClassName";
                        ddlClass.DataValueField = "ClassID";
                        ddlClass.DataBind();
                        ddlClass.Items.Insert(0, new ListItem("- Select Class -", "0"));
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new ApplicationException("Failed to load classes: " + sqlEx.Message, sqlEx);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPER - Load Teachers Dropdown
        // ══════════════════════════════════════════════════════════════════════
        private void LoadTeachersDropDown()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    const string sql = @"
                        SELECT TeacherID,
                               Name AS DisplayName
                        FROM   Teachers
                        WHERE  ISNULL(IsActive,1)=1
                        ORDER  BY Name";

                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlTeacher.DataSource = dt;
                        ddlTeacher.DataTextField = "DisplayName";
                        ddlTeacher.DataValueField = "TeacherID";
                        ddlTeacher.DataBind();
                        ddlTeacher.Items.Insert(0, new ListItem("- Select Teacher -", "0"));
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new ApplicationException("Failed to load teachers: " + sqlEx.Message, sqlEx);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPER - Load Assignments DataGridView
        // ══════════════════════════════════════════════════════════════════════
        private void LoadAssignmentsGrid()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    const string sql = @"
                        SELECT  tc.ClassID,
                                c.ClassName,
                                t.TeacherID,
                                t.Name
                               FROM    TeachersClasses tc
                        INNER JOIN Classes  c ON c.ClassID   = tc.ClassID
                        INNER JOIN Teachers t ON t.TeacherID = tc.InchargeID AND ISNULL(t.IsActive,1)=1
                        ORDER BY c.ClassName";

                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvAssignments.DataSource = dt;
                        gvAssignments.DataBind();
                        lblRecordCount.Text = dt.Rows.Count.ToString();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new ApplicationException("Failed to load assignments: " + sqlEx.Message, sqlEx);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SAVE - INSERT new assignment
        // ══════════════════════════════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                int classId = Convert.ToInt32(ddlClass.SelectedValue);
                int teacherId = Convert.ToInt32(ddlTeacher.SelectedValue);

                if (classId == 0 || teacherId == 0)
                {
                    ShowMessage("Please select both a class and a teacher.", isError: true);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // ── Rule 1: Class must not already have an incharge ──────
                    if (ClassAlreadyAssigned(conn, classId))
                    {
                        ShowMessage(
                            " This class already has an assigned incharge teacher. " +
                            "Use Edit to update the existing assignment.",
                            isError: true);
                        return;
                    }

                    // ── Rule 2: Teacher must not already be incharge elsewhere ──
                    if (TeacherAlreadyIncharge(conn, teacherId))
                    {
                        ShowMessage(
                            " This teacher is already the incharge of another class. " +
                            "Each teacher can manage only one class.",
                            isError: true);
                        return;
                    }

                    // ── Insert ────────────────────────────────────────────────
                    const string sql = @"
                        INSERT INTO TeachersClasses (ClassID, InchargeID)
                        VALUES (@ClassID, @InchargeID)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classId);
                        cmd.Parameters.AddWithValue("@InchargeID", teacherId);
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage(" Teacher assigned as class incharge successfully!", isError: false);
                ClearForm();
                LoadAssignmentsGrid();
            }
            catch (SqlException sqlEx)
            {
                ShowMessage($"Database error while saving: {sqlEx.Message}", isError: true);
            }
            catch (Exception ex)
            {
                ShowMessage($"Unexpected error: {ex.Message}", isError: true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  UPDATE - existing assignment
        // ══════════════════════════════════════════════════════════════════════
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                int editingClassId = Convert.ToInt32(hfEditClassID.Value);
                int newTeacherId = Convert.ToInt32(ddlTeacher.SelectedValue);

                if (editingClassId == 0)
                {
                    ShowMessage("No record selected for update.", isError: true);
                    return;
                }
                if (newTeacherId == 0)
                {
                    ShowMessage("Please select a teacher.", isError: true);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    // ── Rule: New teacher must not already be incharge elsewhere
                    //          (allow if it's the same record being updated)  ──
                    const string checkSql = @"
                        SELECT COUNT(1)
                        FROM   TeachersClasses
                        WHERE  InchargeID = @InchargeID
                        AND    ClassID   <> @ClassID";

                    using (SqlCommand chk = new SqlCommand(checkSql, conn))
                    {
                        chk.Parameters.AddWithValue("@InchargeID", newTeacherId);
                        chk.Parameters.AddWithValue("@ClassID", editingClassId);

                        int cnt = (int)chk.ExecuteScalar();
                        if (cnt > 0)
                        {
                            ShowMessage(
                                " The selected teacher is already incharge of another class.",
                                isError: true);
                            return;
                        }
                    }

                    // ── Update ────────────────────────────────────────────────
                    const string sql = @"
                        UPDATE TeachersClasses
                        SET    InchargeID = @InchargeID
                        WHERE  ClassID   = @ClassID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@InchargeID", newTeacherId);
                        cmd.Parameters.AddWithValue("@ClassID", editingClassId);
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage(" Assignment updated successfully!", isError: false);
                ClearForm();
                LoadAssignmentsGrid();
            }
            catch (SqlException sqlEx)
            {
                ShowMessage($"Database error while updating: {sqlEx.Message}", isError: true);
            }
            catch (Exception ex)
            {
                ShowMessage($"Unexpected error: {ex.Message}", isError: true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DELETE - remove assignment
        // ══════════════════════════════════════════════════════════════════════
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                int classId = Convert.ToInt32(hfEditClassID.Value);
                if (classId == 0)
                {
                    ShowMessage("No record selected for deletion.", isError: true);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    const string sql = @"
                        DELETE FROM TeachersClasses WHERE ClassID = @ClassID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classId);
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage(" Assignment removed successfully.", isError: false);
                ClearForm();
                LoadAssignmentsGrid();
            }
            catch (SqlException sqlEx)
            {
                ShowMessage($"Database error while deleting: {sqlEx.Message}", isError: true);
            }
            catch (Exception ex)
            {
                ShowMessage($"Unexpected error: {ex.Message}", isError: true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CLEAR FORM
        // ══════════════════════════════════════════════════════════════════════
        protected void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                ClearForm();
                HideMessage();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error clearing form: {ex.Message}", isError: true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  REFRESH GRID
        // ══════════════════════════════════════════════════════════════════════
        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                LoadAssignmentsGrid();
                ShowMessage("Grid refreshed.", isError: false);
            }
            catch (Exception ex)
            {
                ShowMessage($"Error refreshing: {ex.Message}", isError: true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GRIDVIEW ROW COMMAND - Edit / Delete
        // ══════════════════════════════════════════════════════════════════════
        protected void gvAssignments_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int classId = Convert.ToInt32(e.CommandArgument);

                if (e.CommandName == "EditRow")
                {
                    LoadRowForEditing(classId);
                }
                else if (e.CommandName == "DeleteRow")
                {
                    DeleteAssignmentByClassId(classId);
                    LoadAssignmentsGrid();
                    ShowMessage(" Assignment removed successfully.", isError: false);
                }
            }
            catch (SqlException sqlEx)
            {
                ShowMessage($"Database error: {sqlEx.Message}", isError: true);
            }
            catch (Exception ex)
            {
                ShowMessage($"Error processing action: {ex.Message}", isError: true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPER - Load row into form for editing
        // ══════════════════════════════════════════════════════════════════════
        private void LoadRowForEditing(int classId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    const string sql = @"
                        SELECT ClassID, InchargeID
                        FROM   TeachersClasses
                        WHERE  ClassID = @ClassID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classId);
                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                hfEditClassID.Value = classId.ToString();

                                // Select class in dropdown
                                ListItem classItem = ddlClass.Items.FindByValue(classId.ToString());
                                if (classItem != null)
                                    ddlClass.SelectedValue = classId.ToString();

                                // Select teacher in dropdown
                                string teacherId = dr["InchargeID"].ToString();
                                ListItem tchItem = ddlTeacher.Items.FindByValue(teacherId);
                                if (tchItem != null)
                                    ddlTeacher.SelectedValue = teacherId;
                            }
                        }
                    }
                }

                // Switch form to Edit mode
                btnSave.Visible = false;
                btnUpdate.Visible = true;
                btnDelete.Visible = true;
                ddlClass.Enabled = false;   // class cannot change-only teacher changes

                ShowMessage(" Record loaded for editing. Modify the teacher and click Update.", isError: false);

                // Scroll to top
                Page.ClientScript.RegisterStartupScript(
                    GetType(), "scroll", "window.scrollTo({top:0,behavior:'smooth'});", true);
            }
            catch (SqlException sqlEx)
            {
                throw new ApplicationException("Failed to load record: " + sqlEx.Message, sqlEx);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPER - Delete by ClassID
        // ══════════════════════════════════════════════════════════════════════
        private void DeleteAssignmentByClassId(int classId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    const string sql = "DELETE FROM TeachersClasses WHERE ClassID = @ClassID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                throw new ApplicationException("Failed to delete assignment: " + sqlEx.Message, sqlEx);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPER - Business rule checks
        // ══════════════════════════════════════════════════════════════════════
        private bool ClassAlreadyAssigned(SqlConnection conn, int classId)
        {
            const string sql = "SELECT COUNT(1) FROM TeachersClasses WHERE ClassID = @ClassID";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ClassID", classId);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private bool TeacherAlreadyIncharge(SqlConnection conn, int teacherId)
        {
            const string sql = "SELECT COUNT(1) FROM TeachersClasses WHERE InchargeID = @InchargeID";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@InchargeID", teacherId);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPER - Clear form / Reset to Add mode
        // ══════════════════════════════════════════════════════════════════════
        private void ClearForm()
        {
            hfEditClassID.Value = "0";
            ddlClass.SelectedIndex = 0;
            ddlTeacher.SelectedIndex = 0;
            ddlClass.Enabled = true;
            btnSave.Visible = true;
            btnUpdate.Visible = false;
            btnDelete.Visible = false;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPER - Show / Hide status message
        // ══════════════════════════════════════════════════════════════════════
        private void ShowMessage(string msg, bool isError)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = msg;

            // Swap CSS class via attribute
            var div = pnlMessage.Controls[0] as System.Web.UI.LiteralControl;
            string css = isError ? "status-bar status-error" : "status-bar status-success";

            // Use a simple attribute approach via the panel
            pnlMessage.Attributes["class"] = "";
            // Inject the styled markup directly via Label
            lblMessage.Text = $"<div class=\"{css}\"><span class=\"status-icon\">{(isError ? "" : "")}</span> {msg}</div>";
            // Reset panel wrapper
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add(new LiteralControl(lblMessage.Text));
        }

        private void HideMessage()
        {
            pnlMessage.Visible = false;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PUBLIC HELPER - used inline in GridView markup
        // ══════════════════════════════════════════════════════════════════════
        public string GetInitials(string fullName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullName)) return "?";
                var parts = fullName.Trim().Split(new[] { ' ' },
                    StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
                return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
            }
            catch
            {
                return "?";
            }
        }

        
    }
}
