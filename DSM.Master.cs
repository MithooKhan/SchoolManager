using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class DSM : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool isAuthenticated = SystemUserSecurity.IsAuthenticated(Context);
            bool isTeacher = PortalAuthorizationService.IsTeacher(Context);
            pnlGuestSession.Visible = !isAuthenticated;
            pnlUserSession.Visible = isAuthenticated;
            MenuHome.Visible = !isTeacher;
            MenuTeacher.Visible = isTeacher;

            if (isAuthenticated)
            {
                lblSignedInUser.Text = Convert.ToString(Session["DisplayName"]);
                lblSignedInRole.Text = Convert.ToString(Session["UserRole"]);
            }
        }

        protected void MenuHome_MenuItemClick(object sender, MenuEventArgs e)
        {
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            SystemUserSecurity.SignOut(Context);
            Response.Redirect(ResolveUrl("~/PortalLogin.aspx"), true);
        }

        protected string GetPageTitle()
        {
            return string.IsNullOrWhiteSpace(Page.Title)
                ? "Digital School Manager"
                : Page.Title + " | Digital School Manager";
        }

        protected string GetHomeUrl()
        {
            if (PortalAuthorizationService.IsTeacher(Context))
                return "~/TeacherDashboard.aspx";
            return SystemUserSecurity.IsAdministrator(Context)
                ? "~/AdminDashboard.aspx"
                : "~/PublicDashboard.aspx";
        }

        protected string GetBodyCssClass()
        {
            string fileName = Path.GetFileNameWithoutExtension(Request.AppRelativeCurrentExecutionFilePath) ?? "page";
            string slug = Regex.Replace(fileName.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
            return "school-body page-" + (string.IsNullOrEmpty(slug) ? "default" : slug);
        }
    }
}
