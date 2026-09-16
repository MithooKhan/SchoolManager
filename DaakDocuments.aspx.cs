using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class DaakDocuments : Page
    {
        private string RecordType { get { return string.Equals(Request.QueryString["type"], "outgoing", StringComparison.OrdinalIgnoreCase) ? "Outgoing" : "Incoming"; } }
        private int RecordID { get { int id; return int.TryParse(Request.QueryString["id"], out id) ? id : 0; } }
        protected void Page_Load(object sender, EventArgs e) { if (!SystemUserSecurity.IsAdministrator(Context)) { Response.Redirect(ResolveUrl("~/Login.aspx"), true); return; } if (IsPostBack) return; try { OfficeDaakService.EnsureSchema(); OfficeDaakAttachmentService.EnsureSchema(); BindHeader(); BindDocuments(); } catch (Exception ex) { Show(ex.Message, false); } }
        protected void btnUpload_Click(object sender, EventArgs e) { try { if (RecordID <= 0) throw new InvalidOperationException("The Daak record is invalid."); var files = OfficeDaakAttachmentService.ReadUploads(fuDocuments); if (files.Count == 0) throw new InvalidOperationException("Select one or more scanned pages or a PDF file."); OfficeDaakAttachmentService.AddDocuments(RecordType, RecordID, files, CurrentUserId()); BindDocuments(); Show(files.Count.ToString(CultureInfo.InvariantCulture) + " document page(s) uploaded successfully.", true); } catch (Exception ex) { Show(ex.Message, false); } }
        protected void gvDocuments_RowCommand(object sender, GridViewCommandEventArgs e) { if (e.CommandName != "DeleteDocument") return; try { int id; if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id)) throw new InvalidOperationException("The selected document is invalid."); OfficeDaakAttachmentService.DeleteDocument(id); BindDocuments(); Show("The selected document page was deleted.", true); } catch (Exception ex) { Show(ex.Message, false); } }
        private void BindHeader() { if (RecordID <= 0) throw new InvalidOperationException("The Daak record is invalid."); DataRow r = OfficeDaakAttachmentService.GetRecord(RecordType, RecordID); lblRegisterTitle.Text = Server.HtmlEncode(Convert.ToString(r["RegisterNo"], CultureInfo.InvariantCulture) + " - Scanned Pages"); lblRecordSummary.Text = Server.HtmlEncode(Convert.ToString(r["Correspondent"], CultureInfo.InvariantCulture) + " | " + Convert.ToString(r["Subject"], CultureInfo.InvariantCulture)); }
        private void BindDocuments() { gvDocuments.DataSource = OfficeDaakAttachmentService.GetDocuments(RecordType, RecordID); gvDocuments.DataBind(); }
        protected string FormatSize(object value) { long size = Convert.ToInt64(value, CultureInfo.InvariantCulture); return size >= 1048576 ? (size / 1048576d).ToString("0.00", CultureInfo.InvariantCulture) + " MB" : (size / 1024d).ToString("0.0", CultureInfo.InvariantCulture) + " KB"; }
        private int? CurrentUserId() { int id; return int.TryParse(Convert.ToString(Session["SystemUserID"], CultureInfo.InvariantCulture), out id) ? (int?)id : null; }
        private void Show(string message, bool success) { pnlMessage.Visible = true; pnlMessage.CssClass = "daak-docs-message " + (success ? "success" : "error"); lblMessage.Text = Server.HtmlEncode(message); }
    }
}
