using System;
using System.IO;
using System.Web;

namespace DigitalSchoolManager
{
    public sealed class PublicSchoolImage : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                PublicImageData image = null;
                int announcementId;
                if (int.TryParse(context.Request.QueryString["announcement"], out announcementId)) image = PublicDashboardService.GetAnnouncementImage(announcementId);
                else if (string.Equals(context.Request.QueryString["principal"], "1", StringComparison.Ordinal)) image = PublicDashboardService.GetPrincipalImage();
                if (image != null && image.Data != null && image.Data.Length > 0)
                {
                    context.Response.ContentType = image.ContentType; context.Response.Cache.SetCacheability(HttpCacheability.Public); context.Response.Cache.SetMaxAge(TimeSpan.FromMinutes(15)); context.Response.BinaryWrite(image.Data); return;
                }
            }
            catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("[PublicSchoolImage] " + ex.Message); }
            string fallback = context.Server.MapPath("~/images/SchoolLogo.png");
            context.Response.ContentType = "image/png"; if (File.Exists(fallback)) context.Response.WriteFile(fallback); else context.Response.StatusCode = 404;
        }
        public bool IsReusable { get { return false; } }
    }
}
