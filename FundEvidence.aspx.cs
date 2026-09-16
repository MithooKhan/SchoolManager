using System;
using System.Data;
using System.Globalization;
using System.Web.UI;

namespace DigitalSchoolManager
{
    public partial class FundEvidence : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;
            int transactionId;
            if (!int.TryParse(Request.QueryString["transaction"], out transactionId) || transactionId <= 0)
            {
                ShowError("The selected fund transaction reference is invalid.");
                return;
            }

            try
            {
                FundManagementService.EnsureSchema();
                FundTransactionEntry entry = FundManagementService.GetTransaction(transactionId);
                if (entry == null)
                {
                    ShowError("The selected fund transaction could not be found.");
                    return;
                }

                lblTransactionNumber.Text = entry.TransactionNumber;
                lblFundType.Text = entry.FundType + " / " + entry.TransactionType;
                lblTransactionDate.Text = entry.TransactionDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
                lblAmount.Text = "Rs. " + entry.Amount.ToString("N2", CultureInfo.InvariantCulture);
                lblReference.Text = JoinValues(entry.ReferenceNo, entry.ChequeNo);
                lblPurpose.Text = string.IsNullOrWhiteSpace(entry.Purpose) ? "Not specified" : entry.Purpose;

                DataTable documents = FundManagementService.GetDocuments(transactionId);
                rptDocuments.DataSource = documents;
                rptDocuments.DataBind();
                pnlNoDocuments.Visible = documents.Rows.Count == 0;
                pnlEvidence.Visible = true;
            }
            catch (Exception ex)
            {
                ShowError("The transaction evidence could not be loaded. " + ex.Message);
            }
        }

        private static string JoinValues(string reference, string cheque)
        {
            reference = (reference ?? string.Empty).Trim();
            cheque = (cheque ?? string.Empty).Trim();
            if (reference.Length == 0 && cheque.Length == 0) return "Not specified";
            if (reference.Length == 0) return "Cheque " + cheque;
            if (cheque.Length == 0) return reference;
            return reference + " / Cheque " + cheque;
        }

        private void ShowError(string message)
        {
            pnlEvidence.Visible = false;
            pnlMessage.Visible = true;
            lblMessage.Text = message;
        }
    }
}
