using System;
using System.Globalization;
using System.Web;
using System.Web.SessionState;

namespace DigitalSchoolManager
{
    public class DaakDocumentHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!SystemUserSecurity.IsAuthenticated(context))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "text/plain";
                context.Response.Write("Your login session has expired. Sign in to view this protected Daak document.");
                return;
            }

            int recordId;
            string recordType = (context.Request.QueryString["type"] ?? string.Empty).Trim();
            if (!int.TryParse(context.Request.QueryString["id"], out recordId) || recordId <= 0 ||
                (!string.Equals(recordType, "incoming", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(recordType, "outgoing", StringComparison.OrdinalIgnoreCase)))
            {
                WriteError(context, 400, "The requested Daak document reference is invalid.");
                return;
            }

            try
            {
                DaakDocument document = string.Equals(recordType, "incoming", StringComparison.OrdinalIgnoreCase)
                    ? OfficeDaakService.GetDiaryDocument(recordId)
                    : OfficeDaakService.GetDispatchDocument(recordId);

                if (document == null || document.Data == null || document.Data.Length == 0)
                {
                    WriteError(context, 404, "The saved Daak document could not be found.");
                    return;
                }

                string fileName = (document.OriginalFileName ?? "DaakDocument")
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty)
                    .Replace("\"", string.Empty);

                context.Response.Clear();
                context.Response.Buffer = true;
                context.Response.ContentType = string.IsNullOrWhiteSpace(document.ContentType)
                    ? "application/octet-stream"
                    : document.ContentType;
                context.Response.AddHeader("X-Content-Type-Options", "nosniff");
                context.Response.AddHeader("Content-Disposition", "inline; filename=\"" + fileName + "\"");
                context.Response.AddHeader(
                    "Content-Length",
                    document.Data.LongLength.ToString(CultureInfo.InvariantCulture));
                context.Response.OutputStream.Write(document.Data, 0, document.Data.Length);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Daak document error: " + ex.Message);
                WriteError(context, 500, "The protected Daak document could not be opened.");
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
