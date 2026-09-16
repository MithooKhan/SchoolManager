using System;
using System.Data;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class BudgetReports : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            try
            {
                BudgetManagementService.EnsureSchema();
                int requested; int.TryParse(Request.QueryString["budget"], out requested);
                BindBudgets(string.Empty, requested);
                if (requested > 0) LoadReport(requested);
            }
            catch (Exception ex) { ShowMessage("Budget reports could not be initialized. " + ex.Message, false); }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try { BindBudgets(txtSearch.Text, 0); pnlReport.Visible = false; ShowMessage(ddlBudgets.Items.Count > 1 ? "Matching budget files loaded." : "No budget file matched that name.", ddlBudgets.Items.Count > 1); }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        protected void ddlBudgets_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id; if (int.TryParse(ddlBudgets.SelectedValue, out id)) LoadReport(id); else pnlReport.Visible = false;
        }

        private void BindBudgets(string search, int selectedId)
        {
            DataTable budgets = BudgetManagementService.GetBudgets(search);
            ddlBudgets.DataSource = budgets; ddlBudgets.DataTextField = "BudgetName"; ddlBudgets.DataValueField = "BudgetID"; ddlBudgets.DataBind();
            ddlBudgets.Items.Insert(0, new ListItem("Select a saved budget file", string.Empty));
            string value = selectedId.ToString(CultureInfo.InvariantCulture);
            if (selectedId > 0 && ddlBudgets.Items.FindByValue(value) != null) ddlBudgets.SelectedValue = value;
        }

        private void LoadReport(int budgetId)
        {
            litReport.Text = BudgetReportBuilder.Build(BudgetManagementService.GetReportData(budgetId));
            pnlReport.Visible = true;
        }

        private void ShowMessage(string message, bool success) { pnlMessage.Visible = true; pnlMessage.CssClass = "budget-message " + (success ? "success" : "error"); lblMessage.Text = Server.HtmlEncode(message); }
    }
}
