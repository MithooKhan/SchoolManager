using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class PublicContentManagement : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SystemUserSecurity.IsAdministrator(Context)) { Response.Redirect(ResolveUrl("~/PortalLogin.aspx?ReturnUrl=%2fPublicContentManagement.aspx"), true); return; }
            if (IsPostBack) return;
            TryAction(delegate { PublicDashboardService.EnsureSchema(); BindTeachers(); LoadProfile(); BindAnnouncements(); txtAnnouncementDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); });
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            TryAction(delegate { int teacherId; int? selected = int.TryParse(ddlPrincipalTeacher.SelectedValue, out teacherId) ? (int?)teacherId : null; PublicDashboardService.SaveProfile(selected, txtPrincipalName.Text, txtPrincipalDesignation.Text, txtPrincipalMessage.Text, txtSchoolIntroduction.Text, txtSchoolHistory.Text, CurrentUserId()); ShowMessage("The public school profile was updated.", true); });
        }

        protected void btnNewAnnouncement_Click(object sender, EventArgs e) { ClearAnnouncement(); }
        protected void btnSearch_Click(object sender, EventArgs e) { TryAction(delegate { BindAnnouncements(); }); }

        protected void btnSaveAnnouncement_Click(object sender, EventArgs e)
        {
            TryAction(delegate
            {
                int id; int.TryParse(hidAnnouncementID.Value, out id);
                ProcessedImage image = fuAnnouncementImage.HasFile ? ImageUploadProcessor.ReadAndOptimize(fuAnnouncementImage, 1600, 1000) : null;
                SchoolAnnouncementInput input = new SchoolAnnouncementInput { AnnouncementID = id, Title = txtAnnouncementTitle.Text, AnnouncementCategory = ddlAnnouncementCategory.SelectedValue,
                    Summary = txtAnnouncementSummary.Text, AnnouncementBody = txtAnnouncementBody.Text, EventDate = DateValue(txtAnnouncementDate.Text), IsPublished = chkPublished.Checked, IsPinned = chkPinned.Checked, UserID = CurrentUserId(),
                    MediaType = ddlMediaType.SelectedValue, MediaUrl = txtMediaUrl.Text, MediaTitle = txtMediaTitle.Text };
                int savedId = PublicDashboardService.SaveAnnouncement(input, image); ClearAnnouncement(); BindAnnouncements(); ShowMessage("Announcement #" + savedId.ToString(CultureInfo.InvariantCulture) + " was saved successfully.", true);
            });
        }

        protected void gvAnnouncements_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            TryAction(delegate
            {
                if (e.CommandName == "EditAnnouncement") { int id = Convert.ToInt32(e.CommandArgument, CultureInfo.InvariantCulture); LoadAnnouncement(id); }
                else if (e.CommandName == "ToggleAnnouncement") { string[] parts = Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture).Split('|'); int id = int.Parse(parts[0], CultureInfo.InvariantCulture); bool current = bool.Parse(parts[1]); PublicDashboardService.SetAnnouncementPublished(id, !current); BindAnnouncements(); ShowMessage(!current ? "The announcement is now public." : "The announcement was removed from the public dashboard.", true); }
            });
        }

        private void BindTeachers()
        {
            DataTable table = PublicDashboardService.GetTeacherOptions(); ddlPrincipalTeacher.DataSource = table; ddlPrincipalTeacher.DataTextField = "DisplayName"; ddlPrincipalTeacher.DataValueField = "teacherid"; ddlPrincipalTeacher.DataBind(); ddlPrincipalTeacher.Items.Insert(0, new ListItem("Select school head / principal", ""));
        }

        private void LoadProfile()
        {
            DataRow row = PublicDashboardService.GetProfile(); string teacherId = E(row, "PrincipalTeacherID"); ListItem item = ddlPrincipalTeacher.Items.FindByValue(teacherId); if (item != null) { ddlPrincipalTeacher.ClearSelection(); item.Selected = true; }
            txtPrincipalName.Text = E(row, "PrincipalDisplayName"); txtPrincipalDesignation.Text = E(row, "PrincipalDesignation"); txtPrincipalMessage.Text = E(row, "PrincipalMessage"); txtSchoolIntroduction.Text = E(row, "SchoolIntroduction"); txtSchoolHistory.Text = E(row, "SchoolHistory");
        }

        private void BindAnnouncements() { gvAnnouncements.DataSource = PublicDashboardService.SearchAnnouncements(txtSearch.Text); gvAnnouncements.DataBind(); }
        private void LoadAnnouncement(int id)
        {
            DataRow row = PublicDashboardService.GetAnnouncement(id); hidAnnouncementID.Value = id.ToString(CultureInfo.InvariantCulture); lblAnnouncementEditorTitle.Text = "Edit announcement #" + id.ToString(CultureInfo.InvariantCulture);
            txtAnnouncementTitle.Text = E(row, "Title"); Select(ddlAnnouncementCategory, E(row, "AnnouncementCategory")); txtAnnouncementSummary.Text = E(row, "Summary"); txtAnnouncementBody.Text = E(row, "AnnouncementBody"); txtAnnouncementDate.Text = InputDate(row["EventDate"]);
            Select(ddlMediaType, E(row, "MediaType")); txtMediaUrl.Text = E(row, "MediaUrl"); txtMediaTitle.Text = E(row, "MediaTitle");
            chkPublished.Checked = Convert.ToBoolean(row["IsPublished"], CultureInfo.InvariantCulture); chkPinned.Checked = Convert.ToBoolean(row["IsPinned"], CultureInfo.InvariantCulture); btnSaveAnnouncement.Text = "Update Announcement";
        }
        private void ClearAnnouncement() { hidAnnouncementID.Value = string.Empty; lblAnnouncementEditorTitle.Text = "Create announcement"; btnSaveAnnouncement.Text = "Save Announcement"; txtAnnouncementTitle.Text = txtAnnouncementSummary.Text = txtAnnouncementBody.Text = txtMediaUrl.Text = txtMediaTitle.Text = string.Empty; txtAnnouncementDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); ddlAnnouncementCategory.SelectedIndex = ddlMediaType.SelectedIndex = 0; chkPublished.Checked = chkPinned.Checked = false; }
        private void TryAction(Action action) { try { action(); } catch (Exception ex) { ShowMessage(ex.Message, false); } }
        private void ShowMessage(string message, bool success) { pnlMessage.Visible = true; pnlMessage.CssClass = "module-message " + (success ? "success" : "error"); lblMessage.Text = Server.HtmlEncode(message); }
        private int? CurrentUserId() { int id; return int.TryParse(Convert.ToString(Session["SystemUserID"], CultureInfo.InvariantCulture), out id) ? (int?)id : null; }
        private static DateTime? DateValue(string value) { if (string.IsNullOrWhiteSpace(value)) return null; DateTime date; if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) throw new InvalidOperationException("Enter a valid announcement date."); return date.Date; }
        private static string E(DataRow row, string column) { return row[column] == DBNull.Value ? string.Empty : Convert.ToString(row[column], CultureInfo.InvariantCulture); }
        private static string InputDate(object value) { return value == DBNull.Value ? string.Empty : Convert.ToDateTime(value, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }
        private static void Select(ListControl control, string value) { ListItem item = control.Items.FindByValue(value); if (item != null) { control.ClearSelection(); item.Selected = true; } }
    }
}
