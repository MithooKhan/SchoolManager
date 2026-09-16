using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class WebForm7 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    LoadVacantPosts();
                    LoadGrid();
                }
            }
            catch (Exception ex)
            {
                Response.Write("Unexpected error while Processing your Request: error detail is:  " + ex.Message);
            }
        }
        private void LoadVacantPosts()
        {
            try
            {
                string cs = ConfigurationManager
                    .ConnectionStrings["SchoolDB"]
                    .ConnectionString;

                using (SqlConnection con =
                    new SqlConnection(cs))
                {
                    SqlCommand cmd = new SqlCommand(@"

            SELECT
                PostID,
                Description +
                ' (BPS-' +
                CAST(BPS AS VARCHAR(10))
                + ')' AS PostName

            FROM Non_TeachingVacancyPosition

            WHERE Vacant > 0

            ORDER BY Description

        ", con);

                    con.Open();

                    ddlPost.DataSource =
                        cmd.ExecuteReader();

                    ddlPost.DataTextField = "PostName";
                    ddlPost.DataValueField = "PostID";

                    ddlPost.DataBind();

                    ddlPost.Items.Insert(
                        0,
                        new ListItem("-- Select Post --", ""));
                }
            }
            catch (Exception ex)
            {
                Response.Write("Unexpected error while Processing your Request: error detail is:  " + ex.Message);
            }
        }
        protected void gvStaff_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try {
                gvStaff.PageIndex = e.NewPageIndex;
                LoadGrid();
            }
            catch (Exception ex)
            {
                Response.Write("Unexpected error while Processing your Request: error detail is:  " + ex.Message);
            }
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {

                string cs =
                    ConfigurationManager
                    .ConnectionStrings["SchoolDB"]
                    .ConnectionString;

                ProcessedImage staffImage =
                    ImageUploadProcessor.ReadAndOptimize(fuImage, 1200, 1600);
                byte[] imageData = staffImage == null ? null : staffImage.Data;

                using (SqlConnection con =
                        new SqlConnection(cs))
                {
                    con.Open();

                    SqlTransaction tran =
                        con.BeginTransaction();

                    try
                    {
                        SqlCommand cmd =
                            new SqlCommand(@"
                INSERT INTO NonTeachingStaff 
     (Name, FatherName, Gender, DOB, CNICNo, ContactNo, EmailID, Address,
     MaritalStatus, Qualification, PersonalNo, DateOfJoining, DateOfJoiningInThisSchool,
     DateOfFirstAppointment, DateOfContractAppointment,
     RegularAppointmentDate, DateOfAwardCurrentGrade,
     PostID, Image)
VALUES 
    (@Name, @FatherName, @Gender, @DOB, @CNICNo, @ContactNo, @EmailID, @Address,
     @MaritalStatus, @Qualification, @PersonalNo, @DateOfJoining, @DateOfJoiningInThisSchool,
     @DateOfFirstAppointment, @DateOfContractAppointment,
     @RegularAppointmentDate, @DateOfAwardCurrentGrade,
     @PostID, @Image)", con, tran);

                        cmd.Parameters.AddWithValue("@Name", txtName.Text);
                        cmd.Parameters.AddWithValue("@FatherName", txtFatherName.Text);
                        cmd.Parameters.AddWithValue("@Gender", ddlGender.SelectedValue);
                        cmd.Parameters.AddWithValue("@DOB", txtDOB.Text);
                        cmd.Parameters.AddWithValue("@CNICNo", txtCNIC.Text);
                        cmd.Parameters.AddWithValue("@ContactNo", txtContact.Text);
                        cmd.Parameters.AddWithValue("@EmailID", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@MaritalStatus", ddlMaritalStatus.SelectedValue);
                        cmd.Parameters.AddWithValue("@Qualification", txtQualification.Text);
                        cmd.Parameters.AddWithValue("@PersonalNo", txtPersonalNo.Text);
                        cmd.Parameters.AddWithValue("@DateOfJoining", txtDateJoining.Text);
                        cmd.Parameters.AddWithValue("@DateOfJoiningInThisSchool", txtJoiningSchool.Text);
                        cmd.Parameters.AddWithValue("@DateOfFirstAppointment", txtFirstAppointment.Text);
                        cmd.Parameters.AddWithValue("@DateOfContractAppointment", txtContractAppointment.Text);
                        cmd.Parameters.AddWithValue("@RegularAppointmentDate", string.IsNullOrEmpty(txtRegularAppointmentDate.Text) ? (object)DBNull.Value : DateTime.Parse(txtRegularAppointmentDate.Text));
                        cmd.Parameters.AddWithValue("@DateOfAwardCurrentGrade", string.IsNullOrEmpty(txtDateOfAwardCurrentGrade.Text) ? (object)DBNull.Value : DateTime.Parse(txtDateOfAwardCurrentGrade.Text));
                        cmd.Parameters.AddWithValue("@Image", (imageData != null) ? (object)imageData : DBNull.Value);
                        cmd.Parameters.AddWithValue("@PostID", ddlPost.SelectedValue);

                        cmd.ExecuteNonQuery();

                        SqlCommand cmdUpdate =
                            new SqlCommand(@"

                UPDATE Non_TeachingVacancyPosition

                SET Working = Working + 1

                WHERE PostID=@PostID

            ", con, tran);

                        cmdUpdate.Parameters.AddWithValue(
                            "@PostID",
                            ddlPost.SelectedValue);

                        cmdUpdate.ExecuteNonQuery();

                        tran.Commit();

                        ScriptManager.RegisterStartupScript(
                            this,
                            GetType(),
                            "msg",
                            "alert('Staff Record Saved Successfully');",
                            true);

                        ClearForm();
                        LoadVacantPosts();
                        LoadGrid();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write("Unexpected error while Processing your Request: error detail is:  " + ex.Message);
            }
        }
        protected void gvStaff_RowDataBound(
    object sender,
    GridViewRowEventArgs e)
        {
            try
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    Image img =
                        (Image)e.Row.FindControl("imgGridStaff");

                    DataRowView drv =
                        (DataRowView)e.Row.DataItem;

                    if (drv["Image"] != DBNull.Value)
                    {
                        byte[] bytes =
                            (byte[])drv["Image"];

                        string base64 =
                            Convert.ToBase64String(bytes);

                        img.ImageUrl =
                            "data:image/jpeg;base64," +
                            base64;
                    }
                    else
                    {
                        img.ImageUrl =
                            "~/images/noimage.png";
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write("Unexpected error while Processing your Request: error detail is:  " + ex.Message);
            }
        }
        private void ClearForm()
        {
            txtRegularAppointmentDate.Text = "";
            txtDateOfAwardCurrentGrade.Text = "";
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

            ddlGender.SelectedIndex = 0;
            ddlMaritalStatus.SelectedIndex = 0;
            ddlPost.SelectedIndex = 0;

            imgStaff.ImageUrl =
                "~/images/noimage.png";

            txtName.Focus();
        }
        private void LoadGrid()
        {
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString;
                using (SqlConnection con = new SqlConnection(cs))
                {
                    string query = @"SELECT s.StaffID, s.Name, s.FatherName, s.Gender, s.DOB,
                                s.CNICNo, s.ContactNo, s.EmailID, s.Address,
                                s.MaritalStatus, s.Qualification, s.PersonalNo,
                                s.DateOfJoining, s.DateOfJoiningInThisSchool,
                                s.DateOfFirstAppointment, s.DateOfContractAppointment,
                                s.RegularAppointmentDate, s.DateOfAwardCurrentGrade,
                                s.Image,
                                p.Description, p.BPS
                         FROM NonTeachingStaff s
                         LEFT JOIN Non_TeachingVacancyPosition p ON s.PostID = p.PostID
                         ORDER BY s.StaffID DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvStaff.DataSource = dt;
                    gvStaff.DataBind();
                }
            }
            catch (Exception ex)
            {
                Response.Write("Unexpected error while Processing your Request: error detail is:  " + ex.Message);
            }
        }
        public string GetImageUrl(string staffId)
        {
           
            if (!string.IsNullOrEmpty(staffId) && staffId != "0")
            {
                return ResolveUrl("~/ShowImage.ashx?id=" + staffId);
            }
            return ResolveUrl("~/images/noimage.png");
        }
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm(); 
        }
    }
    }
