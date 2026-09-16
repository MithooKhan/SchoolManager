using System;
using System.IO;
using System.Web;

namespace DigitalSchoolManager
{
    public class Global : HttpApplication
    {
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Request == null)
            {
                return;
            }

            string extension = Path.GetExtension(context.Request.CurrentExecutionFilePath);
            if (!string.Equals(extension, ".aspx", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string pageName = Path.GetFileName(context.Request.CurrentExecutionFilePath);
            if (IsPublicPage(pageName))
            {
                return;
            }

            if (SystemUserSecurity.IsAuthenticated(context))
            {
                if (PortalAuthorizationService.CanAccessPage(context, pageName))
                    return;

                context.Response.Redirect(
                    VirtualPathUtility.ToAbsolute(PortalAuthorizationService.RoleHome(context)) + "?denied=1",
                    true);
                return;
            }

            string returnUrl = context.Request.AppRelativeCurrentExecutionFilePath;
            if (!string.IsNullOrEmpty(context.Request.Url.Query))
            {
                returnUrl += context.Request.Url.Query;
            }

            string loginUrl = VirtualPathUtility.ToAbsolute("~/PortalLogin.aspx") +
                              "?reason=login&ReturnUrl=" +
                              HttpUtility.UrlEncode(returnUrl);
            context.Response.Redirect(loginUrl, true);
        }

        private static bool IsPublicPage(string pageName)
        {
            return string.Equals(pageName, "PublicDashboard.aspx", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(pageName, "Login.aspx", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(pageName, "PortalLogin.aspx", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(pageName, "StudentProfile.aspx", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(pageName, "StudentUnionVoting.aspx", StringComparison.OrdinalIgnoreCase);
        }
    }
}
