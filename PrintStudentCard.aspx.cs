using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class WebForm26 : System.Web.UI.Page
    {
        protected global::System.Web.UI.WebControls.Label lblMessage;
        protected global::System.Web.UI.WebControls.DropDownList ddlClasses;
        protected global::System.Web.UI.WebControls.DropDownList ddlStudents;
        protected global::System.Web.UI.WebControls.Button btnPrint;
        protected global::System.Web.UI.WebControls.Repeater rptStudentCards;

        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                StudentImageStorage.EnsureSchema(this, _connectionString);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Student image database storage is not ready. Run StudentImage_Database_Update.sql. " + ex.Message;
                return;
            }

            if (!IsPostBack)
            {
                LoadClasses();
                ddlStudents.Items.Clear();
                ddlStudents.Items.Add(new ListItem("Select a class first", "0"));
                ddlStudents.Enabled = false;
            }
        }

        private void LoadClasses()
        {
            const string query = @"SELECT ClassID, ClassName
                                   FROM dbo.Classes
                                   ORDER BY ClassName";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ddlClasses.DataSource = reader;
                        ddlClasses.DataTextField = "ClassName";
                        ddlClasses.DataValueField = "ClassID";
                        ddlClasses.DataBind();
                    }
                }

                ddlClasses.Items.Insert(0, new ListItem("Select a class", "0"));
            }
            catch (Exception ex)
            {
                ShowMessage("Classes could not be loaded. " + ex.Message);
            }
        }

        protected void ddlClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            int classId;
            if (!int.TryParse(ddlClasses.SelectedValue, out classId) || classId <= 0)
            {
                ddlStudents.Items.Clear();
                ddlStudents.Items.Add(new ListItem("Select a class first", "0"));
                ddlStudents.Enabled = false;
                ClearCards();
                return;
            }

            LoadStudentsOfClass(classId);
            LoadCards(classId, 0);
        }

        private void LoadStudentsOfClass(int classId)
        {
            const string query = @"SELECT s.StudentID,
                                          s.Name + ' (' + s.RegNo + ')' AS StudentDisplay
                                   FROM dbo.Students s
                                   INNER JOIN dbo.StudentClass sc ON s.StudentID = sc.StudentID
                                   WHERE sc.ClassID = @ClassID
                                     AND LTRIM(RTRIM(ISNULL(s.Isactive, 'Active'))) IN ('Active', 'True', '1')
                                   ORDER BY s.Name";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@ClassID", SqlDbType.Int).Value = classId;
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        ddlStudents.DataSource = reader;
                        ddlStudents.DataTextField = "StudentDisplay";
                        ddlStudents.DataValueField = "StudentID";
                        ddlStudents.DataBind();
                    }
                }

                ddlStudents.Items.Insert(0, new ListItem("All students", "0"));
                ddlStudents.Enabled = true;
            }
            catch (Exception ex)
            {
                ddlStudents.Items.Clear();
                ddlStudents.Items.Add(new ListItem("Students unavailable", "0"));
                ddlStudents.Enabled = false;
                ShowMessage("Students could not be loaded. " + ex.Message);
            }
        }

        protected void ddlStudents_SelectedIndexChanged(object sender, EventArgs e)
        {
            int classId;
            int studentId;
            if (int.TryParse(ddlClasses.SelectedValue, out classId) && classId > 0 &&
                int.TryParse(ddlStudents.SelectedValue, out studentId))
            {
                LoadCards(classId, studentId);
            }
        }

        private void LoadCards(int classId, int studentId)
        {
            string query = @"SELECT s.StudentID,
                                    s.RegNo,
                                    s.Name,
                                    s.FatherName,
                                    s.Address,
                                    s.ContactNo,
                                    s.StudentImageData AS Image,
                                    sc.StudentRollNo,
                                    c.ClassName
                             FROM dbo.Students s
                             INNER JOIN dbo.StudentClass sc ON s.StudentID = sc.StudentID
                             INNER JOIN dbo.Classes c ON sc.ClassID = c.ClassID
                             WHERE sc.ClassID = @ClassID
                               AND LTRIM(RTRIM(ISNULL(s.Isactive, 'Active'))) IN ('Active', 'True', '1')";

            if (studentId > 0)
            {
                query += " AND s.StudentID = @StudentID";
            }

            query += " ORDER BY sc.StudentRollNo, s.Name";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@ClassID", SqlDbType.Int).Value = classId;
                    if (studentId > 0)
                    {
                        command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;
                    }

                    DataTable cards = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(cards);
                    }

                    rptStudentCards.DataSource = cards;
                    rptStudentCards.DataBind();
                    btnPrint.Enabled = cards.Rows.Count > 0;

                    if (cards.Rows.Count == 0)
                    {
                        ShowMessage("No active student records were found for this selection.");
                    }
                    else
                    {
                        lblMessage.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                ClearCards();
                ShowMessage("Student cards could not be loaded. " + ex.Message);
            }
        }

        protected string GetStudentPhotoUrl(object imageValue)
        {
            return ImageDisplayHelper.GetImageUrl(this, imageValue, "images/students");
        }

        private void ClearCards()
        {
            rptStudentCards.DataSource = null;
            rptStudentCards.DataBind();
            btnPrint.Enabled = false;
        }

        private void ShowMessage(string message)
        {
            lblMessage.Text = message;
        }
    }
}
