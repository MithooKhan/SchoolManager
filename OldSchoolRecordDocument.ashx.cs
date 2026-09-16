using System;
using System.Globalization;
using System.Web;
using System.Web.SessionState;

namespace DigitalSchoolManager
{
    public class OldSchoolRecordDocument : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            if (!SystemUserSecurity.IsAdministrator(context)) { context.Response.StatusCode = 401; return; }
            int id; if (!int.TryParse(context.Request.QueryString["id"], out id)) { context.Response.StatusCode = 400; return; }
            ArchiveDocument document = OldSchoolRecordsService.GetDocument(id); if (document == null) { context.Response.StatusCode = 404; return; }
            bool download = string.Equals(context.Request.QueryString["download"], "1", StringComparison.Ordinal);
            string safeName = (document.FileName ?? "archive-document").Replace("\"", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
            context.Response.Clear(); context.Response.ContentType = document.ContentType; context.Response.AddHeader("Content-Length", document.Data.Length.ToString(CultureInfo.InvariantCulture));
            context.Response.AddHeader("Content-Disposition", (download ? "attachment" : "inline") + "; filename=\"" + safeName + "\""); context.Response.BinaryWrite(document.Data); context.Response.End();
        }
        public bool IsReusable { get { return false; } }
    }
}
