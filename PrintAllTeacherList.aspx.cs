using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class WebForm5 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                if (!IsPostBack)
                {
                    LoadTeachers();
                }
            }
            catch (Exception ex)
            {
                Response.Write("Unexpected error while Processing your Request: error detail is:  " + ex.Message);
            }
        }
        private void LoadTeachers()
        {
            try
            {

                string cs = ConfigurationManager
                    .ConnectionStrings["SchoolDB"]
                    .ConnectionString;

                using (SqlConnection con = new SqlConnection(cs))
                {
                    SqlDataAdapter da = new SqlDataAdapter(@"
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
    T.MaritalStatus,
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
WHERE ISNULL(T.IsActive,1)=1
ORDER BY T.Name;", con);

                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    gvTeachers.DataSource = dt;
                    gvTeachers.DataBind();
                }
            }
            catch (Exception ex)
            {
                Response.Write("Unexpected error while Processing your Request: error detail is:  " + ex.Message);
            }
        }
        protected void btnShowAll_Click(object sender, EventArgs e)
        {
            try
            {
                txtSearch.Text = "";
                LoadTeachers();
            }
            catch (Exception ex)
            {
                Response.Write("Unexpected error while Processing your Request: error detail is:  " + ex.Message);
            }
        }
        protected string GetImage(object img)
        {
            if (img == DBNull.Value || img == null)
                return "~/images/noimage.png";

            byte[] bytes = (byte[])img;

            return "data:image/png;base64," +
                   Convert.ToBase64String(bytes);
        }
        protected string GetTeacherCardUrl(object teacherId)
        {
            if (teacherId == null || teacherId == DBNull.Value)
                return "#";

            return ResolveUrl("~/TeacherCard.aspx?id=" +
                HttpUtility.UrlEncode(teacherId.ToString()));
        }

        protected string GetTeacherProfileUrl(object teacherId)
        {
            if (teacherId == null || teacherId == DBNull.Value)
                return "#";

            return ResolveUrl("~/PrintTeacherProfile.aspx?id=" +
                HttpUtility.UrlEncode(teacherId.ToString()));
        }

        protected void btnPrint_Click(object sender, EventArgs e)
        {

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {

                string cs = ConfigurationManager
                    .ConnectionStrings["SchoolDB"]
                    .ConnectionString;

                using (SqlConnection con = new SqlConnection(cs))
                {
                    SqlDataAdapter da = new SqlDataAdapter(@"

        SELECT
            T.*,
            V.Description,
            V.BPS
        FROM Teachers T
        LEFT JOIN TeachingVacancyPosition V
             ON T.PostID = V.PostID
        WHERE ISNULL(T.IsActive,1)=1 AND (T.Name LIKE '%' + @Search + '%'
           OR T.cnicno LIKE '%' + @Search + '%')
        ORDER BY T.Name

        ", con);

                    da.SelectCommand.Parameters.AddWithValue(
                        "@Search",
                        txtSearch.Text.Trim());

                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    gvTeachers.DataSource = dt;
                    gvTeachers.DataBind();
                }
            }
            catch (Exception ex)
            {
                Response.Write("Unexpected error while Processing your Request: error detail is:  " + ex.Message);
            }
        }
    }

}
