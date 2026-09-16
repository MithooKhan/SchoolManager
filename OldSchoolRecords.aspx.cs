using System;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class OldSchoolRecords : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SystemUserSecurity.IsAdministrator(Context)) { Response.Redirect(ResolveUrl("~/Login.aspx?ReturnUrl=%2fOldSchoolRecords.aspx"), true); return; }
            if (IsPostBack) return;
            try { OldSchoolRecordsService.EnsureSchema(); BindRecords(); }
            catch (Exception ex) { ShowMessage("The digital record room could not be initialized. " + ex.Message, false); }
        }
        protected void btnNew_Click(object sender, EventArgs e) { ClearEditor(); }
        protected void btnSearch_Click(object sender, EventArgs e) { BindRecords(); }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int id; int.TryParse(hidRecordID.Value, out id);
                if (id == 0 && !fuDocuments.HasFile) throw new InvalidOperationException("Attach at least one scanned page or PDF when creating an archive record.");
                OldSchoolRecordInput input = new OldSchoolRecordInput { RecordID = id, Category = ddlCategory.SelectedValue, RecordTitle = txtTitle.Text,
                    RecordReference = txtReference.Text, RegisterYear = txtYear.Text, StartDate = DateValue(txtStartDate.Text), EndDate = DateValue(txtEndDate.Text),
                    PhysicalLocation = txtLocation.Text, ConfidentialityLevel = ddlConfidentiality.SelectedValue, Description = txtDescription.Text,
                    Keywords = txtKeywords.Text, UserID = CurrentUserId() };
                string archiveNumber = OldSchoolRecordsService.SaveRecord(input);
                if (fuDocuments.HasFile) OldSchoolRecordsService.AddDocuments(input.RecordID, fuDocuments.PostedFiles, CurrentUserId());
                ClearEditor(); BindRecords(); ShowMessage("Archive record " + archiveNumber + " was saved successfully.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }
        protected void gvRecords_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id; if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id)) return;
            try { if (e.CommandName == "EditRecord") LoadEditor(id); else if (e.CommandName == "OpenFiles") OpenDocuments(id); }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }
        protected void btnAddDocuments_Click(object sender, EventArgs e)
        {
            try { int id; if (!int.TryParse(hidDocumentRecordID.Value, out id)) throw new InvalidOperationException("Open an archive record first."); int count = OldSchoolRecordsService.AddDocuments(id, fuMoreDocuments.PostedFiles, CurrentUserId()); OpenDocuments(id); BindRecords(); ShowMessage(count + " document(s) were added to the archive record.", true); }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }
        protected void btnCloseDocuments_Click(object sender, EventArgs e) { pnlDocuments.Visible = false; }
        protected string FormatFileSize(object value) { long bytes = Convert.ToInt64(value, CultureInfo.InvariantCulture); return bytes >= 1048576 ? (bytes / 1048576d).ToString("N2", CultureInfo.InvariantCulture) + " MB" : (bytes / 1024d).ToString("N1", CultureInfo.InvariantCulture) + " KB"; }

        private void BindRecords() { gvRecords.DataSource = OldSchoolRecordsService.Search((txtSearch.Text ?? string.Empty).Trim(), ddlFilterCategory.SelectedValue); gvRecords.DataBind(); }
        private void LoadEditor(int id)
        {
            DataRow row = OldSchoolRecordsService.GetRecord(id); hidRecordID.Value = id.ToString(CultureInfo.InvariantCulture); lblEditorTitle.Text = "Edit " + E(row, "ArchiveNumber");
            ddlCategory.SelectedValue = E(row, "Category"); txtTitle.Text = E(row, "RecordTitle"); txtReference.Text = E(row, "RecordReference"); txtYear.Text = E(row, "RegisterYear");
            txtStartDate.Text = InputDate(row["RecordStartDate"]); txtEndDate.Text = InputDate(row["RecordEndDate"]); txtLocation.Text = E(row, "PhysicalLocation");
            ddlConfidentiality.SelectedValue = E(row, "ConfidentialityLevel"); txtDescription.Text = E(row, "Description"); txtKeywords.Text = E(row, "Keywords"); btnSave.Text = "Update Archive Record";
        }
        private void OpenDocuments(int id)
        {
            DataRow row = OldSchoolRecordsService.GetRecord(id); hidDocumentRecordID.Value = id.ToString(CultureInfo.InvariantCulture); lblSelectedArchive.Text = E(row, "ArchiveNumber"); lblSelectedTitle.Text = E(row, "RecordTitle");
            gvDocuments.DataSource = OldSchoolRecordsService.GetDocuments(id); gvDocuments.DataBind(); pnlDocuments.Visible = true;
        }
        private void ClearEditor() { hidRecordID.Value = string.Empty; lblEditorTitle.Text = "Create archive record"; ddlCategory.SelectedIndex = 0; txtTitle.Text = txtReference.Text = txtYear.Text = txtStartDate.Text = txtEndDate.Text = txtLocation.Text = txtDescription.Text = txtKeywords.Text = string.Empty; ddlConfidentiality.SelectedIndex = 0; btnSave.Text = "Save Archive Record"; }
        private static string E(DataRow row, string column) { return row[column] == DBNull.Value ? string.Empty : Convert.ToString(row[column], CultureInfo.InvariantCulture); }
        private static string InputDate(object value) { return value == DBNull.Value ? string.Empty : Convert.ToDateTime(value, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }
        private static DateTime? DateValue(string value) { if (string.IsNullOrWhiteSpace(value)) return null; DateTime date; if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) throw new InvalidOperationException("Enter valid archive dates."); return date.Date; }
        private int? CurrentUserId() { int id; return int.TryParse(Convert.ToString(Session["SystemUserID"], CultureInfo.InvariantCulture), out id) ? (int?)id : null; }
        private void ShowMessage(string message, bool success) { pnlMessage.Visible = true; pnlMessage.CssClass = "module-message " + (success ? "success" : "error"); lblMessage.Text = Server.HtmlEncode(message); }
    }
}
