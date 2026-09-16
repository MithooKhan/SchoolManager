using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class LaboratoryRecords : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SystemUserSecurity.IsAdministrator(Context)) { Response.Redirect(ResolveUrl("~/PortalLogin.aspx?ReturnUrl=%2fLaboratoryRecords.aspx"), true); return; }
            if (IsPostBack) return;
            TryAction(delegate { SchoolResourcesService.EnsureSchema(); BindLabs(); });
        }

        protected void btnNewLab_Click(object sender, EventArgs e) { ClearLabEditor(); }
        protected void btnSearchLabs_Click(object sender, EventArgs e) { TryAction(delegate { BindLabs(); }); }
        protected void btnCloseEquipment_Click(object sender, EventArgs e) { pnlEquipment.Visible = false; }
        protected void btnNewEquipment_Click(object sender, EventArgs e) { ClearEquipmentEditor(); }
        protected void btnCloseDocuments_Click(object sender, EventArgs e) { pnlDocuments.Visible = false; }

        protected void btnSaveLab_Click(object sender, EventArgs e)
        {
            TryAction(delegate
            {
                int id; int.TryParse(hidLabID.Value, out id);
                SchoolLabInput input = new SchoolLabInput { LabID = id, LabName = txtLabName.Text, LabType = ddlLabType.SelectedValue,
                    InchargeName = txtLabIncharge.Text, LabLocation = txtLabLocation.Text, LabDescription = txtLabDescription.Text,
                    SafetyStatus = ddlSafetyStatus.SelectedValue, LastInspectionDate = DateValue(txtLabInspectionDate.Text, "inspection date"), IsActive = chkLabActive.Checked, UserID = CurrentUserId() };
                string code = SchoolResourcesService.SaveLab(input);
                int count = fuLabDocuments.HasFile ? SchoolResourcesService.AddDocuments("Lab", input.LabID, "Lab Evidence", fuLabDocuments.PostedFiles, CurrentUserId()) : 0;
                ClearLabEditor(); BindLabs(); ShowMessage("Laboratory " + code + " was saved" + (count > 0 ? " with " + count.ToString(CultureInfo.InvariantCulture) + " file(s)." : "."), true);
            });
        }

        protected void gvLabs_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id; if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id)) return;
            TryAction(delegate { if (e.CommandName == "EditLab") LoadLabEditor(id); else if (e.CommandName == "ManageLab") OpenLab(id); else if (e.CommandName == "LabFiles") OpenDocuments("Lab", id); });
        }

        protected void btnSaveEquipment_Click(object sender, EventArgs e)
        {
            TryAction(delegate
            {
                int labId; if (!int.TryParse(hidSelectedLabID.Value, out labId)) throw new InvalidOperationException("Select a laboratory first.");
                int equipmentId; int.TryParse(hidEquipmentID.Value, out equipmentId);
                LabEquipmentInput input = new LabEquipmentInput { EquipmentID = equipmentId, LabID = labId, EquipmentName = txtEquipmentName.Text,
                    EquipmentCategory = txtEquipmentCategory.Text, Quantity = IntValue(txtEquipmentQuantity.Text, "total quantity"), WorkingQuantity = IntValue(txtWorkingQuantity.Text, "working quantity"),
                    NonFunctionalQuantity = IntValue(txtNonFunctionalQuantity.Text, "non-functional quantity"), UnitName = ddlEquipmentUnit.SelectedValue,
                    PurchaseDate = DateValue(txtEquipmentPurchaseDate.Text, "purchase date"), PurchaseAmount = DecimalValue(txtEquipmentPurchaseAmount.Text, "purchase amount"),
                    ConditionStatus = ddlEquipmentCondition.SelectedValue, EquipmentLocation = txtEquipmentLocation.Text, Notes = txtEquipmentNotes.Text,
                    IsDisposed = chkEquipmentDisposed.Checked || string.Equals(ddlEquipmentCondition.SelectedValue, "Disposed", StringComparison.OrdinalIgnoreCase),
                    DisposalDate = DateValue(txtEquipmentDisposalDate.Text, "disposal date"), DisposalRemarks = txtEquipmentDisposalRemarks.Text, UserID = CurrentUserId() };
                string code = SchoolResourcesService.SaveEquipment(input);
                int count = fuEquipmentDocuments.HasFile ? SchoolResourcesService.AddDocuments("LabEquipment", input.EquipmentID, "Equipment Evidence", fuEquipmentDocuments.PostedFiles, CurrentUserId()) : 0;
                ClearEquipmentEditor(); BindEquipment(labId); BindLabs(); ShowMessage("Equipment " + code + " was saved" + (count > 0 ? " with " + count.ToString(CultureInfo.InvariantCulture) + " file(s)." : "."), true);
            });
        }

        protected void gvEquipment_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id; if (!int.TryParse(Convert.ToString(e.CommandArgument, CultureInfo.InvariantCulture), out id)) return;
            TryAction(delegate { if (e.CommandName == "EditEquipment") LoadEquipmentEditor(id); else if (e.CommandName == "EquipmentFiles") OpenDocuments("LabEquipment", id); });
        }

        protected void btnAddDocuments_Click(object sender, EventArgs e)
        {
            TryAction(delegate
            {
                int id; if (!int.TryParse(hidDocumentResourceID.Value, out id)) throw new InvalidOperationException("Open a laboratory or equipment record first.");
                string type = hidDocumentResourceType.Value;
                int count = SchoolResourcesService.AddDocuments(type, id, ddlDocumentType.SelectedValue, fuMoreDocuments.PostedFiles, CurrentUserId());
                if (count == 0) throw new InvalidOperationException("Select at least one image or PDF file.");
                OpenDocuments(type, id); BindLabs(); int labId; if (int.TryParse(hidSelectedLabID.Value, out labId)) BindEquipment(labId);
                ShowMessage(count.ToString(CultureInfo.InvariantCulture) + " evidence file(s) were uploaded.", true);
            });
        }

        protected string FormatFileSize(object value) { long bytes = Convert.ToInt64(value, CultureInfo.InvariantCulture); return bytes >= 1048576 ? (bytes / 1048576d).ToString("N2", CultureInfo.InvariantCulture) + " MB" : (bytes / 1024d).ToString("N1", CultureInfo.InvariantCulture) + " KB"; }

        private void BindLabs() { gvLabs.DataSource = SchoolResourcesService.GetLabs(txtLabSearch.Text, chkIncludeInactiveLabs.Checked); gvLabs.DataBind(); }
        private void BindEquipment(int labId) { gvEquipment.DataSource = SchoolResourcesService.GetLabEquipment(labId); gvEquipment.DataBind(); }
        private void OpenLab(int labId)
        {
            DataRow row = SchoolResourcesService.GetLab(labId); hidSelectedLabID.Value = labId.ToString(CultureInfo.InvariantCulture);
            lblSelectedLab.Text = E(row, "LabName"); lblSelectedLabMeta.Text = E(row, "LabCode") + " | " + E(row, "LabType") + " | " + E(row, "LabLocation");
            pnlEquipment.Visible = true; ClearEquipmentEditor(); BindEquipment(labId);
        }

        private void LoadLabEditor(int id)
        {
            DataRow row = SchoolResourcesService.GetLab(id); hidLabID.Value = id.ToString(CultureInfo.InvariantCulture); lblLabEditorTitle.Text = "Edit " + E(row, "LabCode");
            txtLabName.Text = E(row, "LabName"); Select(ddlLabType, E(row, "LabType")); txtLabIncharge.Text = E(row, "InchargeName"); txtLabLocation.Text = E(row, "LabLocation");
            txtLabDescription.Text = E(row, "LabDescription"); Select(ddlSafetyStatus, E(row, "SafetyStatus")); txtLabInspectionDate.Text = InputDate(row["LastInspectionDate"]); chkLabActive.Checked = Convert.ToBoolean(row["IsActive"], CultureInfo.InvariantCulture); btnSaveLab.Text = "Update Laboratory";
        }

        private void LoadEquipmentEditor(int id)
        {
            DataRow row = SchoolResourcesService.GetEquipment(id); int labId = Convert.ToInt32(row["LabID"], CultureInfo.InvariantCulture);
            if (hidSelectedLabID.Value != labId.ToString(CultureInfo.InvariantCulture)) OpenLab(labId);
            hidEquipmentID.Value = id.ToString(CultureInfo.InvariantCulture); lblEquipmentEditorTitle.Text = "Edit " + E(row, "EquipmentCode"); txtEquipmentName.Text = E(row, "EquipmentName"); txtEquipmentCategory.Text = E(row, "EquipmentCategory");
            txtEquipmentQuantity.Text = E(row, "Quantity"); txtWorkingQuantity.Text = E(row, "WorkingQuantity"); txtNonFunctionalQuantity.Text = E(row, "NonFunctionalQuantity"); Select(ddlEquipmentUnit, E(row, "UnitName"));
            txtEquipmentPurchaseDate.Text = InputDate(row["PurchaseDate"]); txtEquipmentPurchaseAmount.Text = InputDecimal(row["PurchaseAmount"]); Select(ddlEquipmentCondition, E(row, "ConditionStatus")); txtEquipmentLocation.Text = E(row, "EquipmentLocation");
            txtEquipmentNotes.Text = E(row, "Notes"); chkEquipmentDisposed.Checked = Convert.ToBoolean(row["IsDisposed"], CultureInfo.InvariantCulture); txtEquipmentDisposalDate.Text = InputDate(row["DisposalDate"]); txtEquipmentDisposalRemarks.Text = E(row, "DisposalRemarks"); btnSaveEquipment.Text = "Update Equipment";
        }

        private void OpenDocuments(string resourceType, int resourceId)
        {
            string title;
            if (resourceType == "Lab") { DataRow row = SchoolResourcesService.GetLab(resourceId); title = E(row, "LabCode") + " | " + E(row, "LabName"); }
            else { DataRow row = SchoolResourcesService.GetEquipment(resourceId); title = E(row, "EquipmentCode") + " | " + E(row, "EquipmentName"); }
            hidDocumentResourceType.Value = resourceType; hidDocumentResourceID.Value = resourceId.ToString(CultureInfo.InvariantCulture); lblDocumentTitle.Text = title;
            gvDocuments.DataSource = SchoolResourcesService.GetDocuments(resourceType, resourceId); gvDocuments.DataBind(); pnlDocuments.Visible = true;
        }

        private void ClearLabEditor() { hidLabID.Value = string.Empty; lblLabEditorTitle.Text = "Create laboratory record"; btnSaveLab.Text = "Save Laboratory"; txtLabName.Text = txtLabIncharge.Text = txtLabLocation.Text = txtLabDescription.Text = txtLabInspectionDate.Text = string.Empty; ddlLabType.SelectedIndex = 0; ddlSafetyStatus.SelectedIndex = 0; chkLabActive.Checked = true; }
        private void ClearEquipmentEditor() { hidEquipmentID.Value = string.Empty; lblEquipmentEditorTitle.Text = "Add laboratory equipment"; btnSaveEquipment.Text = "Save Equipment"; txtEquipmentName.Text = txtEquipmentCategory.Text = txtEquipmentPurchaseDate.Text = txtEquipmentPurchaseAmount.Text = txtEquipmentLocation.Text = txtEquipmentNotes.Text = txtEquipmentDisposalDate.Text = txtEquipmentDisposalRemarks.Text = string.Empty; txtEquipmentQuantity.Text = txtWorkingQuantity.Text = "1"; txtNonFunctionalQuantity.Text = "0"; ddlEquipmentUnit.SelectedIndex = 0; ddlEquipmentCondition.SelectedIndex = 0; chkEquipmentDisposed.Checked = false; }
        private void TryAction(Action action) { try { action(); } catch (Exception ex) { ShowMessage(ex.Message, false); } }
        private void ShowMessage(string message, bool success) { pnlMessage.Visible = true; pnlMessage.CssClass = "module-message " + (success ? "success" : "error"); lblMessage.Text = Server.HtmlEncode(message); }
        private int? CurrentUserId() { int id; return int.TryParse(Convert.ToString(Session["SystemUserID"], CultureInfo.InvariantCulture), out id) ? (int?)id : null; }
        private static string E(DataRow row, string column) { return row[column] == DBNull.Value ? string.Empty : Convert.ToString(row[column], CultureInfo.InvariantCulture); }
        private static string InputDate(object value) { return value == DBNull.Value ? string.Empty : Convert.ToDateTime(value, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }
        private static string InputDecimal(object value) { return value == DBNull.Value ? string.Empty : Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString("0.##", CultureInfo.InvariantCulture); }
        private static DateTime? DateValue(string value, string label) { if (string.IsNullOrWhiteSpace(value)) return null; DateTime result; if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) throw new InvalidOperationException("Enter a valid " + label + "."); return result.Date; }
        private static decimal? DecimalValue(string value, string label) { if (string.IsNullOrWhiteSpace(value)) return null; decimal result; if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result)) throw new InvalidOperationException("Enter a valid " + label + "."); return result; }
        private static int IntValue(string value, string label) { int result; if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) || result < 0) throw new InvalidOperationException("Enter a valid " + label + "."); return result; }
        private static void Select(ListControl control, string value) { ListItem item = control.Items.FindByValue(value); if (item != null) { control.ClearSelection(); item.Selected = true; } }
    }
}
