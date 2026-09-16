using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace DigitalSchoolManager
{
    public partial class WebForm11 : System.Web.UI.Page
    {
        string cs = ConfigurationManager
                     .ConnectionStrings["SchoolDB"].ConnectionString;

       

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    BindGrid(string.Empty);
                    LoadStats();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Page load error: " + ex.Message, MessageType.Error);
            }

        }
        // ════════════════════════════════════════════════════════════════
        //  DATA BINDING
        // ════════════════════════════════════════════════════════════════

        /// <summary>Loads the GridView, optionally filtered by a search term.</summary>
        private void BindGrid(string searchTerm)
        {
            try
            {
                SqlConnection con = new SqlConnection(cs);
                {
                    string sql = @"
                        SELECT ClassID, ClassName
                        FROM   Classes
                        WHERE  (@Search = '' OR ClassName LIKE '%' + @Search + '%')
                        ORDER  BY ClassName ASC";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Search",
                            string.IsNullOrWhiteSpace(searchTerm) ? "" : searchTerm.Trim());

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        gvClasses.DataSource = dt;
                        gvClasses.DataBind();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                ShowMessage("Database error while loading classes: " + sqlEx.Message, MessageType.Error);
            }
            catch (Exception ex)
            {
                ShowMessage("Unexpected error while loading classes: " + ex.Message, MessageType.Error);
            }
        }

        /// <summary>Populates the stats strip (total count, last-added name, today's date).</summary>
        private void LoadStats()
        {
            try
            {
                SqlConnection con = new SqlConnection(cs);
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) AS Total, MAX(ClassName) AS LastName FROM Classes", con);
                {
                    con.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            lblTotalClasses.Text = rdr["Total"].ToString();
                            lblActiveToday.Text = rdr["LastName"] == DBNull.Value
                                                   ? "-"
                                                   : rdr["LastName"].ToString();
                        }
                    }
                }
                lblLastUpdated.Text = DateTime.Now.ToString("HH:mm");
            }
            catch (SqlException sqlEx)
            {
                // Non-critical - silently log; don't interrupt the page
                System.Diagnostics.Debug.WriteLine("Stats SQL error: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Stats error: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  ADD / UPDATE (FORM CARD)
        // ════════════════════════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                string className = txtClassName.Text.Trim();
                int classId = Convert.ToInt32(hfClassID.Value);
                bool isEdit = classId > 0;

                SqlConnection con = new SqlConnection(cs);
                {
                    con.Open();

                    // Duplicate check
                    using (SqlCommand chk = new SqlCommand(
                        "SELECT COUNT(1) FROM Classes WHERE ClassName = @Name AND ClassID <> @ID", con))
                    {
                        chk.Parameters.AddWithValue("@Name", className);
                        chk.Parameters.AddWithValue("@ID", classId);
                        int exists = (int)chk.ExecuteScalar();
                        if (exists > 0)
                        {
                            ShowMessage($"A class named \"{className}\" already exists. Please use a different name.",
                                        MessageType.Error);
                            return;
                        }
                    }

                    string sql = isEdit
                        ? "UPDATE Classes SET ClassName = @Name WHERE ClassID = @ID"
                        : "INSERT INTO Classes (ClassName) VALUES (@Name)";

                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", className);
                        if (isEdit) cmd.Parameters.AddWithValue("@ID", classId);
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage(
                    isEdit
                        ? $"Class \"{className}\" updated successfully."
                        : $"Class \"{className}\" added successfully.",
                    MessageType.Success);

                ClearForm();
                BindGrid(txtSearch.Text.Trim());
                LoadStats();
            }
            catch (SqlException sqlEx)
            {
                ShowMessage("Database error while saving: " + sqlEx.Message, MessageType.Error);
            }
            catch (FormatException)
            {
                ShowMessage("Invalid data format. Please check your input.", MessageType.Error);
            }
            catch (Exception ex)
            {
                ShowMessage("Unexpected error while saving: " + ex.Message, MessageType.Error);
            }

        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                ClearForm();
                pnlMessage.Visible = false;
            }
            catch (Exception ex)
            {
                ShowMessage("Error clearing form: " + ex.Message, MessageType.Error);
            }
        }
        protected void gvClasses_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                gvClasses.EditIndex = e.NewEditIndex;
                BindGrid(txtSearch.Text.Trim());
            }
            catch (Exception ex)
            {
                ShowMessage("Error entering edit mode: " + ex.Message, MessageType.Error);
            }
        }

        protected void gvClasses_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            try
            {
                gvClasses.EditIndex = -1;
                BindGrid(txtSearch.Text.Trim());
            }
            catch (Exception ex)
            {
                ShowMessage("Error cancelling edit: " + ex.Message, MessageType.Error);
            }
        }

        protected void gvClasses_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                // Read the in-grid textbox
                GridViewRow row = gvClasses.Rows[e.RowIndex];
                TextBox tbName = row.FindControl("txtEditName") as TextBox;
                int classId = Convert.ToInt32(gvClasses.DataKeys[e.RowIndex].Value);

                if (tbName == null || string.IsNullOrWhiteSpace(tbName.Text))
                {
                    ShowMessage("Class Name cannot be empty.", MessageType.Error);
                    return;
                }

                string newName = tbName.Text.Trim();

                // Basic length guard (mirrors regex on UI)
                if (newName.Length < 2 || newName.Length > 100)
                {
                    ShowMessage("Class Name must be between 2 and 100 characters.", MessageType.Error);
                    return;
                }

                SqlConnection con = new SqlConnection(cs);
                {
                    con.Open();

                    // Duplicate check
                    using (SqlCommand chk = new SqlCommand(
                        "SELECT COUNT(1) FROM Classes WHERE ClassName = @Name AND ClassID <> @ID", con))
                    {
                        chk.Parameters.AddWithValue("@Name", newName);
                        chk.Parameters.AddWithValue("@ID", classId);
                        if ((int)chk.ExecuteScalar() > 0)
                        {
                            ShowMessage($"A class named \"{newName}\" already exists.", MessageType.Error);
                            return;
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE Classes SET ClassName = @Name WHERE ClassID = @ID", con))
                    {
                        cmd.Parameters.AddWithValue("@Name", newName);
                        cmd.Parameters.AddWithValue("@ID", classId);
                        cmd.ExecuteNonQuery();
                    }
                }

                gvClasses.EditIndex = -1;
                BindGrid(txtSearch.Text.Trim());
                LoadStats();
                ShowMessage($"Class updated to \"{newName}\" successfully.", MessageType.Success);
            }
            catch (SqlException sqlEx)
            {
                ShowMessage("Database error while updating: " + sqlEx.Message, MessageType.Error);
            }
            catch (FormatException)
            {
                ShowMessage("Invalid class ID. Please refresh the page.", MessageType.Error);
            }
            catch (Exception ex)
            {
                ShowMessage("Unexpected error while updating: " + ex.Message, MessageType.Error);
            }
        }

        protected void gvClasses_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int classId = Convert.ToInt32(gvClasses.DataKeys[e.RowIndex].Value);

                SqlConnection con = new SqlConnection(cs);
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Classes WHERE ClassID = @ID", con))
                {
                    cmd.Parameters.AddWithValue("@ID", classId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                gvClasses.EditIndex = -1;
                BindGrid(txtSearch.Text.Trim());
                LoadStats();
                ShowMessage("Class deleted successfully.", MessageType.Success);
            }
            catch (SqlException sqlEx)
            {
                // FK violation - a common case
                if (sqlEx.Number == 547)
                    ShowMessage("Cannot delete: this class is referenced by other records.", MessageType.Error);
                else
                    ShowMessage("Database error while deleting: " + sqlEx.Message, MessageType.Error);
            }
            catch (FormatException)
            {
                ShowMessage("Invalid class ID. Please refresh the page.", MessageType.Error);
            }
            catch (Exception ex)
            {
                ShowMessage("Unexpected error while deleting: " + ex.Message, MessageType.Error);
            }
        }

        
        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                txtSearch.Text = string.Empty;
                gvClasses.EditIndex = -1;
                BindGrid(string.Empty);
                LoadStats();
                pnlMessage.Visible = false;
            }
            catch (Exception ex)
            {
                ShowMessage("Refresh error: " + ex.Message, MessageType.Error);
            }

        }
        // ════════════════════════════════════════════════════════════════
        //  SEARCH
        // ════════════════════════════════════════════════════════════════

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                gvClasses.EditIndex = -1;
                BindGrid(txtSearch.Text.Trim());
            }
            catch (Exception ex)
            {
                ShowMessage("Search error: " + ex.Message, MessageType.Error);
            }

        }
        // ════════════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════════════

        private void ClearForm()
        {
            txtClassName.Text = string.Empty;
            hfClassID.Value = "0";
            lblFormTitle.Text = "Add New Class";
            btnSave.Text = "Save Class";
        }

        private enum MessageType { Success, Error, Info }

        private void ShowMessage(string message, MessageType type)
        {
            lblMessage.Text = Server.HtmlEncode(message);
            pnlMessage.Visible = true;

            // Swap the CSS class on the inner div via ScriptManager
            string cssClass = type switch
            {
                MessageType.Error => "cm-alert cm-alert-error",
                MessageType.Info => "cm-alert cm-alert-info",
                _ => "cm-alert cm-alert-success"
            };

            // Inject script to update banner class after render
            ScriptManager.RegisterStartupScript(this, GetType(), "bannerClass",
                $"(function(){{var b=document.getElementById('statusBanner');if(b)b.className='{cssClass}';}})();",
                true);
        }
    }

}