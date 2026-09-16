using System;
using System.Globalization;
using System.Web;

namespace DigitalSchoolManager
{
    public class DaakAttachment : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (!SystemUserSecurity.IsAuthenticated(context)) { context.Response.StatusCode = 401; return; }
            int id; if (!int.TryParse(context.Request.QueryString["id"], out id) || id <= 0) { context.Response.StatusCode = 400; return; }
            DaakAttachmentUpload file = OfficeDaakAttachmentService.GetDocument(id); if (file == null) { context.Response.StatusCode = 404; return; }
            string disposition = context.Request.QueryString["download"] == "1" ? "attachment" : "inline"; string name = (file.FileName ?? "daak-document").Replace("\"", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
            context.Response.Clear(); context.Response.ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType; context.Response.AddHeader("Content-Disposition", disposition + "; filename=\"" + name + "\""); context.Response.AddHeader("Content-Length", file.Data.Length.ToString(CultureInfo.InvariantCulture)); context.Response.BinaryWrite(file.Data); context.ApplicationInstance.CompleteRequest();
        }
        public bool IsReusable { get { return false; } }
    }
}
