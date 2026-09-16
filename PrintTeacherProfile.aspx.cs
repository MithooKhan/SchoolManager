using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class WebForm4 : System.Web.UI.Page
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
            if (!IsPostBack)
            {
                ClearProfile();
            }
        }

        // =====================================================================
        // SEARCH BY NAME OR CNIC
        // =====================================================================
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string name = txtNameSearch.Text.Trim();
                string cnic = txtCNICSearch.Text.Trim();

                if (string.IsNullOrWhiteSpace(name) &&
                    string.IsNullOrWhiteSpace(cnic))
                {
                    ShowMessage(
                        "Enter teacher name or CNIC to search.");
                    return;
                }

                using (SqlConnection con =
                    new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        teacherid,
                        Name,
                        cnicno
                    FROM Teachers
                    WHERE
                        ISNULL(IsActive,1)=1 AND
                        (@Name = '' OR Name LIKE '%' + @Name + '%')
                        AND
                        (@CNIC = '' OR cnicno LIKE '%' + @CNIC + '%')
                    ORDER BY Name;", con))
                {
                    cmd.Parameters.Add(
                        "@Name",
                        SqlDbType.NVarChar,
                        150).Value = name;

                    cmd.Parameters.Add(
                        "@CNIC",
                        SqlDbType.NVarChar,
                        30).Value = cnic;

                    DataTable dt = new DataTable();

                    using (SqlDataAdapter da =
                        new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count == 0)
                    {
                        pnlSearchResults.Visible = false;
                        pnlProfile.Visible = false;

                        ShowMessage(
                            "No teacher record found.");
                        return;
                    }

                    ddlTeachers.Items.Clear();
                    ddlTeachers.Items.Add(
                        new ListItem(
                            "-- Select Matching Teacher --",
                            ""));

                    foreach (DataRow row in dt.Rows)
                    {
                        string text =
                            row["Name"].ToString();

                        if (row["cnicno"] != DBNull.Value &&
                            !string.IsNullOrWhiteSpace(
                                row["cnicno"].ToString()))
                        {
                            text +=
                                " - " +
                                row["cnicno"].ToString();
                        }

                        ddlTeachers.Items.Add(
                            new ListItem(
                                text,
                                row["teacherid"].ToString()));
                    }

                    pnlSearchResults.Visible = true;

                    // If only one exact/partial match is found, load it immediately.
                    if (dt.Rows.Count == 1)
                    {
                        int teacherId =
                            Convert.ToInt32(
                                dt.Rows[0]["teacherid"]);

                        ddlTeachers.SelectedValue =
                            teacherId.ToString();

                        LoadTeacher(teacherId);
                    }
                    else
                    {
                        pnlProfile.Visible = false;

                        ShowMessage(
                            dt.Rows.Count +
                            " matching teachers found. Select one from the list.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Search failed: " +
                    ex.Message);
            }
        }

        protected void ddlTeachers_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            try
            {
                int teacherId;

                if (int.TryParse(
                    ddlTeachers.SelectedValue,
                    out teacherId))
                {
                    LoadTeacher(teacherId);
                }
                else
                {
                    pnlProfile.Visible = false;
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    "Unable to load teacher: " +
                    ex.Message);
            }
        }

        // =====================================================================
        // LOAD COMPLETE TEACHER BIODATA
        // =====================================================================
        private void LoadTeacher(int teacherId)
        {
            try
            {
                using (SqlConnection con =
                    new SqlConnection(ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT
                        T.teacherid,
                        T.Name,
                        T.fathername,
                        T.Gender,
                        T.dob,
                        T.cnicno,
                        T.contactno,
                        T.emailId,
                        T.address,
                        T.qualification,
                        T.Professionalqualification,
                        T.personalNo,
                        T.dateOfjoningGovtService,
                        T.dateOfjoininginthischool,
                        T.dateofRegularappointment,
                        T.DateOfContractAppointment,
                        T.DateofawardofCurrentGrade,
                        T.image,
                        V.Description,
                        V.BPS
                    FROM Teachers T
                    LEFT JOIN TeachingVacancyPosition V
                        ON T.PostID = V.PostID
                    WHERE T.teacherid = @TeacherID AND ISNULL(T.IsActive,1)=1;", con))
                {
                    cmd.Parameters.Add(
                        "@TeacherID",
                        SqlDbType.Int).Value = teacherId;

                    con.Open();

                    using (SqlDataReader dr =
                        cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            pnlProfile.Visible = false;

                            ShowMessage(
                                "Teacher record not found.");
                            return;
                        }

                        string name =
                            DbText(dr["Name"]);

                        string father =
                            DbText(dr["fathername"]);

                        string gender =
                            DbText(dr["Gender"]);

                        string cnic =
                            DbText(dr["cnicno"]);

                        string contact =
                            DbText(dr["contactno"]);

                        string email =
                            DbText(dr["emailId"]);

                        string address =
                            DbText(dr["address"]);

                        string qualification =
                            DbText(dr["qualification"]);

                        string professionalQualification =
                            DbText(
                                dr["Professionalqualification"]);

                        string personalNo =
                            DbText(dr["personalNo"]);

                        string designation =
                            DbText(dr["Description"]);

                        string bps =
                            DbText(dr["BPS"]);

                        lblName.Text =
                            H(name);

                        lblFatherName.Text =
                            H(OrDash(father));

                        lblGender.Text =
                            H(OrDash(gender));

                        lblCNIC.Text =
                            H(OrDash(cnic));

                        lblCNIC2.Text =
                            H(OrDash(cnic));

                        lblContact.Text =
                            H(OrDash(contact));

                        lblEmail.Text =
                            H(OrDash(email));

                        lblAddress.Text =
                            H(OrDash(address));

                        lblQualification.Text =
                            H(OrDash(qualification));

                        lblProfessionalQualification.Text =
                            H(OrDash(
                                professionalQualification));

                        lblPersonalNo.Text =
                            H(OrDash(personalNo));

                        lblDesignation.Text =
                            H(OrDash(designation));

                        lblDesignation2.Text =
                            H(OrDash(designation));

                        if (!string.IsNullOrWhiteSpace(bps))
                        {
                            lblBPS.Text =
                                "(BPS-" +
                                H(bps) +
                                ")";
                        }
                        else
                        {
                            lblBPS.Text = "";
                        }

                        lblBPS2.Text =
                            H(OrDash(bps));

                        lblDOB.Text =
                            DateText(dr["dob"]);

                        lblDateOfJoining.Text =
                            DateText(
                                dr["dateOfjoningGovtService"]);

                        lblJoiningSchool.Text =
                            DateText(
                                dr["dateOfjoininginthischool"]);

                        lblRegularAppointment.Text =
                            DateText(
                                dr["dateofRegularappointment"]);

                        lblContractAppointment.Text =
                            DateText(
                                dr["DateOfContractAppointment"]);

                        lblCurrentGradeDate.Text =
                            DateText(
                                dr["DateofawardofCurrentGrade"]);

                        LoadTeacherImage(dr["image"]);

                        pnlProfile.Visible = true;

                        ShowMessage(
                            "Teacher bio data loaded successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                pnlProfile.Visible = false;

                ShowMessage(
                    "Unable to load teacher bio data: " +
                    ex.Message);
            }
        }

        private void LoadTeacherImage(object imageValue)
        {
            if (imageValue == null ||
                imageValue == DBNull.Value)
            {
                imgTeacher.ImageUrl =
                    "~/images/noimage.png";
                return;
            }

            byte[] bytes =
                imageValue as byte[];

            if (bytes == null ||
                bytes.Length == 0)
            {
                imgTeacher.ImageUrl =
                    "~/images/noimage.png";
                return;
            }

            imgTeacher.ImageUrl =
                "data:image/jpeg;base64," +
                Convert.ToBase64String(bytes);
        }

        // =====================================================================
        // CLEAR
        // =====================================================================
        protected void btnClear_Click(
            object sender,
            EventArgs e)
        {
            txtNameSearch.Text = "";
            txtCNICSearch.Text = "";

            pnlSearchResults.Visible = false;

            ddlTeachers.Items.Clear();

            ClearProfile();

            pnlMessage.Visible = false;
        }

        private void ClearProfile()
        {
            pnlProfile.Visible = false;

            lblName.Text = "";
            lblFatherName.Text = "";
            lblGender.Text = "";
            lblDOB.Text = "";
            lblCNIC.Text = "";
            lblCNIC2.Text = "";
            lblContact.Text = "";
            lblEmail.Text = "";
            lblAddress.Text = "";
            lblQualification.Text = "";
            lblProfessionalQualification.Text = "";
            lblPersonalNo.Text = "";
            lblDesignation.Text = "";
            lblDesignation2.Text = "";
            lblBPS.Text = "";
            lblBPS2.Text = "";
            lblDateOfJoining.Text = "";
            lblJoiningSchool.Text = "";
            lblRegularAppointment.Text = "";
            lblContractAppointment.Text = "";
            lblCurrentGradeDate.Text = "";

            imgTeacher.ImageUrl =
                "~/images/noimage.png";
        }

        // =====================================================================
        // HELPERS
        // =====================================================================
        private static string DbText(object value)
        {
            return value == null ||
                   value == DBNull.Value
                ? ""
                : value.ToString();
        }

        private static string OrDash(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "-"
                : value;
        }

        private static string DateText(object value)
        {
            if (value == null ||
                value == DBNull.Value)
            {
                return "-";
            }

            return Convert.ToDateTime(value)
                .ToString("dd-MMM-yyyy");
        }

        private static string H(string value)
        {
            return HttpUtility.HtmlEncode(value ?? "");
        }

        private void ShowMessage(string message)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
        }
    }
}
