using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Web.UI;

namespace DigitalSchoolManager
{
    public partial class FTFFundManagement : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
                return;
            try
            {
                FundManagementService.EnsureSchema();
                string today = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                txtDepositDate.Text = today;
                txtUtilizationDate.Text = today;
                ApplyQuickPeriod("current");
                RefreshPage();
            }
            catch (Exception ex)
            {
                ShowMessage("FTF management could not be initialized. Run FundManagement_Database_Update.sql and verify database permissions. " + ex.Message, false);
            }
        }

        protected void btnSaveDeposit_Click(object sender, EventArgs e)
        {
            try
            {
                IList<FundDocumentUpload> documents = FundManagementService.ProcessImages(fuDepositReceipt, "Receipt", 1);
                if (documents.Count == 0)
                    throw new InvalidOperationException("Attach the bank deposit receipt image.");
                string number = FundManagementService.SaveTransaction(new FundTransactionEntry
                {
                    FundType = "FTF",
                    TransactionType = "Deposit",
                    TransactionDate = ReadDate(txtDepositDate.Text, "deposit date"),
                    Amount = ReadAmount(txtDepositAmount.Text),
                    SourceOrPayee = "FTF Bank Account",
                    ReferenceNo = txtDepositReference.Text,
                    Purpose = "Deposit from FTF Cash in Hand to FTF account",
                    Remarks = txtDepositRemarks.Text,
                    CreatedByUserID = GetCurrentUserId()
                }, documents);
                ClearDepositForm();
                RefreshPage();
                ShowMessage("FTF bank deposit " + number + " was saved and deducted from FTF Cash in Hand.", true);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, false);
            }
        }

        protected void btnEditBankAccount_Click(object sender, EventArgs e)
        {
            BindBankAccount();
            pnlBankAccountEdit.Visible = true;
            txtBankName.Focus();
        }

        protected void btnCancelBankAccount_Click(object sender, EventArgs e)
        {
            pnlBankAccountEdit.Visible = false;
            BindBankAccount();
        }

        protected void btnSaveBankAccount_Click(object sender, EventArgs e)
        {
            try
            {
                FundManagementService.SaveBankAccount(new FundBankAccountDetails
                {
                    FundType = "FTF",
                    BankName = txtBankName.Text,
                    BranchCode = txtBranchCode.Text,
                    BranchAddress = txtBranchAddress.Text,
                    AccountIBAN = txtAccountIBAN.Text,
                    UpdatedByUserID = GetCurrentUserId()
                });
                pnlBankAccountEdit.Visible = false;
                BindBankAccount();
                ShowMessage("FTF bank account details were saved successfully.", true);
            }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        protected void btnSaveUtilization_Click(object sender, EventArgs e)
        {
            try
            {
                IList<FundDocumentUpload> chequeDocuments = FundManagementService.ProcessImages(fuChequeImage, "Cheque", 1);
                IList<FundDocumentUpload> receiptDocuments = FundManagementService.ProcessImages(fuExpenseReceipts, "Receipt", 8);
                if (chequeDocuments.Count == 0)
                    throw new InvalidOperationException("Attach an image of the FTF cheque.");
                if (receiptDocuments.Count == 0)
                    throw new InvalidOperationException("Attach at least one trader receipt or invoice image.");
                var documents = new List<FundDocumentUpload>();
                foreach (FundDocumentUpload item in chequeDocuments) documents.Add(item);
                foreach (FundDocumentUpload item in receiptDocuments) documents.Add(item);

                string number = FundManagementService.SaveTransaction(new FundTransactionEntry
                {
                    FundType = "FTF",
                    TransactionType = "Utilization",
                    TransactionDate = ReadDate(txtUtilizationDate.Text, "utilization date"),
                    Amount = ReadAmount(txtUtilizationAmount.Text),
                    SourceOrPayee = txtPayee.Text,
                    ChequeNo = txtChequeNo.Text,
                    WorkType = ddlWorkType.SelectedValue,
                    Purpose = txtUtilizationPurpose.Text,
                    Remarks = txtUtilizationRemarks.Text,
                    CreatedByUserID = GetCurrentUserId()
                }, documents);
                ClearUtilizationForm();
                RefreshPage();
                ShowMessage("FTF utilization " + number + " was saved successfully.", true);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, false);
            }
        }

        protected void ddlStatementPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlStatementPeriod.SelectedValue != "custom")
                ApplyQuickPeriod(ddlStatementPeriod.SelectedValue);
            LoadStatement();
        }

        protected void btnLoadStatement_Click(object sender, EventArgs e)
        {
            try { LoadStatement(); }
            catch (Exception ex) { ShowMessage(ex.Message, false); }
        }

        private void RefreshPage()
        {
            BindBankAccount();
            BindSummary();
            LoadStatement();
        }

        private void BindBankAccount()
        {
            FundBankAccountDetails account = FundManagementService.GetBankAccount("FTF");
            string bank = account.IsConfigured ? account.BankName : "Not configured";
            string branch = string.IsNullOrWhiteSpace(account.BranchCode) ? "-" : account.BranchCode;
            string address = string.IsNullOrWhiteSpace(account.BranchAddress) ? "-" : account.BranchAddress;
            string iban = string.IsNullOrWhiteSpace(account.AccountIBAN) ? "-" : account.AccountIBAN;
            lblBankName.Text = bank; lblBranchCode.Text = branch; lblBranchAddress.Text = address; lblAccountIBAN.Text = iban;
            lblPrintBankName.Text = bank; lblPrintBranchCode.Text = branch; lblPrintBranchAddress.Text = address; lblPrintAccountIBAN.Text = iban;
            txtBankName.Text = account.BankName; txtBranchCode.Text = account.BranchCode; txtBranchAddress.Text = account.BranchAddress; txtAccountIBAN.Text = account.AccountIBAN;
        }

        private void BindSummary()
        {
            FundAccountSummary summary = FundManagementService.GetSummary();
            lblFtfCollected.Text = Money(summary.FtfCollected);
            lblFtfCashInHand.Text = Money(summary.FtfCashInHand);
            lblDepositAvailable.Text = Money(summary.FtfCashInHand);
            lblFtfUtilized.Text = Money(summary.FtfUtilized);
            lblFtfAccountBalance.Text = Money(summary.FtfAccountBalance);
            lblFtfHeroBalance.Text = Money(summary.FtfAccountBalance);
        }

        private void LoadStatement()
        {
            DateTime fromDate = ReadDate(txtFromDate.Text, "From date");
            DateTime toDate = ReadDate(txtToDate.Text, "To date");
            if (fromDate > toDate)
                throw new InvalidOperationException("The From date cannot be later than the To date.");

            DataTable collections = FundManagementService.GetFtfCollections(fromDate, toDate);
            rptStudentCollections.DataSource = collections;
            rptStudentCollections.DataBind();
            pnlNoStudentCollections.Visible = collections.Rows.Count == 0;

            FundStatementTotals walletTotals = FundManagementService.GetFtfWalletStatementTotals(fromDate, toDate);
            lblWalletOpening.Text = Money(walletTotals.OpeningBalance);
            lblWalletCollected.Text = Money(walletTotals.PeriodCredits);
            lblWalletDeposited.Text = Money(walletTotals.PeriodDebits);
            lblWalletClosing.Text = Money(walletTotals.ClosingBalance);

            DataTable transactions = FundManagementService.GetTransactions("FTF", fromDate, toDate);
            rptTransactions.DataSource = transactions;
            rptTransactions.DataBind();
            pnlNoTransactions.Visible = transactions.Rows.Count == 0;

            FundStatementTotals totals = FundManagementService.GetStatementTotals("FTF", fromDate, toDate);
            lblOpeningBalance.Text = Money(totals.OpeningBalance);
            lblPeriodDeposited.Text = Money(totals.PeriodCredits);
            lblPeriodUtilized.Text = Money(totals.PeriodDebits);
            lblClosingBalance.Text = Money(totals.ClosingBalance);

            FtfCollectionPerformance performance = FundManagementService.GetFtfPerformance(fromDate, toDate);
            lblTotalStudents.Text = performance.TotalStudents.ToString("N0", CultureInfo.InvariantCulture);
            lblStudentsWithRate.Text = performance.StudentsWithRate.ToString("N0", CultureInfo.InvariantCulture);
            lblMonthlyExpected.Text = Money(performance.MonthlyExpected);
            lblExpectedForPeriod.Text = Money(performance.ExpectedForPeriod);
            lblActualCollected.Text = Money(performance.ActualCollected);
            lblCollectionPercentage.Text = performance.CollectionPercentage.ToString("N2", CultureInfo.InvariantCulture) + "%";
            lblCollectionVariance.Text = Money(performance.Variance);
            lblPeriodMonths.Text = performance.MonthCount.ToString(CultureInfo.InvariantCulture);
            lblStatementPeriod.Text = fromDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) + " to " + toDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture);
        }

        private void ApplyQuickPeriod(string period)
        {
            DateTime today = DateTime.Today;
            DateTime from;
            DateTime to;
            switch (period)
            {
                case "last": from = new DateTime(today.Year, today.Month, 1).AddMonths(-1); to = from.AddMonths(1).AddDays(-1); break;
                case "six": from = new DateTime(today.Year, today.Month, 1).AddMonths(-5); to = today; break;
                case "year": from = new DateTime(today.Year, today.Month, 1).AddMonths(-11); to = today; break;
                default: from = new DateTime(today.Year, today.Month, 1); to = today; break;
            }
            txtFromDate.Text = from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            txtToDate.Text = to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static DateTime ReadDate(string text, string fieldName)
        {
            DateTime value;
            if (!DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
                throw new InvalidOperationException("Enter a valid " + fieldName + ".");
            return value.Date;
        }

        private static decimal ReadAmount(string text)
        {
            decimal value;
            if ((!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value) && !decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value)) || value <= 0m)
                throw new InvalidOperationException("Enter an amount greater than zero.");
            return decimal.Round(value, 2);
        }

        private int? GetCurrentUserId()
        {
            int userId;
            return Session != null && int.TryParse(Convert.ToString(Session["SystemUserID"]), out userId) ? (int?)userId : null;
        }

        private void ClearDepositForm()
        {
            txtDepositAmount.Text = string.Empty;
            txtDepositReference.Text = string.Empty;
            txtDepositRemarks.Text = string.Empty;
        }

        private void ClearUtilizationForm()
        {
            txtUtilizationAmount.Text = string.Empty;
            txtChequeNo.Text = string.Empty;
            txtPayee.Text = string.Empty;
            txtUtilizationPurpose.Text = string.Empty;
            txtUtilizationRemarks.Text = string.Empty;
            ddlWorkType.SelectedIndex = 0;
        }

        private void ShowMessage(string message, bool success)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = success ? "fund-message fund-message-success" : "fund-message fund-message-error";
            lblMessage.Text = message;
        }

        private static string Money(decimal value) { return "Rs. " + value.ToString("N2", CultureInfo.InvariantCulture); }
    }
}
