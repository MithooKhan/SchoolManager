using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class BudgetSetup : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            try { BudgetManagementService.EnsureSchema(); BindAll(); }
            catch (Exception ex) { ShowMessage("Budget setup could not be initialized. " + ex.Message, false); }
        }

        protected void btnAddAllowance_Click(object sender, EventArgs e)
        {
            try
            {
                int order; int.TryParse(txtAllowanceOrder.Text, out order);
                BudgetManagementService.SaveAllowance(null, txtAllowanceCode.Text, txtAllowanceName.Text, order, true);
                txtAllowanceCode.Text = txtAllowanceName.Text = string.Empty;
                BindAllowances(); ShowMessage("Allowance added successfully.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        protected void btnSaveAllowances_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (GridViewRow row in gvAllowances.Rows)
                {
                    int id = Convert.ToInt32(gvAllowances.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                    int order; int.TryParse(((TextBox)row.FindControl("txtOrder")).Text, out order);
                    BudgetManagementService.SaveAllowance(id, ((TextBox)row.FindControl("txtCode")).Text, ((TextBox)row.FindControl("txtName")).Text, order, ((CheckBox)row.FindControl("chkActive")).Checked);
                }
                BindAllowances(); ShowMessage("Allowance master saved successfully.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        protected void btnSavePosts_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (GridViewRow row in gvPosts.Rows)
                {
                    string category = Convert.ToString(gvPosts.DataKeys[row.RowIndex].Values["StaffCategory"]);
                    int sourcePostId = Convert.ToInt32(gvPosts.DataKeys[row.RowIndex].Values["SourcePostID"], CultureInfo.InvariantCulture);
                    string code = ((TextBox)row.FindControl("txtPostCode")).Text;
                    BudgetManagementService.SavePostCode(category, sourcePostId, code, row.Cells[1].Text == "&nbsp;" ? string.Empty : Server.HtmlDecode(row.Cells[1].Text), Convert.ToInt32(row.Cells[2].Text, CultureInfo.InvariantCulture));
                }
                BindPosts(); ShowMessage("Permanent post codes saved successfully.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        protected void btnRefreshPosts_Click(object sender, EventArgs e) { try { BindPosts(); ShowMessage("Vacancy positions refreshed.", true); } catch (Exception ex) { ShowMessage(ex.Message, false); } }

        protected void btnSavePayScales_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (GridViewRow row in gvPayScales.Rows)
                {
                    int bps = Convert.ToInt32(gvPayScales.DataKeys[row.RowIndex].Value, CultureInfo.InvariantCulture);
                    decimal minimum = ReadMoney((TextBox)row.FindControl("txtMinimum"));
                    decimal maximum = ReadMoney((TextBox)row.FindControl("txtMaximum"));
                    decimal increment = ReadMoney((TextBox)row.FindControl("txtIncrement"));
                    BudgetManagementService.SavePayScale(bps, minimum, maximum, increment);
                }
                BindPayScales(); ShowMessage("Pay scales saved successfully.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        private void BindAll() { BindAllowances(); BindPosts(); BindPayScales(); }
        private void BindAllowances() { gvAllowances.DataSource = BudgetManagementService.GetAllowances(false); gvAllowances.DataBind(); }
        private void BindPosts() { gvPosts.DataSource = BudgetManagementService.GetSourcePosts(); gvPosts.DataBind(); }
        private void BindPayScales() { gvPayScales.DataSource = BudgetManagementService.GetPayScales(); gvPayScales.DataBind(); }
        private static decimal ReadMoney(TextBox box) { decimal value; if (!decimal.TryParse(box.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out value) && !decimal.TryParse(box.Text, out value)) throw new InvalidOperationException("Enter valid pay-scale amounts."); return value; }
        private void ShowMessage(string message, bool success) { pnlMessage.Visible = true; pnlMessage.CssClass = "budget-message " + (success ? "success" : "error"); lblMessage.Text = Server.HtmlEncode(message); }
    }
}
