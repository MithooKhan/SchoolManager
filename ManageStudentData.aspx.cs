using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;



namespace DigitalSchoolManager
{
    public partial class WebForm16 : System.Web.UI.Page
    {

        private readonly string _conn =
         System.Configuration.ConfigurationManager
               .ConnectionStrings["SchoolDB"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                StudentImageStorage.EnsureSchema(this, _conn);
                StudentClassEnrollmentService.EnsureSchema();

                if (!IsPostBack)
                {
                    LoadClassDropdowns();
                    LoadStudentGrid();
                    LoadStats();
                }
            }
            catch (Exception ex)
            {
                ShowError("Page load failed: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        #region ── Data Loaders ──

        /// <summary>Fill all class dropdowns (search, edit form, grid filter).</summary>
        private void LoadClassDropdowns()
        {
            try
            {
                DataTable dt = GetClassTable();

                // Search panel dropdown
                ddlSearchClass.DataSource = dt;
                ddlSearchClass.DataTextField = "ClassName";
                ddlSearchClass.DataValueField = "ClassID";
                ddlSearchClass.DataBind();
                ddlSearchClass.Items.Insert(0, new ListItem("All Classes", "0"));

                // Edit form dropdown
                DataTable dtEdit = dt.Copy();
                ddlClass.DataSource = dtEdit;
                ddlClass.DataTextField = "ClassName";
                ddlClass.DataValueField = "ClassID";
                ddlClass.DataBind();
                ddlClass.Items.Insert(0, new ListItem("Select Class", "0"));

                // Grid filter dropdown
                DataTable dtGrid = dt.Copy();
                ddlGridClass.DataSource = dtGrid;
                ddlGridClass.DataTextField = "ClassName";
                ddlGridClass.DataValueField = "ClassID";
                ddlGridClass.DataBind();
                ddlGridClass.Items.Insert(0, new ListItem("All Classes", "0"));
            }
            catch (Exception ex)
            {
                ShowError("Could not load classes: " + ex.Message);
            }
        }

        private DataTable GetClassTable()
        {
            using (SqlConnection con = new SqlConnection(_conn))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ClassID, ClassName FROM Classes ORDER BY ClassName", con))
            {
                con.Open();
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        /// <summary>Load (or refresh) the student GridView with optional filters.</summary>
        private void LoadStudentGrid(string search = "",
                                     int classFilter = 0,
                                     string medium = "")
        {
            try
            {
                const string sql = @"
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY c.ClassName, sc.StudentRollNo) AS RowNum,
                        s.StudentID,
                        s.StudentImageData AS Image,
                        s.Regno,
                        s.Name,
                        s.FatherName,
                        s.ContactNo,
                        s.Gender,
                        s.StudyMedium,
                        CONVERT(DATE, s.DateofAdmission) AS DateofAdmission,
                        CONVERT(DATE, s.CurrentClassEnrollmentDate) AS CurrentClassEnrollmentDate,
                        c.ClassName,
                        sc.StudentRollNo
                    FROM Students s
                    INNER JOIN StudentClass sc ON sc.StudentID = s.StudentID
                                                  AND sc.ClassID = s.CurrentClassID
                    INNER JOIN Classes      c  ON c.ClassID   = sc.ClassID
                    WHERE
                        (@Search = ''  OR s.Name LIKE '%'+@Search+'%'
                                       OR s.Regno LIKE '%'+@Search+'%')
                        AND (@ClassID  = 0  OR sc.ClassID   = @ClassID)
                        AND (@Medium   = ''  OR s.StudyMedium = @Medium)
                    ORDER BY c.ClassName, sc.StudentRollNo";

                using (SqlConnection con = new SqlConnection(_conn))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Search", search ?? "");
                    cmd.Parameters.AddWithValue("@ClassID", classFilter);
                    cmd.Parameters.AddWithValue("@Medium", medium ?? "");

                    con.Open();
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    gvStudents.DataSource = dt;
                    gvStudents.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowError("Failed to load student list: " + ex.Message);
            }
        }

        protected string GetStudentImageUrl(object imageValue)
        {
            return ImageDisplayHelper.GetImageUrl(this, imageValue, "images/students");
        }

        /// <summary>Load dashboard stat counters.</summary>
        private void LoadStats()
        {
            try
            {
                const string sql = @"
                    SELECT
                        (SELECT COUNT(*) FROM Students)                          AS Total,
                        (SELECT COUNT(*) FROM Classes)                           AS Classes,
                        (SELECT COUNT(*) FROM Students WHERE Gender = 'Male')    AS Male,
                        (SELECT COUNT(*) FROM Students WHERE Gender = 'Female')  AS Female";

                using (SqlConnection con = new SqlConnection(_conn))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            lblTotal.Text = dr["Total"].ToString();
                            lblClasses.Text = dr["Classes"].ToString();
                            lblMale.Text = dr["Male"].ToString();
                            lblFemale.Text = dr["Female"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Stats error: " + ex.Message);
            }
        }

        /// <summary>Compute the next roll number for a class, optionally excluding
        /// the current student (so the existing assignment is not counted twice).</summary>
        private int GetNextRollNumber(int classId, int excludeStudentId = 0)
        {
            try
            {
                const string sql = @"
                    SELECT ISNULL(MAX(StudentRollNo), 0) + 1
                    FROM StudentClass
                    WHERE ClassID = @ClassID
                      AND (@ExcludeID = 0 OR StudentID <> @ExcludeID)";

                using (SqlConnection con = new SqlConnection(_conn))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classId);
                    cmd.Parameters.AddWithValue("@ExcludeID", excludeStudentId);
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                ShowError("Roll number error: " + ex.Message);
                return 1;
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region ── Search Events ──

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                HideMessages();
                string search = txtSearchStudent.Text.Trim();
                int classId = Convert.ToInt32(ddlSearchClass.SelectedValue);

                if (string.IsNullOrEmpty(search) && classId == 0)
                {
                    ShowInfo("Enter a name, reg no, or select a class to search.");
                    return;
                }

                // Refresh grid with the search terms
                gvStudents.PageIndex = 0;
                LoadStudentGrid(search, classId);
            }
            catch (Exception ex)
            {
                ShowError("Search error: " + ex.Message);
            }
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            try
            {
                HideMessages();
                txtSearchStudent.Text = string.Empty;
                ddlSearchClass.SelectedIndex = 0;
                gvStudents.PageIndex = 0;
                LoadStudentGrid();
            }
            catch (Exception ex)
            {
                ShowError("Clear error: " + ex.Message);
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region ── GridView Events ──

        protected void gvStudents_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditStudent") return;

            try
            {
                HideMessages();
                int studentId = Convert.ToInt32(e.CommandArgument);
                LoadStudentIntoForm(studentId);
                pnlEditForm.Visible = true;

                // Scroll to form
                ScriptManager.RegisterStartupScript(this, GetType(), "scroll",
                    "window.scrollTo({top:0,behavior:'smooth'});", true);
            }
            catch (Exception ex)
            {
                ShowError("Could not open student for editing: " + ex.Message);
            }
        }

        protected void gvStudents_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvStudents.PageIndex = e.NewPageIndex;
                LoadStudentGrid(txtGridSearch.Text.Trim(),
                                Convert.ToInt32(ddlGridClass.SelectedValue),
                                ddlGridMedium.SelectedValue);
            }
            catch (Exception ex)
            {
                ShowError("Paging error: " + ex.Message);
            }
        }

        protected void txtGridSearch_Changed(object sender, EventArgs e)
        {
            try
            {
                gvStudents.PageIndex = 0;
                LoadStudentGrid(txtGridSearch.Text.Trim(),
                                Convert.ToInt32(ddlGridClass.SelectedValue),
                                ddlGridMedium.SelectedValue);
            }
            catch (Exception ex)
            {
                ShowError("Search error: " + ex.Message);
            }
        }

        protected void ddlGridClass_Changed(object sender, EventArgs e)
        {
            try
            {
                gvStudents.PageIndex = 0;
                LoadStudentGrid(txtGridSearch.Text.Trim(),
                                Convert.ToInt32(ddlGridClass.SelectedValue),
                                ddlGridMedium.SelectedValue);
            }
            catch (Exception ex)
            {
                ShowError("Filter error: " + ex.Message);
            }
        }

        protected void ddlGridMedium_Changed(object sender, EventArgs e)
        {
            try
            {
                gvStudents.PageIndex = 0;
                LoadStudentGrid(txtGridSearch.Text.Trim(),
                                Convert.ToInt32(ddlGridClass.SelectedValue),
                                ddlGridMedium.SelectedValue);
            }
            catch (Exception ex)
            {
                ShowError("Filter error: " + ex.Message);
            }
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                txtGridSearch.Text = string.Empty;
                ddlGridClass.SelectedIndex = 0;
                ddlGridMedium.SelectedIndex = 0;
                gvStudents.PageIndex = 0;
                LoadStudentGrid();
                LoadStats();
                HideMessages();
            }
            catch (Exception ex)
            {
                ShowError("Refresh error: " + ex.Message);
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region ── Load Student Into Form ──

        private void LoadStudentIntoForm(int studentId)
        {
            try
            {
                const string sql = @"
                    SELECT
                        s.StudentID, s.Regno, s.Name, s.FatherName,
                        s.FormBNo, s.FatherCNICNo,
                        CONVERT(VARCHAR(10), s.DOB, 23)             AS DOB,
                        s.Address, s.ContactNo, s.Gender,
                        s.AdmissionClass,
                        ISNULL(admissionClass.ClassName, CONVERT(NVARCHAR(100),s.AdmissionClass)) AS AdmissionClassName,
                        CONVERT(VARCHAR(10), s.DateofAdmission, 23) AS DateofAdmission,
                        CONVERT(VARCHAR(10), s.CurrentClassEnrollmentDate, 23) AS CurrentClassEnrollmentDate,
                        s.StudyMedium, s.StudentImageData AS Image, s.Image AS LegacyImage,
                        sc.ClassID, sc.StudentRollNo,
                        c.ClassName
                    FROM Students s
                    INNER JOIN StudentClass sc ON sc.StudentID = s.StudentID
                                                  AND sc.ClassID = s.CurrentClassID
                    INNER JOIN Classes      c  ON c.ClassID   = sc.ClassID
                    LEFT JOIN Classes admissionClass ON admissionClass.ClassID=TRY_CONVERT(INT,s.AdmissionClass)
                    WHERE s.StudentID = @StudentID";

                using (SqlConnection con = new SqlConnection(_conn))
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            ShowError("Student record not found.");
                            return;
                        }

                        // Hidden ID
                        hfStudentID.Value = dr["StudentID"].ToString();

                        // Fields
                        txtRegNo.Text = dr["Regno"].ToString();
                        txtName.Text = dr["Name"].ToString();
                        txtFatherName.Text = dr["FatherName"].ToString();
                        txtFormBNo.Text = dr["FormBNo"].ToString();
                        txtCNIC.Text = dr["FatherCNICNo"].ToString();
                        txtDOB.Text = dr["DOB"].ToString();
                        txtAddress.Text = dr["Address"].ToString();
                        txtContact.Text = dr["ContactNo"].ToString();
                        txtAdmDate.Text = dr["DateofAdmission"].ToString();
                        txtAdmissionClass.Text = dr["AdmissionClassName"].ToString();
                        txtCurrentEnrollmentDate.Text = dr["CurrentClassEnrollmentDate"].ToString();
                        txtRollNo.Text = dr["StudentRollNo"].ToString();

                        // Gender
                        rbMale.Checked = dr["Gender"].ToString() == "Male";
                        rbFemale.Checked = dr["Gender"].ToString() == "Female";

                        // Class dropdown
                        string classId = dr["ClassID"].ToString();
                        if (ddlClass.Items.FindByValue(classId) != null)
                            ddlClass.SelectedValue = classId;

                        // Medium dropdown
                        string med = dr["StudyMedium"].ToString();
                        if (ddlMedium.Items.FindByValue(med) != null)
                            ddlMedium.SelectedValue = med;

                        // Roll pill
                        lblRollPill.Visible = !string.IsNullOrEmpty(dr["StudentRollNo"].ToString());

                        // Status strip
                        lblStripName.Text = dr["Name"].ToString();
                        lblStripRegno.Text = dr["Regno"].ToString();
                        lblStripClass.Text = dr["ClassName"].ToString();
                        lblStripRoll.Text = dr["StudentRollNo"].ToString();

                        // Existing photo: SQL binary data is authoritative;
                        // the old path is used only during legacy migration.
                        object imageValue = StudentImageStorage.GetPreferredImageValue(
                            dr["Image"], dr["LegacyImage"]);
                        string imageUrl = ImageDisplayHelper.GetImageUrl(
                            this, imageValue, "images/students");
                        imgPreview.ImageUrl = imageUrl;
                        imgPreview.Visible = true;
                        imgStripPhoto.ImageUrl = imageUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Failed to load student data: " + ex.Message);
                throw; // re-throw so caller can catch
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region ── Class Changed  Auto Roll Number ──

        protected void ddlClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                HideMessages();

                if (ddlClass.SelectedValue == "0")
                {
                    txtRollNo.Text = string.Empty;
                    lblRollPill.Visible = false;
                    return;
                }

                int classId = Convert.ToInt32(ddlClass.SelectedValue);
                int studentId = string.IsNullOrEmpty(hfStudentID.Value)
                                ? 0 : Convert.ToInt32(hfStudentID.Value);

                // Exclude current student so it doesn't double-count their existing slot
                int rollNo = GetNextRollNumber(classId, studentId);

                txtRollNo.Text = rollNo.ToString();
                lblRollPill.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError("Roll number calculation failed: " + ex.Message);
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region ── Update Student ──

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            HideMessages();

            if (!Page.IsValid) return;

            if (string.IsNullOrEmpty(hfStudentID.Value))
            {
                ShowError("No student selected. Please search and select a student first.");
                return;
            }

            SqlConnection con = null;
            SqlTransaction tx = null;

            try
            {
                int studentId = Convert.ToInt32(hfStudentID.Value);
                string gender = rbMale.Checked ? "Male" : "Female";

                // Read a replacement photo directly into memory. A null value
                // means keep the current database image.
                StudentUploadedImage newImage = StudentImageStorage.ReadUploadedImage(fuPhoto);

                // ── Open connection + transaction ───────────────────────────
                con = new SqlConnection(_conn);
                con.Open();
                tx = con.BeginTransaction();

                // ── 1. Update Students table ────────────────────────────────
                string sqlUpdate = newImage != null
                    ? @"UPDATE Students SET
                            Regno=@Regno, Name=@Name, FatherName=@FatherName,
                            FormBNo=@FormBNo, FatherCNICNo=@CNIC,
                            DOB=@DOB, Address=@Address, ContactNo=@Contact,
                            Gender=@Gender, StudyMedium=@Medium,
                            StudentImageData=@StudentImageData,
                            StudentImageContentType=@StudentImageContentType,
                            StudentImageFileName=@StudentImageFileName
                        WHERE StudentID=@StudentID"
                    : @"UPDATE Students SET
                            Regno=@Regno, Name=@Name, FatherName=@FatherName,
                            FormBNo=@FormBNo, FatherCNICNo=@CNIC,
                            DOB=@DOB, Address=@Address, ContactNo=@Contact,
                            Gender=@Gender, StudyMedium=@Medium
                        WHERE StudentID=@StudentID";

                using (SqlCommand cmd = new SqlCommand(sqlUpdate, con, tx))
                {
                    cmd.Parameters.AddWithValue("@Regno", txtRegNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("@FatherName", txtFatherName.Text.Trim());
                    cmd.Parameters.AddWithValue("@FormBNo", txtFormBNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@CNIC", txtCNIC.Text.Trim());
                    cmd.Parameters.AddWithValue("@DOB", Convert.ToDateTime(txtDOB.Text));
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@Contact", txtContact.Text.Trim());
                    cmd.Parameters.AddWithValue("@Gender", gender);
                    cmd.Parameters.AddWithValue("@Medium", ddlMedium.SelectedValue);
                    if (newImage != null)
                    {
                        cmd.Parameters.Add("@StudentImageData", SqlDbType.VarBinary, -1).Value = newImage.Data;
                        cmd.Parameters.Add("@StudentImageContentType", SqlDbType.NVarChar, 100).Value = newImage.ContentType;
                        cmd.Parameters.Add("@StudentImageFileName", SqlDbType.NVarChar, 260).Value = newImage.FileName;
                    }
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    cmd.ExecuteNonQuery();
                }

                // Admission class/date and current placement are protected here.
                // Student Promotion records every class movement permanently.
                int newRollNo = GetExistingRollNo(studentId, con, tx);

                // ── 4. Commit ────────────────────────────────────────────────
                tx.Commit();

                ShowSuccess($" Student <strong>{txtName.Text.Trim()}</strong> updated successfully! " +
                            $"Current Class: <strong>{ddlClass.SelectedItem.Text}</strong>, " +
                            $"Roll No: <strong>{newRollNo}</strong>");

                // Refresh grid & stats
                LoadStudentGrid();
                LoadStats();

                // Hide form
                pnlEditForm.Visible = false;
                hfStudentID.Value = string.Empty;
            }
            catch (SqlException sqlEx)
            {
                SafeRollback(tx);
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                    ShowError("A student with this Registration Number already exists.");
                else
                    ShowError("Database error: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                SafeRollback(tx);
                ShowError("Update failed: " + ex.Message);
            }
            finally
            {
                con?.Close();
                con?.Dispose();
            }
        }

        // Helper: get the ClassID currently stored for this student
        private int GetExistingClassId(int studentId, SqlConnection con, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ClassID FROM StudentClass WHERE StudentID=@SID", con, tx))
            {
                cmd.Parameters.AddWithValue("@SID", studentId);
                object val = cmd.ExecuteScalar();
                return val == null ? 0 : Convert.ToInt32(val);
            }
        }

        // Helper: get existing roll number inside a transaction
        private int GetExistingRollNo(int studentId, SqlConnection con, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(
                "SELECT StudentRollNo FROM StudentClass WHERE StudentID=@SID", con, tx))
            {
                cmd.Parameters.AddWithValue("@SID", studentId);
                object val = cmd.ExecuteScalar();
                return val == null ? 1 : Convert.ToInt32(val);
            }
        }

        // Helper: compute next roll number inside a transaction (avoids race condition)
        private int GetNextRollNumberTx(int classId, int excludeId,
                                        SqlConnection con, SqlTransaction tx)
        {
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT ISNULL(MAX(StudentRollNo),0)+1
                  FROM StudentClass
                  WHERE ClassID=@CID AND StudentID<>@EID", con, tx))
            {
                cmd.Parameters.AddWithValue("@CID", classId);
                cmd.Parameters.AddWithValue("@EID", excludeId);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void SafeRollback(SqlTransaction tx)
        {
            try { tx?.Rollback(); }
            catch { /* swallow rollback errors */ }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region ── Cancel / Reset ──

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            try
            {
                pnlEditForm.Visible = false;
                hfStudentID.Value = string.Empty;
                HideMessages();
                LoadStudentGrid();
            }
            catch (Exception ex)
            {
                ShowError("Cancel error: " + ex.Message);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                HideMessages();
                if (!string.IsNullOrEmpty(hfStudentID.Value))
                {
                    int id = Convert.ToInt32(hfStudentID.Value);
                    LoadStudentIntoForm(id);
                    ShowInfo("Form reloaded with original data.");
                }
            }
            catch (Exception ex)
            {
                ShowError("Reload error: " + ex.Message);
            }
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        #region ── UI Helpers ──

        private void ShowSuccess(string msg)
        {
            pnlOk.Visible = true;
            pnlErr.Visible = false;
            pnlInfo.Visible = false;
            lblOk.Text = msg;
        }

        private void ShowError(string msg)
        {
            pnlErr.Visible = true;
            pnlOk.Visible = false;
            pnlInfo.Visible = false;
            lblErr.Text = msg;
        }

        private void ShowInfo(string msg)
        {
            pnlInfo.Visible = true;
            pnlOk.Visible = false;
            pnlErr.Visible = false;
            lblInfo.Text = msg;
        }

        private void HideMessages()
        {
            pnlOk.Visible = false;
            pnlErr.Visible = false;
            pnlInfo.Visible = false;
        }

        #endregion
    }
}
