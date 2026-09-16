using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class WebForm14 : Page
    {
        private readonly string _connStr = ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SystemUserSecurity.IsAdministrator(Context)) { Response.Redirect(ResolveUrl("~/PortalLogin.aspx?ReturnUrl=%2fClassSubjectsManagement.aspx"), true); return; }
            if (IsPostBack) return;
            TryAction(delegate { EnsureSchema(); BindClasses(); BindSubjects(); BindGrid(); BindStats(); });
        }

        protected void btnNew_Click(object sender, EventArgs e) { ClearEditor(); }
        protected void btnCancel_Click(object sender, EventArgs e) { ClearEditor(); }
        protected void btnFilter_Click(object sender, EventArgs e) { TryAction(delegate { BindGrid(); }); }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            TryAction(delegate
            {
                int recordId; int.TryParse(hidRecordID.Value, out recordId);
                int classId = Convert.ToInt32(ddlClass.SelectedValue, CultureInfo.InvariantCulture);
                int subjectId = Convert.ToInt32(ddlSubject.SelectedValue, CultureInfo.InvariantCulture);
                string subjectGroup = ddlSubjectGroup.SelectedValue;
                bool selectable = chkStudentSelectable.Checked && subjectGroup == "Optional";
                string optionGroupCode = string.Empty; string optionGroupName = string.Empty;
                if (subjectGroup == "Optional" && !string.IsNullOrEmpty(ddlOptionGroup.SelectedValue))
                {
                    if (ddlOptionGroup.SelectedValue == "CUSTOM")
                    {
                        optionGroupName = Required(txtCustomGroupName.Text, "custom choice-group name", 100);
                        optionGroupCode = "CUSTOM_" + NormalizeCode(optionGroupName);
                    }
                    else { optionGroupCode = ddlOptionGroup.SelectedValue; optionGroupName = GroupName(optionGroupCode); }
                }
                if (selectable && string.IsNullOrEmpty(optionGroupCode))
                    throw new InvalidOperationException("Select a student choice group when the subject must be selected during registration.");
                int displayOrder; if (!int.TryParse(txtDisplayOrder.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out displayOrder) || displayOrder < 0) throw new InvalidOperationException("Enter a valid display order.");
                if (AssignmentExists(classId, subjectId, recordId)) throw new InvalidOperationException("This subject is already assigned to the selected class.");

                using (SqlConnection connection = new SqlConnection(_connStr))
                using (SqlCommand command = new SqlCommand(recordId > 0
                    ? @"UPDATE dbo.ClassSubjects SET ClassID=@ClassID,SubjectID=@SubjectID,SubjectGroup=@SubjectGroup,OptionGroupCode=@GroupCode,OptionGroupName=@GroupName,ReligionEligibility=@Religion,IsStudentSelectable=@Selectable,DisplayOrder=@DisplayOrder WHERE ClassSubjectID=@ID;"
                    : @"INSERT dbo.ClassSubjects(ClassID,SubjectID,SubjectGroup,OptionGroupCode,OptionGroupName,ReligionEligibility,IsStudentSelectable,DisplayOrder) VALUES(@ClassID,@SubjectID,@SubjectGroup,@GroupCode,@GroupName,@Religion,@Selectable,@DisplayOrder);", connection))
                {
                    command.Parameters.Add("@ClassID", SqlDbType.Int).Value = classId; command.Parameters.Add("@SubjectID", SqlDbType.Int).Value = subjectId;
                    command.Parameters.Add("@SubjectGroup", SqlDbType.NVarChar, 20).Value = subjectGroup;
                    command.Parameters.Add("@GroupCode", SqlDbType.NVarChar, 30).Value = string.IsNullOrEmpty(optionGroupCode) ? (object)DBNull.Value : optionGroupCode;
                    command.Parameters.Add("@GroupName", SqlDbType.NVarChar, 100).Value = string.IsNullOrEmpty(optionGroupName) ? (object)DBNull.Value : optionGroupName;
                    command.Parameters.Add("@Religion", SqlDbType.NVarChar, 20).Value = ddlReligionEligibility.SelectedValue;
                    command.Parameters.Add("@Selectable", SqlDbType.Bit).Value = selectable; command.Parameters.Add("@DisplayOrder", SqlDbType.Int).Value = displayOrder;
                    if (recordId > 0) command.Parameters.Add("@ID", SqlDbType.Int).Value = recordId;
                    connection.Open(); command.ExecuteNonQuery();
                }
                string message = recordId > 0 ? "The class-subject assignment was updated." : "The subject was assigned to the class.";
                ClearEditor(); BindGrid(); BindStats(); ShowMessage(message, true);
            });
        }

        protected void gvClassSubjects_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id; if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id)) return;
            TryAction(delegate { if (e.CommandName == "EditAssignment") LoadEditor(id); else if (e.CommandName == "DeleteAssignment") DeleteAssignment(id); });
        }

        private void EnsureSchema()
        {
            using (SqlConnection connection = new SqlConnection(_connStr))
            using (SqlCommand command = new SqlCommand(File.ReadAllText(Server.MapPath("~/ClassSubjects_Database_Update.sql")), connection))
            { command.CommandTimeout = 120; connection.Open(); command.ExecuteNonQuery(); }
        }

        private void BindClasses()
        {
            DataTable table = Fill("SELECT ClassID,ClassName FROM dbo.Classes ORDER BY ClassName;");
            BindList(ddlClass, table, "ClassName", "ClassID", "Select a class", ""); BindList(ddlFilterClass, table, "ClassName", "ClassID", "All classes", "");
        }
        private void BindSubjects() { BindList(ddlSubject, Fill("SELECT SubjectID,SubjectName FROM dbo.Subjects ORDER BY SubjectName;"), "SubjectName", "SubjectID", "Select a subject", ""); }
        private void BindGrid()
        {
            int classId; int.TryParse(ddlFilterClass.SelectedValue, out classId);
            gvClassSubjects.DataSource = Fill(@"SELECT cs.ClassSubjectID,cs.ClassID,c.ClassName,cs.SubjectID,s.SubjectName,cs.SubjectGroup,cs.OptionGroupCode,
COALESCE(cs.OptionGroupName,CASE cs.OptionGroupCode WHEN N'FAITH' THEN N'Faith / Religion Choice' WHEN N'GROUP2' THEN N'Group 2 - Arabic / Computer Science' WHEN N'GROUP3' THEN N'Group 3 - Skills / Humanities' END) AS OptionGroupName,
cs.ReligionEligibility,cs.IsStudentSelectable,cs.DisplayOrder FROM dbo.ClassSubjects cs INNER JOIN dbo.Classes c ON c.ClassID=cs.ClassID INNER JOIN dbo.Subjects s ON s.SubjectID=cs.SubjectID
WHERE @ClassID=0 OR cs.ClassID=@ClassID ORDER BY c.ClassName,cs.SubjectGroup,cs.DisplayOrder,s.SubjectName;", new SqlParameter("@ClassID", SqlDbType.Int) { Value = classId });
            gvClassSubjects.DataBind();
        }
        private void BindStats()
        {
            DataRow row = Fill(@"SELECT (SELECT COUNT(*) FROM dbo.Classes) AS Classes,(SELECT COUNT(*) FROM dbo.ClassSubjects) AS Assignments,
(SELECT COUNT(*) FROM dbo.ClassSubjects WHERE SubjectGroup=N'Compulsory') AS Compulsory,
(SELECT COUNT(*) FROM dbo.ClassSubjects WHERE SubjectGroup=N'Optional' AND IsStudentSelectable=1) AS Choices;").Rows[0];
            lblTotalClasses.Text = E(row, "Classes"); lblTotalAssignments.Text = E(row, "Assignments"); lblCompulsoryCount.Text = E(row, "Compulsory"); lblChoiceCount.Text = E(row, "Choices");
        }

        private bool AssignmentExists(int classId, int subjectId, int excludeId)
        {
            DataTable table = Fill("SELECT COUNT(*) AS CountValue FROM dbo.ClassSubjects WHERE ClassID=@ClassID AND SubjectID=@SubjectID AND ClassSubjectID<>@ID;",
                new SqlParameter("@ClassID", SqlDbType.Int) { Value = classId }, new SqlParameter("@SubjectID", SqlDbType.Int) { Value = subjectId }, new SqlParameter("@ID", SqlDbType.Int) { Value = excludeId });
            return Convert.ToInt32(table.Rows[0]["CountValue"], CultureInfo.InvariantCulture) > 0;
        }

        private void LoadEditor(int id)
        {
            DataTable table = Fill("SELECT * FROM dbo.ClassSubjects WHERE ClassSubjectID=@ID;", new SqlParameter("@ID", SqlDbType.Int) { Value = id });
            if (table.Rows.Count == 0) throw new InvalidOperationException("The selected assignment was not found.");
            DataRow row = table.Rows[0]; hidRecordID.Value = id.ToString(CultureInfo.InvariantCulture); lblEditorTitle.Text = "Edit class-subject assignment";
            Select(ddlClass, E(row, "ClassID")); Select(ddlSubject, E(row, "SubjectID")); Select(ddlSubjectGroup, E(row, "SubjectGroup")); Select(ddlReligionEligibility, E(row, "ReligionEligibility"));
            string groupCode = E(row, "OptionGroupCode"); if (groupCode == "FAITH" || groupCode == "GROUP2" || groupCode == "GROUP3") Select(ddlOptionGroup, groupCode); else if (!string.IsNullOrEmpty(groupCode)) { Select(ddlOptionGroup, "CUSTOM"); txtCustomGroupName.Text = E(row, "OptionGroupName"); } else ddlOptionGroup.SelectedIndex = 0;
            chkStudentSelectable.Checked = Convert.ToBoolean(row["IsStudentSelectable"], CultureInfo.InvariantCulture); txtDisplayOrder.Text = E(row, "DisplayOrder"); btnSave.Text = "Update Assignment"; btnCancel.Visible = true;
        }
        private void DeleteAssignment(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connStr)) using (SqlCommand command = new SqlCommand("DELETE dbo.ClassSubjects WHERE ClassSubjectID=@ID;", connection))
            { command.Parameters.Add("@ID", SqlDbType.Int).Value = id; connection.Open(); if (command.ExecuteNonQuery() == 0) throw new InvalidOperationException("The selected assignment was not found."); }
            ClearEditor(); BindGrid(); BindStats(); ShowMessage("The class-subject assignment was deleted. Existing student selection history was retained.", true);
        }

        private DataTable Fill(string sql, params SqlParameter[] parameters) { DataTable table = new DataTable(); using (SqlConnection connection = new SqlConnection(_connStr)) using (SqlCommand command = new SqlCommand(sql, connection)) using (SqlDataAdapter adapter = new SqlDataAdapter(command)) { if (parameters != null) command.Parameters.AddRange(parameters); adapter.Fill(table); } return table; }
        private static void BindList(ListControl control, DataTable table, string text, string value, string prompt, string promptValue) { control.Items.Clear(); control.DataSource = table; control.DataTextField = text; control.DataValueField = value; control.DataBind(); control.Items.Insert(0, new ListItem(prompt, promptValue)); }
        private void ClearEditor() { hidRecordID.Value = string.Empty; lblEditorTitle.Text = "Assign a subject to a class"; btnSave.Text = "Save Assignment"; btnCancel.Visible = false; ddlClass.SelectedIndex = ddlSubject.SelectedIndex = ddlSubjectGroup.SelectedIndex = ddlOptionGroup.SelectedIndex = ddlReligionEligibility.SelectedIndex = 0; txtCustomGroupName.Text = string.Empty; txtDisplayOrder.Text = "0"; chkStudentSelectable.Checked = false; }
        private void TryAction(Action action) { try { action(); } catch (Exception ex) { ShowMessage(ex.Message, false); } }
        private void ShowMessage(string message, bool success) { pnlMessage.Visible = true; pnlMessage.CssClass = "module-message " + (success ? "success" : "error"); lblMessage.Text = Server.HtmlEncode(message); }
        private static string Required(string value, string label, int max) { value = (value ?? string.Empty).Trim(); if (value.Length == 0) throw new InvalidOperationException("Enter the " + label + "."); if (value.Length > max) throw new InvalidOperationException("The " + label + " is too long."); return value; }
        private static string NormalizeCode(string value) { string code = Regex.Replace((value ?? string.Empty).ToUpperInvariant(), "[^A-Z0-9]+", "_").Trim('_'); if (code.Length > 23) code = code.Substring(0, 23); if (code.Length == 0) throw new InvalidOperationException("Enter a valid custom choice-group name."); return code; }
        private static string GroupName(string code) { return code == "FAITH" ? "Faith / Religion Choice" : code == "GROUP2" ? "Group 2 - Arabic / Computer Science" : "Group 3 - Skills / Humanities"; }
        private static string E(DataRow row, string column) { return row[column] == DBNull.Value ? string.Empty : Convert.ToString(row[column], CultureInfo.InvariantCulture); }
        private static void Select(ListControl control, string value) { ListItem item = control.Items.FindByValue(value); if (item != null) { control.ClearSelection(); item.Selected = true; } }
    }
}
