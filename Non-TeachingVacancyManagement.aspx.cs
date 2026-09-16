using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace DigitalSchoolManager
{
    public partial class WebForm8 : System.Web.UI.Page
    {
        private string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadVacancies();
                ClearForm();
            }
        }

        private void LoadVacancies()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                using (SqlDataAdapter da = new SqlDataAdapter(@"
                    SELECT
                        postid,
                        Description,
                        BPS,
                        Sactioned,
                        Working,
                        Vacant,
                        Comments
                    FROM Non_TeachingVacancyPosition
                    ORDER BY Description, BPS;", con))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvVacancies.DataSource = dt;
                    gvVacancies.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Unable to load vacancy positions. " + ex.Message);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            int bps;
            int sanctioned;

            if (!int.TryParse(txtBPS.Text.Trim(), out bps) ||
                !int.TryParse(txtSanctioned.Text.Trim(), out sanctioned))
            {
                ShowMessage("Please enter valid numeric values for BPS and Sanctioned Posts.");
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(hfPostId.Value))
                {
                    InsertVacancy(bps, sanctioned);
                    ShowMessage("Vacancy position saved successfully.");
                }
                else
                {
                    int postId;
                    if (!int.TryParse(hfPostId.Value, out postId))
                    {
                        ShowMessage("Invalid vacancy position selected.");
                        ClearForm();
                        return;
                    }

                    if (!UpdateVacancy(postId, bps, sanctioned))
                        return;
                }

                LoadVacancies();
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowMessage("Unable to save vacancy position. " + ex.Message);
            }
        }

        private void InsertVacancy(int bps, int sanctioned)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Non_TeachingVacancyPosition
                (
                    Description,
                    BPS,
                    Sactioned,
                    Working,
                    Comments
                )
                VALUES
                (
                    @Description,
                    @BPS,
                    @Sactioned,
                    0,
                    @Comments
                );", con))
            {
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 150).Value =
                    txtDescription.Text.Trim();

                cmd.Parameters.Add("@BPS", SqlDbType.Int).Value = bps;
                cmd.Parameters.Add("@Sactioned", SqlDbType.Int).Value = sanctioned;

                cmd.Parameters.Add("@Comments", SqlDbType.NVarChar, 250).Value =
                    string.IsNullOrWhiteSpace(txtComments.Text)
                        ? (object)DBNull.Value
                        : txtComments.Text.Trim();

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private bool UpdateVacancy(int postId, int bps, int sanctioned)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                int working;

                using (SqlCommand checkCmd = new SqlCommand(@"
                    SELECT ISNULL(Working, 0)
                    FROM Non_TeachingVacancyPosition
                    WHERE postid = @postid;", con))
                {
                    checkCmd.Parameters.Add("@postid", SqlDbType.Int).Value = postId;
                    object result = checkCmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        ShowMessage("The selected vacancy position no longer exists.");
                        return false;
                    }

                    working = Convert.ToInt32(result);
                }

                if (sanctioned < working)
                {
                    ShowMessage("Sanctioned Posts cannot be less than Working Posts (" + working + ").");
                    return false;
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Non_TeachingVacancyPosition
                    SET
                        Description = @Description,
                        BPS = @BPS,
                        Sactioned = @Sactioned,
                        Comments = @Comments
                    WHERE postid = @postid;", con))
                {
                    cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 150).Value =
                        txtDescription.Text.Trim();

                    cmd.Parameters.Add("@BPS", SqlDbType.Int).Value = bps;
                    cmd.Parameters.Add("@Sactioned", SqlDbType.Int).Value = sanctioned;

                    cmd.Parameters.Add("@Comments", SqlDbType.NVarChar, 250).Value =
                        string.IsNullOrWhiteSpace(txtComments.Text)
                            ? (object)DBNull.Value
                            : txtComments.Text.Trim();

                    cmd.Parameters.Add("@postid", SqlDbType.Int).Value = postId;

                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        ShowMessage("Vacancy position updated successfully.");
                        return true;
                    }

                    ShowMessage("No vacancy position was updated.");
                    return false;
                }
            }
        }

        protected void gvVacancies_RowCommand(
            object sender,
            System.Web.UI.WebControls.GridViewCommandEventArgs e)
        {
            int postId;

            if (!int.TryParse(Convert.ToString(e.CommandArgument), out postId))
            {
                ShowMessage("Invalid vacancy position selected.");
                return;
            }

            if (e.CommandName == "EditVacancy")
            {
                LoadVacancyForEdit(postId);
            }
            else if (e.CommandName == "DeleteVacancy")
            {
                DeleteVacancy(postId);
            }
        }

        private void LoadVacancyForEdit(int postId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        postid,
                        Description,
                        BPS,
                        Sactioned,
                        Comments
                    FROM Non_TeachingVacancyPosition
                    WHERE postid = @postid;", con))
                {
                    cmd.Parameters.Add("@postid", SqlDbType.Int).Value = postId;

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            ShowMessage("The selected vacancy position was not found.");
                            return;
                        }

                        hfPostId.Value = Convert.ToString(dr["postid"]);
                        txtDescription.Text = Convert.ToString(dr["Description"]);
                        txtBPS.Text = Convert.ToString(dr["BPS"]);
                        txtSanctioned.Text = Convert.ToString(dr["Sactioned"]);
                        txtComments.Text = dr["Comments"] == DBNull.Value
                            ? ""
                            : Convert.ToString(dr["Comments"]);

                        btnSave.Text = "Update Position";
                        lblFormTitle.Text = "Edit Vacancy Position";
                        lblEditMode.Visible = true;
                    }
                }

                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "scrollEdit",
                    "window.scrollTo({ top: 0, behavior: 'smooth' });",
                    true);
            }
            catch (Exception ex)
            {
                ShowMessage("Unable to load the vacancy position for editing. " + ex.Message);
            }
        }

        private void DeleteVacancy(int postId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    int working;

                    using (SqlCommand checkCmd = new SqlCommand(@"
                        SELECT ISNULL(Working, 0)
                        FROM Non_TeachingVacancyPosition
                        WHERE postid = @postid;", con))
                    {
                        checkCmd.Parameters.Add("@postid", SqlDbType.Int).Value = postId;
                        object result = checkCmd.ExecuteScalar();

                        if (result == null || result == DBNull.Value)
                        {
                            ShowMessage("The selected vacancy position no longer exists.");
                            LoadVacancies();
                            return;
                        }

                        working = Convert.ToInt32(result);
                    }

                    if (working > 0)
                    {
                        ShowMessage(
                            "This vacancy position cannot be deleted because " +
                            working +
                            " staff member(s) are currently working against it. " +
                            "Reassign or remove those staff records first.");
                        return;
                    }

                    using (SqlCommand cmd = new SqlCommand(@"
                        DELETE FROM Non_TeachingVacancyPosition
                        WHERE postid = @postid;", con))
                    {
                        cmd.Parameters.Add("@postid", SqlDbType.Int).Value = postId;

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            ShowMessage("Vacancy position deleted successfully.");
                        else
                            ShowMessage("No vacancy position was deleted.");
                    }
                }

                LoadVacancies();

                if (hfPostId.Value == postId.ToString())
                    ClearForm();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547)
                {
                    ShowMessage(
                        "This vacancy position is linked with another record and cannot be deleted. " +
                        "Remove or reassign the related record first.");
                }
                else
                {
                    ShowMessage("Database error while deleting vacancy position. " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Unable to delete vacancy position. " + ex.Message);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfPostId.Value = "";

            txtDescription.Text = "";
            txtBPS.Text = "";
            txtSanctioned.Text = "";
            txtComments.Text = "";

            btnSave.Text = "Save Position";
            lblFormTitle.Text = "Add Vacancy Position";
            lblEditMode.Visible = false;

            txtDescription.Focus();
        }

        private void ShowMessage(string message)
        {
            string safeMessage = (message ?? "")
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                Guid.NewGuid().ToString("N"),
                "alert('" + safeMessage + "');",
                true);
        }
    }
}
