using System;
using System.Web.UI;

namespace DigitalSchoolManager
{
    public partial class PortalLogin : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            if (PortalAuthorizationService.IsTeacher(Context))
            {
                Response.Redirect(ResolveUrl("~/TeacherDashboard.aspx"), true);
                return;
            }
            if (SystemUserSecurity.IsAdministrator(Context))
            {
                Response.Redirect(ResolveUrl("~/AdminDashboard.aspx"), true);
                return;
            }
            if (Request.QueryString["reason"] == "denied")
                ShowMessage("Please sign in with an authorized teacher account to open that page.", false);
            else if (Request.QueryString["reason"] == "login")
                ShowMessage("Sign in to open the selected school-management page.", false);
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                PortalLoginResult result = PortalIdentityService.AuthenticateTeacher(
                    txtCellNumber.Text, txtPersonalNumber.Text);
                if (!result.Success)
                {
                    ShowMessage(result.Message, false);
                    txtPersonalNumber.Text = string.Empty;
                    return;
                }
                PortalIdentityService.EstablishTeacherSession(Context, result, chkRemember.Checked);
                string returnUrl = Request.QueryString["ReturnUrl"];
                if (string.IsNullOrWhiteSpace(returnUrl) || !UrlIsLocal(returnUrl))
                    returnUrl = ResolveUrl("~/TeacherDashboard.aspx");
                else if (returnUrl.StartsWith("~/", StringComparison.Ordinal))
                    returnUrl = ResolveUrl(returnUrl);

                Response.Redirect(returnUrl, false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            catch (Exception ex)
            {
                ShowMessage("Teacher login could not be completed. Run Portal_Database_Update.sql if required. " + ex.Message, false);
            }
        }

        private bool UrlIsLocal(string value)
        {
            return value.StartsWith("~/", StringComparison.Ordinal) ||
                   (value.StartsWith("/", StringComparison.Ordinal) &&
                   !value.StartsWith("//", StringComparison.Ordinal) &&
                   !value.StartsWith("/\\", StringComparison.Ordinal));
        }

        private void ShowMessage(string message, bool success)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = success ? "portal-message is-success" : "portal-message is-error";
            lblMessage.Text = message;
        }
    }
}
