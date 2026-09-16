using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class WebForm9 : System.Web.UI.Page
    {
        private string ConnectionString
        {
            get
            {
                return ConfigurationManager
                    .ConnectionStrings["SchoolDB"]
                    .ConnectionString;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // FileUpload loses its file on postback. Preserve a newly selected
                // image in ViewState until Update Record is clicked.
                if (IsPostBack && fuImage.HasFile)
                {
                    ProcessedImage staffImage =
                        ImageUploadProcessor.ReadAndOptimize(fuImage, 1200, 1600);
                    ViewState["UploadedImage"] = staffImage.Data;
                }

                if (!IsPostBack)
                {
                    LoadPosts();
                    LoadStaff();
                    LoadGrid();
                    ClearForm(false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Page load error: " + ex.Message,
                    false);
            }
        }

        // =====================================================================
        // STAFF LIST / GRID
        // =====================================================================
        private void LoadGrid(string searchText = "")
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        StaffID,
                        Name,
                        CNICNo,
                        ContactNo,
                        Image
                    FROM NonTeachingStaff
                    WHERE
                        @Search = ''
                        OR Name LIKE '%' + @Search + '%'
                        OR CNICNo LIKE '%' + @Search + '%'
                        OR ContactNo LIKE '%' + @Search + '%'
                    ORDER BY Name;", con))
                {
                    cmd.Parameters.Add(
                        "@Search",
                        SqlDbType.NVarChar,
                        100).Value =
                        searchText == null
                            ? ""
                            : searchText.Trim();

                    DataTable dt = new DataTable();

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    gvStaff.DataSource = dt;
                    gvStaff.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Unable to load staff records: " +
                    ex.Message,
                    false);
            }
        }

        private void LoadStaff()
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        StaffID,
                        Name +
                        CASE
                            WHEN ISNULL(CNICNo, '') = ''
                            THEN ''
                            ELSE ' - ' + CNICNo
                        END AS StaffName
                    FROM NonTeachingStaff
                    ORDER BY Name;", con))
                {
                    con.Open();

                    ddlStaff.DataSource =
                        cmd.ExecuteReader();

                    ddlStaff.DataTextField =
                        "StaffName";

                    ddlStaff.DataValueField =
                        "StaffID";

                    ddlStaff.DataBind();

                    ddlStaff.Items.Insert(
                        0,
                        new ListItem(
                            "-- Select Staff --",
                            ""));
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Unable to load staff list: " +
                    ex.Message,
                    false);
            }
        }

        private void LoadPosts()
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        PostID,
                        Description +
                        ' (BPS-' +
                        CAST(BPS AS varchar(10)) +
                        ')' AS PostName
                    FROM Non_TeachingVacancyPosition
                    ORDER BY Description;", con))
                {
                    con.Open();

                    ddlPost.DataSource =
                        cmd.ExecuteReader();

                    ddlPost.DataTextField =
                        "PostName";

                    ddlPost.DataValueField =
                        "PostID";

                    ddlPost.DataBind();

                    ddlPost.Items.Insert(
                        0,
                        new ListItem(
                            "-- Select Designation --",
                            ""));
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Unable to load designations: " +
                    ex.Message,
                    false);
            }
        }

        // =====================================================================
        // SEARCH / LOAD RECORD
        // =====================================================================
        protected void ddlStaff_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(
                    ddlStaff.SelectedValue))
                {
                    int staffId;

                    if (int.TryParse(
                        ddlStaff.SelectedValue,
                        out staffId))
                    {
                        LoadStaffRecord(staffId);
                    }
                }
                else
                {
                    ClearForm(false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Unable to load selected staff record: " +
                    ex.Message,
                    false);
            }
        }

        protected void btnSearch_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                string search =
                    txtSearchStaff.Text.Trim();

                // If a staff member is already selected in the dropdown,
                // Search acts as an explicit "load selected record" button.
                if (!string.IsNullOrWhiteSpace(
                    ddlStaff.SelectedValue))
                {
                    int selectedId;

                    if (int.TryParse(
                        ddlStaff.SelectedValue,
                        out selectedId))
                    {
                        LoadStaffRecord(selectedId);
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(search))
                {
                    ShowMessage(
                        "Select a staff member or enter a Name/CNIC to search.",
                        false);
                    return;
                }

                using (SqlConnection con =
                    new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 1 StaffID
                    FROM NonTeachingStaff
                    WHERE
                        Name LIKE '%' + @Search + '%'
                        OR CNICNo LIKE '%' + @Search + '%'
                        OR ContactNo LIKE '%' + @Search + '%'
                    ORDER BY
                        CASE WHEN Name = @Search THEN 0 ELSE 1 END,
                        Name;", con))
                {
                    cmd.Parameters.Add(
                        "@Search",
                        SqlDbType.NVarChar,
                        100).Value = search;

                    con.Open();

                    object result =
                        cmd.ExecuteScalar();

                    if (result == null ||
                        result == DBNull.Value)
                    {
                        LoadGrid(search);

                        ShowMessage(
                            "No matching staff record found.",
                            false);
                        return;
                    }

                    int staffId =
                        Convert.ToInt32(result);

                    SelectStaffInDropDown(staffId);

                    LoadStaffRecord(staffId);

                    LoadGrid(search);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Search failed: " +
                    ex.Message,
                    false);
            }
        }

        private void SelectStaffInDropDown(
            int staffId)
        {
            ListItem item =
                ddlStaff.Items.FindByValue(
                    staffId.ToString());

            if (item != null)
            {
                ddlStaff.ClearSelection();
                item.Selected = true;
            }
        }

        private void LoadStaffRecord(
            int staffId)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        StaffID,
                        Name,
                        FatherName,
                        Gender,
                        DOB,
                        CNICNo,
                        ContactNo,
                        EmailID,
                        Address,
                        MaritalStatus,
                        Qualification,
                        PersonalNo,
                        DateOfJoining,
                        DateOfJoiningInThisSchool,
                        DateOfFirstAppointment,
                        DateOfContractAppointment,
                        RegularAppointmentDate,
                        DateOfAwardCurrentGrade,
                        PostID,
                        Image
                    FROM NonTeachingStaff
                    WHERE StaffID = @StaffID;", con))
                {
                    cmd.Parameters.Add(
                        "@StaffID",
                        SqlDbType.Int).Value = staffId;

                    con.Open();

                    using (SqlDataReader dr =
                        cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            ShowMessage(
                                "Staff record was not found.",
                                false);
                            return;
                        }

                        hfSelectedStaffID.Value =
                            staffId.ToString();

                        lblSelectedStaff.Text =
                            dr["Name"].ToString() +
                            " (Staff ID: " +
                            staffId +
                            ")";

                        txtName.Text =
                            GetString(dr, "Name");

                        txtFatherName.Text =
                            GetString(dr, "FatherName");

                        SetDropDownValue(
                            ddlGender,
                            GetString(dr, "Gender"));

                        txtDOB.Text =
                            GetDateText(dr, "DOB");

                        txtCNIC.Text =
                            GetString(dr, "CNICNo");

                        txtContact.Text =
                            GetString(dr, "ContactNo");

                        txtEmail.Text =
                            GetString(dr, "EmailID");

                        txtAddress.Text =
                            GetString(dr, "Address");

                        SetDropDownValue(
                            ddlMaritalStatus,
                            GetString(dr, "MaritalStatus"));

                        txtQualification.Text =
                            GetString(dr, "Qualification");

                        txtPersonalNo.Text =
                            GetString(dr, "PersonalNo");

                        txtDateJoining.Text =
                            GetDateText(
                                dr,
                                "DateOfJoining");

                        txtJoiningSchool.Text =
                            GetDateText(
                                dr,
                                "DateOfJoiningInThisSchool");

                        txtFirstAppointment.Text =
                            GetDateText(
                                dr,
                                "DateOfFirstAppointment");

                        txtContractAppointment.Text =
                            GetDateText(
                                dr,
                                "DateOfContractAppointment");

                        txtRegularAppointmentDate.Text =
                            GetDateText(
                                dr,
                                "RegularAppointmentDate");

                        txtDateOfAwardCurrentGrade.Text =
                            GetDateText(
                                dr,
                                "DateOfAwardCurrentGrade");

                        string postId =
                            GetString(dr, "PostID");

                        SetDropDownValue(
                            ddlPost,
                            postId);

                        // Load stored staff image directly from the database.
                        if (dr["Image"] != DBNull.Value)
                        {
                            byte[] imageBytes =
                                dr["Image"] as byte[];

                            if (imageBytes != null &&
                                imageBytes.Length > 0)
                            {
                                ViewState["CurrentImage"] =
                                    imageBytes;

                                imgStaff.ImageUrl =
                                    "data:image/jpeg;base64," +
                                    Convert.ToBase64String(
                                        imageBytes);
                            }
                            else
                            {
                                ViewState["CurrentImage"] =
                                    null;

                                imgStaff.ImageUrl =
                                    "~/images/noimage.png";
                            }
                        }
                        else
                        {
                            ViewState["CurrentImage"] =
                                null;

                            imgStaff.ImageUrl =
                                "~/images/noimage.png";
                        }

                        // A previously selected replacement image must not
                        // carry over to another staff member.
                        ViewState["UploadedImage"] =
                            null;
                    }
                }

                ShowMessage(
                    "Staff record loaded successfully.",
                    true);
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Unable to load staff record: " +
                    ex.Message,
                    false);
            }
        }

        // =====================================================================
        // UPDATE RECORD
        // =====================================================================
        protected void btnUpdate_Click(
            object sender,
            EventArgs e)
        {
            if (!Page.IsValid)
                return;

            int staffId;

            if (!int.TryParse(
                hfSelectedStaffID.Value,
                out staffId) ||
                staffId <= 0)
            {
                ShowMessage(
                    "Search and load a staff record before updating.",
                    false);
                return;
            }

            int newPostId;

            if (!int.TryParse(
                ddlPost.SelectedValue,
                out newPostId))
            {
                ShowMessage(
                    "Please select a valid designation.",
                    false);
                return;
            }

            try
            {
                using (SqlConnection con =
                    new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlTransaction tran =
                        con.BeginTransaction())
                    {
                        try
                        {
                            int oldPostId = 0;

                            using (SqlCommand oldCmd =
                                new SqlCommand(@"
                                    SELECT ISNULL(PostID, 0)
                                    FROM NonTeachingStaff
                                    WHERE StaffID = @StaffID;",
                                    con,
                                    tran))
                            {
                                oldCmd.Parameters.Add(
                                    "@StaffID",
                                    SqlDbType.Int)
                                    .Value = staffId;

                                object oldValue =
                                    oldCmd.ExecuteScalar();

                                if (oldValue == null ||
                                    oldValue == DBNull.Value)
                                {
                                    throw new Exception(
                                        "Selected staff record no longer exists.");
                                }

                                oldPostId =
                                    Convert.ToInt32(oldValue);
                            }

                            byte[] newImage =
                                ViewState["UploadedImage"]
                                as byte[];

                            bool hasNewImage =
                                newImage != null &&
                                newImage.Length > 0;

                            string sql = @"
                                UPDATE NonTeachingStaff
                                SET
                                    Name = @Name,
                                    FatherName = @FatherName,
                                    Gender = @Gender,
                                    DOB = @DOB,
                                    CNICNo = @CNICNo,
                                    ContactNo = @ContactNo,
                                    EmailID = @EmailID,
                                    Address = @Address,
                                    MaritalStatus = @MaritalStatus,
                                    Qualification = @Qualification,
                                    PersonalNo = @PersonalNo,
                                    DateOfJoining = @DateOfJoining,
                                    DateOfJoiningInThisSchool = @DateOfJoiningInThisSchool,
                                    DateOfFirstAppointment = @DateOfFirstAppointment,
                                    DateOfContractAppointment = @DateOfContractAppointment,
                                    RegularAppointmentDate = @RegularAppointmentDate,
                                    DateOfAwardCurrentGrade = @DateOfAwardCurrentGrade,
                                    PostID = @PostID" +
                                    (hasNewImage
                                        ? ", Image = @Image"
                                        : "") + @"
                                WHERE StaffID = @StaffID;";

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    sql,
                                    con,
                                    tran))
                            {
                                cmd.Parameters.Add(
                                    "@Name",
                                    SqlDbType.NVarChar,
                                    150).Value =
                                    txtName.Text.Trim();

                                cmd.Parameters.Add(
                                    "@FatherName",
                                    SqlDbType.NVarChar,
                                    150).Value =
                                    DbString(
                                        txtFatherName.Text);

                                cmd.Parameters.Add(
                                    "@Gender",
                                    SqlDbType.NVarChar,
                                    20).Value =
                                    DbString(
                                        ddlGender.SelectedValue);

                                cmd.Parameters.Add(
                                    "@DOB",
                                    SqlDbType.Date).Value =
                                    DbDate(txtDOB.Text);

                                cmd.Parameters.Add(
                                    "@CNICNo",
                                    SqlDbType.NVarChar,
                                    30).Value =
                                    DbString(txtCNIC.Text);

                                cmd.Parameters.Add(
                                    "@ContactNo",
                                    SqlDbType.NVarChar,
                                    30).Value =
                                    DbString(txtContact.Text);

                                cmd.Parameters.Add(
                                    "@EmailID",
                                    SqlDbType.NVarChar,
                                    150).Value =
                                    DbString(txtEmail.Text);

                                cmd.Parameters.Add(
                                    "@Address",
                                    SqlDbType.NVarChar,
                                    500).Value =
                                    DbString(txtAddress.Text);

                                cmd.Parameters.Add(
                                    "@MaritalStatus",
                                    SqlDbType.NVarChar,
                                    30).Value =
                                    DbString(
                                        ddlMaritalStatus.SelectedValue);

                                cmd.Parameters.Add(
                                    "@Qualification",
                                    SqlDbType.NVarChar,
                                    250).Value =
                                    DbString(
                                        txtQualification.Text);

                                cmd.Parameters.Add(
                                    "@PersonalNo",
                                    SqlDbType.NVarChar,
                                    50).Value =
                                    DbString(
                                        txtPersonalNo.Text);

                                cmd.Parameters.Add(
                                    "@DateOfJoining",
                                    SqlDbType.Date).Value =
                                    DbDate(
                                        txtDateJoining.Text);

                                cmd.Parameters.Add(
                                    "@DateOfJoiningInThisSchool",
                                    SqlDbType.Date).Value =
                                    DbDate(
                                        txtJoiningSchool.Text);

                                cmd.Parameters.Add(
                                    "@DateOfFirstAppointment",
                                    SqlDbType.Date).Value =
                                    DbDate(
                                        txtFirstAppointment.Text);

                                cmd.Parameters.Add(
                                    "@DateOfContractAppointment",
                                    SqlDbType.Date).Value =
                                    DbDate(
                                        txtContractAppointment.Text);

                                cmd.Parameters.Add(
                                    "@RegularAppointmentDate",
                                    SqlDbType.Date).Value =
                                    DbDate(
                                        txtRegularAppointmentDate.Text);

                                cmd.Parameters.Add(
                                    "@DateOfAwardCurrentGrade",
                                    SqlDbType.Date).Value =
                                    DbDate(
                                        txtDateOfAwardCurrentGrade.Text);

                                cmd.Parameters.Add(
                                    "@PostID",
                                    SqlDbType.Int).Value =
                                    newPostId;

                                cmd.Parameters.Add(
                                    "@StaffID",
                                    SqlDbType.Int).Value =
                                    staffId;

                                if (hasNewImage)
                                {
                                    cmd.Parameters.Add(
                                        "@Image",
                                        SqlDbType.VarBinary,
                                        -1).Value = newImage;
                                }

                                int rows =
                                    cmd.ExecuteNonQuery();

                                if (rows == 0)
                                {
                                    throw new Exception(
                                        "No record was updated.");
                                }
                            }

                            // Keep vacancy Working counts synchronized when
                            // the staff designation changes.
                            if (oldPostId != newPostId)
                            {
                                if (oldPostId > 0)
                                {
                                    using (SqlCommand decrease =
                                        new SqlCommand(@"
                                            UPDATE Non_TeachingVacancyPosition
                                            SET Working =
                                                CASE
                                                    WHEN ISNULL(Working,0) > 0
                                                    THEN Working - 1
                                                    ELSE 0
                                                END
                                            WHERE PostID = @PostID;",
                                            con,
                                            tran))
                                    {
                                        decrease.Parameters.Add(
                                            "@PostID",
                                            SqlDbType.Int)
                                            .Value = oldPostId;

                                        decrease.ExecuteNonQuery();
                                    }
                                }

                                using (SqlCommand increase =
                                    new SqlCommand(@"
                                        UPDATE Non_TeachingVacancyPosition
                                        SET Working =
                                            ISNULL(Working,0) + 1
                                        WHERE PostID = @PostID;",
                                        con,
                                        tran))
                                {
                                    increase.Parameters.Add(
                                        "@PostID",
                                        SqlDbType.Int)
                                        .Value = newPostId;

                                    increase.ExecuteNonQuery();
                                }
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

                ViewState["UploadedImage"] =
                    null;

                // Refresh dropdown/grid and reload saved record so the form
                // displays exactly what is now in the database.
                LoadStaff();
                SelectStaffInDropDown(staffId);
                LoadGrid();
                LoadStaffRecord(staffId);

                ShowMessage(
                    "Staff record updated and saved successfully.",
                    true);
            }
            catch (SqlException ex)
            {
                ShowMessage(
                    "Database update failed: " +
                    ex.Message,
                    false);
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Update failed: " +
                    ex.Message,
                    false);
            }
        }

        // =====================================================================
        // GRID IMAGE
        // =====================================================================
        protected void gvStaff_RowDataBound(
            object sender,
            GridViewRowEventArgs e)
        {
            if (e.Row.RowType !=
                DataControlRowType.DataRow)
                return;

            Image img =
                e.Row.FindControl(
                    "imgGridStaff") as Image;

            if (img == null)
                return;

            DataRowView row =
                e.Row.DataItem as DataRowView;

            if (row != null &&
                row["Image"] != DBNull.Value)
            {
                byte[] bytes =
                    row["Image"] as byte[];

                if (bytes != null &&
                    bytes.Length > 0)
                {
                    img.ImageUrl =
                        "data:image/jpeg;base64," +
                        Convert.ToBase64String(bytes);
                    return;
                }
            }

            img.ImageUrl =
                "~/images/noimage.png";
        }

        public string GetImageUrl(
            string staffId)
        {
            if (!string.IsNullOrEmpty(staffId) &&
                staffId != "0")
            {
                return ResolveUrl(
                    "~/ShowImage.ashx?id=" +
                    staffId);
            }

            return ResolveUrl(
                "~/images/noimage.png");
        }

        protected void gvStaff_PageIndexChanging(
            object sender,
            GridViewPageEventArgs e)
        {
            gvStaff.PageIndex =
                e.NewPageIndex;

            LoadGrid(
                txtSearchStaff.Text.Trim());
        }

        // =====================================================================
        // CLEAR
        // =====================================================================
        protected void btnClear_Click(
            object sender,
            EventArgs e)
        {
            ClearForm(true);
            LoadGrid();
        }

        private void ClearForm(
            bool clearSearch)
        {
            hfSelectedStaffID.Value = "";

            lblSelectedStaff.Text =
                "No staff record selected.";

            txtName.Text = "";
            txtFatherName.Text = "";
            txtDOB.Text = "";
            txtCNIC.Text = "";
            txtContact.Text = "";
            txtEmail.Text = "";
            txtAddress.Text = "";
            txtQualification.Text = "";
            txtPersonalNo.Text = "";

            txtDateJoining.Text = "";
            txtJoiningSchool.Text = "";
            txtFirstAppointment.Text = "";
            txtContractAppointment.Text = "";
            txtRegularAppointmentDate.Text = "";
            txtDateOfAwardCurrentGrade.Text = "";

            if (ddlGender.Items.Count > 0)
                ddlGender.SelectedIndex = 0;

            if (ddlMaritalStatus.Items.Count > 0)
                ddlMaritalStatus.SelectedIndex = 0;

            if (ddlPost.Items.Count > 0)
                ddlPost.SelectedIndex = 0;

            if (ddlStaff.Items.Count > 0)
                ddlStaff.SelectedIndex = 0;

            imgStaff.ImageUrl =
                "~/images/noimage.png";

            ViewState["UploadedImage"] =
                null;

            ViewState["CurrentImage"] =
                null;

            if (clearSearch)
                txtSearchStaff.Text = "";
        }

        // =====================================================================
        // HELPERS
        // =====================================================================
        private static string GetString(
            SqlDataReader dr,
            string column)
        {
            return dr[column] == DBNull.Value
                ? ""
                : dr[column].ToString();
        }

        private static string GetDateText(
            SqlDataReader dr,
            string column)
        {
            if (dr[column] == DBNull.Value)
                return "";

            return Convert.ToDateTime(
                dr[column])
                .ToString("yyyy-MM-dd");
        }

        private static object DbString(
            string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? (object)DBNull.Value
                : value.Trim();
        }

        private static object DbDate(
            string value)
        {
            DateTime date;

            return DateTime.TryParse(
                value,
                out date)
                ? (object)date.Date
                : DBNull.Value;
        }

        private static void SetDropDownValue(
            DropDownList ddl,
            string value)
        {
            if (ddl == null)
                return;

            ListItem item =
                ddl.Items.FindByValue(
                    value ?? "");

            if (item != null)
            {
                ddl.ClearSelection();
                item.Selected = true;
            }
            else if (ddl.Items.Count > 0)
            {
                ddl.SelectedIndex = 0;
            }
        }

        private void ShowMessage(
            string message,
            bool success)
        {
            string safe =
                (message ?? "")
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");

            string script =
                "alert('" + safe + "');";

            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                Guid.NewGuid().ToString("N"),
                script,
                true);
        }
    }
}
