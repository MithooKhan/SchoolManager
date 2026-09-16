using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class DaakDiary : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            ClearEntryForm();
            try { OfficeDaakService.EnsureSchema(); OfficeDaakAttachmentService.EnsureSchema(); BindSummary(); BindDiaryRegister(); }
            catch (Exception ex) { ShowMessage("The incoming Daak register could not be prepared. Run OfficeDaak_Database_Update.sql if the database user cannot create tables. Details: " + ex.Message, "error"); }
        }

        protected void btnSaveDiary_Click(object sender, EventArgs e)
        {
            Page.Validate("DiaryEntry"); if (!Page.IsValid) { ShowMessage("Complete the required Daak details before saving.", "error"); return; }
            DateTime receivedDate; if (!TryParseRequiredDate(txtReceivedDate.Text, out receivedDate)) { ShowMessage("Enter a valid received date.", "error"); return; }
            DateTime? letterDate = ParseOptionalDate(txtLetterDate.Text), dueDate = ParseOptionalDate(txtActionDueDate.Text);
            if (!string.IsNullOrWhiteSpace(txtLetterDate.Text) && !letterDate.HasValue) { ShowMessage("Enter a valid letter date.", "error"); return; }
            if (!string.IsNullOrWhiteSpace(txtActionDueDate.Text) && !dueDate.HasValue) { ShowMessage("Enter a valid action due date.", "error"); return; }
            try
            {
                OfficeDaakService.EnsureSchema(); OfficeDaakAttachmentService.EnsureSchema(); var files = OfficeDaakAttachmentService.ReadUploads(fuDaakDocument);
                DaakDiaryRecord record = new DaakDiaryRecord { ReceivedDate = receivedDate, LetterDate = letterDate, SenderOffice = txtSenderOffice.Text, SenderReferenceNo = txtSenderReferenceNo.Text, Subject = txtSubject.Text, Description = txtDescription.Text, Category = ddlCategory.SelectedValue, Priority = ddlPriority.SelectedValue, DeliveryMode = ddlDeliveryMode.SelectedValue, AssignedTo = txtAssignedTo.Text, ActionDueDate = dueDate, Status = ddlStatus.SelectedValue, Remarks = txtRemarks.Text, Document = null, CreatedByUserID = GetCurrentUserId() };
                int id; int.TryParse(hfDiaryID.Value, out id); bool updated = id > 0; string diaryNo;
                if (updated) diaryNo = OfficeDaakAttachmentService.UpdateDiary(id, record); else { diaryNo = OfficeDaakService.CreateDiary(record); id = OfficeDaakAttachmentService.GetRecordId("Incoming", diaryNo); }
                OfficeDaakAttachmentService.AddDocuments("Incoming", id, files, GetCurrentUserId());
                ClearEntryForm(); lblSavedDiaryNo.Text = diaryNo; lblSavedReceivedDate.Text = receivedDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture); pnlSavedStamp.Visible = true; BindSummary(); gvDiary.PageIndex = 0; BindDiaryRegister(); ShowMessage((updated ? "Incoming Daak updated: " : "Incoming Daak saved: ") + diaryNo + ". " + files.Count.ToString(CultureInfo.InvariantCulture) + " new page(s) archived.", "success");
            }
            catch (Exception ex) { ShowMessage("The incoming Daak could not be saved. " + ex.Message, "error"); }
        }

        protected void btnClearDiary_Click(object sender, EventArgs e) { ClearEntryForm(); pnlSavedStamp.Visible = false; HideMessage(); }
        protected void btnSearch_Click(object sender, EventArgs e) { gvDiary.PageIndex = 0; BindDiaryRegister(); }
        protected void btnResetSearch_Click(object sender, EventArgs e) { txtSearch.Text = txtFromDate.Text = txtToDate.Text = string.Empty; ddlFilterStatus.SelectedIndex = 0; gvDiary.PageIndex = 0; BindDiaryRegister(); HideMessage(); }
        protected void gvDiary_PageIndexChanging(object sender, GridViewPageEventArgs e) { gvDiary.PageIndex = e.NewPageIndex; BindDiaryRegister(); }
        protected void gvDiary_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "EditRecord") return; int id; if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id)) { ShowMessage("The selected Daak record is invalid.", "error"); return; }
            try { LoadRecord(id); ShowMessage("The diary entry is ready to edit. Existing pages remain stored; selected files will be added.", "success"); }
            catch (Exception ex) { ShowMessage(ex.Message, "error"); }
        }

        private void LoadRecord(int id)
        {
            DataRow r = OfficeDaakAttachmentService.GetRecord("Incoming", id); hfDiaryID.Value = id.ToString(CultureInfo.InvariantCulture); txtReceivedDate.Text = InputDate(r["ReceivedDate"]); txtLetterDate.Text = InputDate(r["LetterDate"]); txtSenderOffice.Text = S(r, "SenderOffice"); txtSenderReferenceNo.Text = S(r, "SenderReferenceNo"); txtSubject.Text = S(r, "Subject"); txtDescription.Text = S(r, "Description"); Set(ddlCategory, S(r, "Category")); Set(ddlPriority, S(r, "Priority")); Set(ddlDeliveryMode, S(r, "DeliveryMode")); txtAssignedTo.Text = S(r, "AssignedTo"); txtActionDueDate.Text = InputDate(r["ActionDueDate"]); Set(ddlStatus, S(r, "Status")); txtRemarks.Text = S(r, "Remarks"); btnSaveDiary.Text = "Update Diary Entry and Add Pages"; pnlSavedStamp.Visible = false;
        }
        private void BindDiaryRegister()
        {
            try { DateTime? from = ParseOptionalDate(txtFromDate.Text), to = ParseOptionalDate(txtToDate.Text); if ((!string.IsNullOrWhiteSpace(txtFromDate.Text) && !from.HasValue) || (!string.IsNullOrWhiteSpace(txtToDate.Text) && !to.HasValue)) throw new InvalidOperationException("Enter valid register dates."); if (from.HasValue && to.HasValue && from.Value > to.Value) throw new InvalidOperationException("The from date cannot be later than the to date."); gvDiary.DataSource = OfficeDaakService.GetDiaryRecords(txtSearch.Text, ddlFilterStatus.SelectedValue, from, to); gvDiary.DataBind(); } catch (Exception ex) { ShowMessage("Incoming Daak records could not be loaded. " + ex.Message, "error"); }
        }
        private void BindSummary() { DaakRegisterStats s = OfficeDaakService.GetDiaryStats(); lblTotalRecords.Text = s.TotalRecords.ToString("N0", CultureInfo.InvariantCulture); lblTodayRecords.Text = s.TodayRecords.ToString("N0", CultureInfo.InvariantCulture); lblActiveRecords.Text = s.ActiveRecords.ToString("N0", CultureInfo.InvariantCulture); lblFiledCopies.Text = s.FiledCopies.ToString("N0", CultureInfo.InvariantCulture); }
        private void ClearEntryForm() { hfDiaryID.Value = "0"; txtReceivedDate.Text = txtLetterDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); txtSenderOffice.Text = txtSenderReferenceNo.Text = txtSubject.Text = txtDescription.Text = txtAssignedTo.Text = txtActionDueDate.Text = txtRemarks.Text = string.Empty; Set(ddlCategory, "General"); Set(ddlPriority, "Normal"); Set(ddlDeliveryMode, "By Hand"); Set(ddlStatus, "Received"); btnSaveDiary.Text = "Save and Assign Diary Number"; }
        private int? GetCurrentUserId() { int id; return Session != null && int.TryParse(Convert.ToString(Session["SystemUserID"], CultureInfo.InvariantCulture), out id) ? (int?)id : null; }
        private static bool TryParseRequiredDate(string value, out DateTime date) { return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date); }
        private static DateTime? ParseOptionalDate(string value) { if (string.IsNullOrWhiteSpace(value)) return null; DateTime date; return TryParseRequiredDate(value, out date) ? (DateTime?)date.Date : null; }
        private static string InputDate(object value) { return value == null || value == DBNull.Value ? string.Empty : Convert.ToDateTime(value, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }
        private static string S(DataRow r, string c) { return r[c] == DBNull.Value ? string.Empty : Convert.ToString(r[c], CultureInfo.InvariantCulture); }
        private static void Set(ListControl list, string value) { ListItem item = list.Items.FindByValue(value); if (item != null) list.SelectedValue = value; }
        private void ShowMessage(string message, string type) { pnlMessage.Visible = true; pnlMessage.CssClass = "office-daak-message office-daak-message-" + type; lblMessage.Text = Server.HtmlEncode(message); }
        private void HideMessage() { pnlMessage.Visible = false; lblMessage.Text = string.Empty; }
    }
}
