using System;
using System.Globalization;
using System.Web;
using System.Web.SessionState;

namespace DigitalSchoolManager
{
    public class FundDocumentHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            if (!SystemUserSecurity.IsAuthenticated(context))
            {
                WriteError(context, 401, "Your login session has expired. Sign in to view this protected accounting image.");
                return;
            }

            int documentId;
            if (!int.TryParse(context.Request.QueryString["id"], out documentId) || documentId <= 0)
            {
                WriteError(context, 400, "The requested evidence reference is invalid.");
                return;
            }

            try
            {
                FundDocumentData document = FundManagementService.GetDocument(documentId);
                if (document == null || document.Data == null || document.Data.Length == 0)
                {
                    WriteError(context, 404, "The saved evidence image could not be found.");
                    return;
                }

                string fileName = (document.FileName ?? "fund-evidence.jpg").Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\"", string.Empty);
                context.Response.Clear();
                context.Response.Buffer = true;
                context.Response.ContentType = string.IsNullOrWhiteSpace(document.ContentType) ? "image/jpeg" : document.ContentType;
                context.Response.AddHeader("X-Content-Type-Options", "nosniff");
                context.Response.AddHeader("Content-Disposition", "inline; filename=\"" + fileName + "\"");
                context.Response.AddHeader("Content-Length", document.Data.LongLength.ToString(CultureInfo.InvariantCulture));
                context.Response.Cache.SetCacheability(HttpCacheability.Private);
                context.Response.Cache.SetMaxAge(TimeSpan.FromMinutes(10));
                context.Response.OutputStream.Write(document.Data, 0, document.Data.Length);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[FundDocument] {0}", ex.Message);
                WriteError(context, 500, "The protected accounting image could not be opened.");
            }
        }

        private static void WriteError(HttpContext context, int statusCode, string message)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.ContentType = "text/plain";
            context.Response.Write(message);
        }
    }
}
