using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class AnnualBudget : Page
    {
        private int CurrentBudgetId
        {
            get { return ViewState["BudgetID"] == null ? 0 : Convert.ToInt32(ViewState["BudgetID"], CultureInfo.InvariantCulture); }
            set { ViewState["BudgetID"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            try
            {
                BudgetManagementService.EnsureSchema();
                SetDefaults();
                BindBudgetList(0);
            }
            catch (Exception ex) { ShowMessage("Annual budget could not be initialized. " + ex.Message, false); }
        }

        protected void btnCreateBudget_Click(object sender, EventArgs e)
        {
            try
            {
                int year;
                if (!int.TryParse(txtFiscalStartYear.Text, out year)) throw new InvalidOperationException("Enter a valid fiscal start year.");
                int budgetId = BudgetManagementService.CreateBudget(new BudgetHeader
                {
                    BudgetName = txtBudgetName.Text,
                    FiscalStartYear = year,
                    SchoolName = txtSchoolName.Text,
                    LocalGovernmentName = txtLocalGovernment.Text,
                    DemandName = txtDemandName.Text,
                    GrantNo = txtGrantNo.Text,
                    CostCenterCode = txtCostCenter.Text,
                    DetailedFunctionCode = txtFunctionCode.Text,
                    DetailedFunctionName = txtFunctionName.Text,
                    CreatedByUserID = GetCurrentUserId()
                });
                BindBudgetList(budgetId);
                OpenBudget(budgetId);
                ShowMessage("Budget file created and current staff and vacancies loaded successfully.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        protected void ddlBudgets_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id; if (int.TryParse(ddlBudgets.SelectedValue, out id)) OpenBudget(id); else { CurrentBudgetId = 0; pnlWorkspace.Visible = false; }
        }

        protected void btnRefreshBudgets_Click(object sender, EventArgs e) { try { BindBudgetList(CurrentBudgetId); ShowMessage("Budget archive refreshed.", true); } catch (Exception ex) { ShowMessage(ex.Message, false); } }

        protected void btnSyncStaff_Click(object sender, EventArgs e)
        {
            try { RequireBudget(); BudgetManagementService.SyncBudgetStaff(CurrentBudgetId); BindWorkspace(); ShowMessage("New staff and current vacant positions were synchronized. Existing saved figures were preserved.", true); }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        protected void btnSaveStaff_Click(object sender, EventArgs e)
        {
            try
            {
                RequireBudget();
                foreach (GridViewRow row in gvStaff.Rows)
                {
                    int lineId = Convert.ToInt32(gvStaff.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                    decimal basic = ReadMoney((TextBox)row.FindControl("txtBasicPay"), "basic pay");
                    decimal increment = ReadMoney((TextBox)row.FindControl("txtIncrementRate"), "increment rate");
                    DateTime? date = ReadOptionalDate(((TextBox)row.FindControl("txtIncrementDate")).Text);
                    bool recruit = ((CheckBox)row.FindControl("chkRecruitment")).Checked;
                    string remarks = ((TextBox)row.FindControl("txtRemarks")).Text;
                    BudgetManagementService.SaveStaffLine(lineId, basic, date, increment, recruit, remarks);
                }
                BindWorkspace(); ShowMessage("Staff pay and increment figures saved.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        protected void ddlAllowanceStaff_SelectedIndexChanged(object sender, EventArgs e) { try { BindAllowanceGrid(); } catch (Exception ex) { ShowMessage(ex.Message, false); } }

        protected void btnSaveAllowances_Click(object sender, EventArgs e)
        {
            try
            {
                int staffLineId;
                if (!int.TryParse(ddlAllowanceStaff.SelectedValue, out staffLineId)) throw new InvalidOperationException("Select an officer, official, or vacant post.");
                foreach (GridViewRow row in gvStaffAllowances.Rows)
                {
                    int allowanceId = Convert.ToInt32(gvStaffAllowances.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                    decimal amount = ReadMoney((TextBox)row.FindControl("txtMonthlyAmount"), "monthly allowance");
                    BudgetManagementService.SaveStaffAllowance(staffLineId, allowanceId, amount);
                }
                BindAllowanceGrid(); BindTotals(); ShowMessage("Monthly allowance figures saved and annual provisions recalculated.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        protected void btnSaveObjects_Click(object sender, EventArgs e)
        {
            try
            {
                RequireBudget();
                foreach (GridViewRow row in gvObjects.Rows)
                {
                    int id = Convert.ToInt32(gvObjects.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                    BudgetManagementService.SaveObjectEntry(id,
                        ReadMoney((TextBox)row.FindControl("txtPrevious"), "previous budget"),
                        ReadMoney((TextBox)row.FindControl("txtRevised"), "current revised estimate"),
                        ReadMoney((TextBox)row.FindControl("txtProposed"), "proposed estimate"));
                }
                BindWorkspace(); ShowMessage("Other budget object heads saved.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        protected void btnFinalize_Click(object sender, EventArgs e)
        {
            try
            {
                RequireBudget();
                bool makeFinal = btnFinalize.Text.StartsWith("Finalize", StringComparison.OrdinalIgnoreCase);
                BudgetManagementService.FinalizeBudget(CurrentBudgetId, makeFinal);
                BindWorkspace(); BindBudgetList(CurrentBudgetId);
                ShowMessage(makeFinal ? "Budget finalized. It remains searchable and printable at any time." : "Budget returned to Draft status for editing.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        private void OpenBudget(int budgetId) { CurrentBudgetId = budgetId; pnlWorkspace.Visible = true; BindWorkspace(); }

        private void BindWorkspace()
        {
            DataRow budget = BudgetManagementService.GetBudget(CurrentBudgetId);
            lblActiveBudget.Text = Convert.ToString(budget["BudgetName"]);
            string status = Convert.ToString(budget["Status"]);
            lblBudgetStatus.Text = status + " | " + Convert.ToString(budget["CostCenterCode"]);
            btnFinalize.Text = status == "Finalized" ? "Reopen as Draft" : "Finalize Budget";
            lnkOpenReports.NavigateUrl = "~/BudgetReports.aspx?budget=" + CurrentBudgetId.ToString(CultureInfo.InvariantCulture);
            lnkReports.NavigateUrl = lnkOpenReports.NavigateUrl;

            DataTable staff = BudgetManagementService.GetBudgetStaff(CurrentBudgetId);
            gvStaff.DataSource = staff; gvStaff.DataBind();
            BindStaffSelector(staff);
            gvObjects.DataSource = BudgetManagementService.GetObjectEntries(CurrentBudgetId); gvObjects.DataBind();
            BindAllowanceGrid(); BindTotals();
        }

        private void BindStaffSelector(DataTable staff)
        {
            string selected = ddlAllowanceStaff.SelectedValue;
            ddlAllowanceStaff.Items.Clear();
            ddlAllowanceStaff.Items.Add(new ListItem("Select staff or vacancy", string.Empty));
            foreach (DataRow row in staff.Rows)
            {
                string text = Convert.ToString(row["EmployeeName"]) + " - " + Convert.ToString(row["Designation"]) + " (BPS-" + Convert.ToString(row["BPS"]) + ")";
                ddlAllowanceStaff.Items.Add(new ListItem(text, Convert.ToString(row["BudgetStaffLineID"])));
            }
            if (ddlAllowanceStaff.Items.FindByValue(selected) != null) ddlAllowanceStaff.SelectedValue = selected;
        }

        private void BindAllowanceGrid()
        {
            int staffLineId;
            if (!int.TryParse(ddlAllowanceStaff.SelectedValue, out staffLineId)) { gvStaffAllowances.DataSource = null; gvStaffAllowances.DataBind(); btnSaveAllowances.Enabled = false; return; }
            gvStaffAllowances.DataSource = BudgetManagementService.GetStaffAllowances(staffLineId); gvStaffAllowances.DataBind(); btnSaveAllowances.Enabled = true;
        }

        private void BindTotals()
        {
            BudgetTotals totals = BudgetManagementService.GetTotals(CurrentBudgetId);
            lblPayTotal.Text = Money(totals.Pay); lblAllowanceTotal.Text = Money(totals.Allowances); lblGrandTotal.Text = Money(totals.GrandTotal);
        }

        private void BindBudgetList(int selectedId)
        {
            DataTable budgets = BudgetManagementService.GetBudgets(string.Empty);
            ddlBudgets.DataSource = budgets; ddlBudgets.DataTextField = "BudgetName"; ddlBudgets.DataValueField = "BudgetID"; ddlBudgets.DataBind();
            ddlBudgets.Items.Insert(0, new ListItem("Select a saved budget file", string.Empty));
            string value = selectedId.ToString(CultureInfo.InvariantCulture);
            if (selectedId > 0 && ddlBudgets.Items.FindByValue(value) != null) ddlBudgets.SelectedValue = value;
        }

        private void SetDefaults()
        {
            int startYear = DateTime.Today.Month >= 7 ? DateTime.Today.Year : DateTime.Today.Year - 1;
            txtFiscalStartYear.Text = startYear.ToString(CultureInfo.InvariantCulture);
            txtBudgetName.Text = "Budget Estimate " + startYear + "-" + ((startYear + 1) % 100).ToString("00", CultureInfo.InvariantCulture);
            txtSchoolName.Text = "Government Higher Secondary School Maankot, Tehsil Kabirwala, District Khanewal";
            txtLocalGovernment.Text = "DISTRICT GOVERNMENT KHANEWAL";
            txtDemandName.Text = "DEMAND 15-EDUCATION (DEA, KHANEWAL)";
            txtGrantNo.Text = "15"; txtCostCenter.Text = "KC6202"; txtFunctionCode.Text = "092101"; txtFunctionName.Text = "SECONDARY EDUCATION";
        }

        private void RequireBudget() { if (CurrentBudgetId <= 0) throw new InvalidOperationException("Select or create a budget first."); }
        private int? GetCurrentUserId() { int id; return Session != null && int.TryParse(Convert.ToString(Session["SystemUserID"]), out id) ? (int?)id : null; }
        private static string Money(decimal value) { return "Rs. " + value.ToString("N2", CultureInfo.InvariantCulture); }
        private static decimal ReadMoney(TextBox box, string field) { decimal value; if (!decimal.TryParse(box.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out value) && !decimal.TryParse(box.Text, out value)) throw new InvalidOperationException("Enter a valid " + field + "."); return value; }
        private static DateTime? ReadOptionalDate(string text) { if (string.IsNullOrWhiteSpace(text)) return null; DateTime value; if (!DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out value)) throw new InvalidOperationException("Enter a valid increment date."); return value.Date; }
        private void ShowMessage(string message, bool success) { pnlMessage.Visible = true; pnlMessage.CssClass = "budget-message " + (success ? "success" : "error"); lblMessage.Text = Server.HtmlEncode(message); }
    }
}
