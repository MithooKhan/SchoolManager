using System;
using System.Data;
using System.Globalization;
using System.Web.UI;

namespace DigitalSchoolManager
{
    public partial class PublicDashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            pnlManageContent.Visible = SystemUserSecurity.IsAdministrator(Context);
            try
            {
                PublicDashboardService.EnsureSchema();
                BindProfile(); BindStats(); BindAnnouncements(); BindExams();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[PublicDashboard] " + ex.Message);
                pnlStatus.Visible = true; pnlStatus.CssClass = "module-message error";
                lblStatus.Text = "The public school information is temporarily unavailable. Please try again shortly.";
            }
        }

        protected string FormatAnnouncementDate(object eventDate, object publishedAt)
        {
            object value = eventDate != DBNull.Value ? eventDate : publishedAt;
            return value == null || value == DBNull.Value ? string.Empty : Convert.ToDateTime(value, CultureInfo.InvariantCulture).ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        protected string BuildAnnouncementMedia(object mediaType, object mediaUrl, object mediaTitle)
        {
            return PublicDashboardService.BuildPublicMediaMarkup(
                Convert.ToString(mediaType, CultureInfo.InvariantCulture),
                Convert.ToString(mediaUrl, CultureInfo.InvariantCulture),
                Convert.ToString(mediaTitle, CultureInfo.InvariantCulture));
        }

        private void BindProfile()
        {
            DataRow row = PublicDashboardService.GetProfile(); lblPrincipalName.Text = E(row, "PrincipalName"); lblPrincipalDesignation.Text = E(row, "EffectiveDesignation");
            lblPrincipalMessage.Text = E(row, "PrincipalMessage"); lblIntroduction.Text = E(row, "SchoolIntroduction"); lblHistory.Text = E(row, "SchoolHistory");
            imgPrincipal.ImageUrl = ResolveUrl("~/PublicSchoolImage.ashx?principal=1&v=" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
        }

        private void BindStats()
        {
            DataRow row = PublicDashboardService.GetPublicStats(); lblTotalStudents.Text = N(row["TotalStudents"]); lblTotalTeachers.Text = N(row["TotalTeachers"]); lblTotalClasses.Text = N(row["TotalClasses"]);
        }

        private void BindAnnouncements()
        {
            DataTable table = PublicDashboardService.GetPublishedAnnouncements(); rptAnnouncements.DataSource = table; rptAnnouncements.DataBind(); pnlNoAnnouncements.Visible = table.Rows.Count == 0;
        }

        private void BindExams()
        {
            DataTable table = PublicDashboardService.GetUpcomingExams(); rptUpcomingExams.DataSource = table; rptUpcomingExams.DataBind(); pnlNoUpcomingExams.Visible = table.Rows.Count == 0;
        }

        private static string E(DataRow row, string column) { return row[column] == DBNull.Value ? string.Empty : Convert.ToString(row[column], CultureInfo.InvariantCulture); }
        private static string N(object value) { return value == null || value == DBNull.Value ? "0" : Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString("N0", CultureInfo.InvariantCulture); }
    }
}
