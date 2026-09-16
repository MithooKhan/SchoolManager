<%@ Page Title="FTF Fund Management" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="FTFFundManagement.aspx.cs" Inherits="DigitalSchoolManager.FTFFundManagement" %>

<asp:Content ID="FTFFundContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="fund-page fund-page-ftf">
        <section class="fund-hero" aria-labelledby="ftfPageTitle">
            <div class="fund-hero-brand">
                <img src="<%= ResolveUrl("~/images/SchoolLogo.png") %>" alt="School logo" />
                <div><span>Funds and Accounts</span><h1 id="ftfPageTitle">Farogh-e-Taleem Fund Management</h1><p>Reconcile student collections, cash in hand, bank deposits, and approved expenditure.</p></div>
            </div>
            <div class="fund-hero-balance"><span>FTF Account Balance</span><strong><asp:Label ID="lblFtfHeroBalance" runat="server" Text="Rs. 0.00" /></strong><small>Bank deposits less utilization</small></div>
        </section>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="fund-message" role="status" aria-live="polite"><asp:Label ID="lblMessage" runat="server" /></asp:Panel>

        <section class="fund-bank-account-card" aria-labelledby="ftfBankAccountTitle">
            <div class="fund-bank-account-heading"><div><span>Official bank profile</span><h2 id="ftfBankAccountTitle">FTF Bank Account Details</h2><p>Maintain the account used for Farogh-e-Taleem Fund deposits and utilization.</p></div><asp:Button ID="btnEditBankAccount" runat="server" Text="Add or Edit Details" CssClass="fund-button fund-button-secondary no-print" CausesValidation="false" OnClick="btnEditBankAccount_Click" /></div>
            <div class="fund-bank-account-display"><div><span>Bank name</span><strong><asp:Label ID="lblBankName" runat="server" Text="Not configured" /></strong></div><div><span>Branch code</span><strong><asp:Label ID="lblBranchCode" runat="server" Text="-" /></strong></div><div><span>Branch address</span><strong><asp:Label ID="lblBranchAddress" runat="server" Text="-" /></strong></div><div><span>Account No. / IBAN</span><strong class="fund-iban"><asp:Label ID="lblAccountIBAN" runat="server" Text="-" /></strong></div></div>
            <asp:Panel ID="pnlBankAccountEdit" runat="server" Visible="false" CssClass="fund-bank-account-editor no-print">
                <div class="fund-field"><label for="<%= txtBankName.ClientID %>">Bank name</label><asp:TextBox ID="txtBankName" runat="server" CssClass="fund-control" MaxLength="150" /></div>
                <div class="fund-field"><label for="<%= txtBranchCode.ClientID %>">Branch code</label><asp:TextBox ID="txtBranchCode" runat="server" CssClass="fund-control" MaxLength="50" /></div>
                <div class="fund-field fund-field-wide"><label for="<%= txtBranchAddress.ClientID %>">Branch address</label><asp:TextBox ID="txtBranchAddress" runat="server" CssClass="fund-control" MaxLength="300" /></div>
                <div class="fund-field fund-field-wide"><label for="<%= txtAccountIBAN.ClientID %>">Account No. / IBAN</label><asp:TextBox ID="txtAccountIBAN" runat="server" CssClass="fund-control" MaxLength="50" /></div>
                <div class="fund-bank-account-actions"><asp:Button ID="btnSaveBankAccount" runat="server" Text="Save Account Details" CssClass="fund-button fund-button-primary" OnClick="btnSaveBankAccount_Click" /><asp:Button ID="btnCancelBankAccount" runat="server" Text="Cancel" CssClass="fund-button fund-button-secondary" CausesValidation="false" OnClick="btnCancelBankAccount_Click" /></div>
            </asp:Panel>
        </section>

        <section class="fund-summary-grid fund-summary-grid-four" aria-label="FTF financial summary">
            <article class="fund-summary-card fund-summary-credit"><span>Collected from Students</span><strong><asp:Label ID="lblFtfCollected" runat="server" /></strong><small>Automatic from FTF fee vouchers</small></article>
            <article class="fund-summary-card fund-summary-wallet"><span>FTF Cash in Hand</span><strong><asp:Label ID="lblFtfCashInHand" runat="server" /></strong><small>Collections not yet deposited</small></article>
            <article class="fund-summary-card fund-summary-debit"><span>Total Utilized</span><strong><asp:Label ID="lblFtfUtilized" runat="server" /></strong><small>Paid from FTF account</small></article>
            <article class="fund-summary-card fund-summary-balance"><span>FTF Account Balance</span><strong><asp:Label ID="lblFtfAccountBalance" runat="server" /></strong><small>Deposited less utilized</small></article>
        </section>

        <div class="fund-flow-strip" aria-label="FTF accounting flow">
            <div><span>1</span><strong>Student pays FTF</strong><small>Fee voucher credits Cash in Hand</small></div>
            <div><span>2</span><strong>Deposit to account</strong><small>Cash in Hand is reduced</small></div>
            <div><span>3</span><strong>Utilize by cheque</strong><small>FTF account balance is reduced</small></div>
        </div>
        <div class="fund-rule-note"><strong>Automatic wallet rule:</strong> a fee charge whose description contains “FTF” or “Farogh-e-Taleem” is counted in FTF Cash in Hand as soon as its student voucher is saved.</div>

        <section class="fund-entry-grid">
            <article class="fund-entry-card">
                <header class="fund-card-heading"><span class="fund-card-number">01</span><div><span>Cash to bank</span><h2>Deposit FTF in Account</h2><p>Move collected FTF from Cash in Hand into the school FTF account.</p></div></header>
                <div class="fund-available-callout"><span>Available to deposit</span><strong><asp:Label ID="lblDepositAvailable" runat="server" /></strong></div>
                <div class="fund-form-grid">
                    <div class="fund-field"><label for="<%= txtDepositDate.ClientID %>">Deposit date</label><asp:TextBox ID="txtDepositDate" runat="server" TextMode="Date" CssClass="fund-control" /></div>
                    <div class="fund-field"><label for="<%= txtDepositAmount.ClientID %>">Amount deposited (Rs.)</label><asp:TextBox ID="txtDepositAmount" runat="server" TextMode="Number" CssClass="fund-control" min="0.01" step="0.01" placeholder="0.00" /></div>
                    <div class="fund-field"><label for="<%= txtDepositReference.ClientID %>">Bank slip / reference number</label><asp:TextBox ID="txtDepositReference" runat="server" CssClass="fund-control" MaxLength="100" placeholder="Deposit slip reference" /></div>
                    <div class="fund-field"><label for="<%= fuDepositReceipt.ClientID %>">Deposit receipt image</label><asp:FileUpload ID="fuDepositReceipt" runat="server" CssClass="fund-file-control" accept="image/jpeg,image/png,image/gif,image/bmp" /><small>Required; automatically optimized below 1 MB.</small></div>
                    <div class="fund-field fund-field-wide"><label for="<%= txtDepositRemarks.ClientID %>">Remarks</label><asp:TextBox ID="txtDepositRemarks" runat="server" CssClass="fund-control" MaxLength="500" placeholder="Bank branch, account note, or other details" /></div>
                </div>
                <asp:Button ID="btnSaveDeposit" runat="server" Text="Save FTF Bank Deposit" CssClass="fund-button fund-button-primary" OnClick="btnSaveDeposit_Click" />
            </article>

            <article class="fund-entry-card">
                <header class="fund-card-heading"><span class="fund-card-number">02</span><div><span>Approved expenditure</span><h2>Record FTF Utilization</h2><p>Save the cheque, purpose, payee, and trader receipts.</p></div></header>
                <div class="fund-form-grid">
                    <div class="fund-field"><label for="<%= txtUtilizationDate.ClientID %>">Utilization date</label><asp:TextBox ID="txtUtilizationDate" runat="server" TextMode="Date" CssClass="fund-control" /></div>
                    <div class="fund-field"><label for="<%= txtUtilizationAmount.ClientID %>">Amount utilized (Rs.)</label><asp:TextBox ID="txtUtilizationAmount" runat="server" TextMode="Number" CssClass="fund-control" min="0.01" step="0.01" placeholder="0.00" /></div>
                    <div class="fund-field"><label for="<%= txtChequeNo.ClientID %>">Cheque number</label><asp:TextBox ID="txtChequeNo" runat="server" CssClass="fund-control" MaxLength="100" placeholder="Required" /></div>
                    <div class="fund-field"><label for="<%= ddlWorkType.ClientID %>">Work / expense type</label><asp:DropDownList ID="ddlWorkType" runat="server" CssClass="fund-control"><asp:ListItem Text="Student Welfare" /><asp:ListItem Text="Learning Material" /><asp:ListItem Text="Stationery" /><asp:ListItem Text="Repair Work" /><asp:ListItem Text="Furniture and Equipment" /><asp:ListItem Text="School Improvement" /><asp:ListItem Text="Other" /></asp:DropDownList></div>
                    <div class="fund-field fund-field-wide"><label for="<%= txtPayee.ClientID %>">Payee / vendor</label><asp:TextBox ID="txtPayee" runat="server" CssClass="fund-control" MaxLength="200" placeholder="Person, firm, or service provider" /></div>
                    <div class="fund-field fund-field-wide"><label for="<%= txtUtilizationPurpose.ClientID %>">Purpose and work details</label><asp:TextBox ID="txtUtilizationPurpose" runat="server" CssClass="fund-control" TextMode="MultiLine" Rows="3" MaxLength="500" placeholder="Describe the work, goods, or student service" /></div>
                    <div class="fund-field"><label for="<%= fuChequeImage.ClientID %>">Cheque image</label><asp:FileUpload ID="fuChequeImage" runat="server" CssClass="fund-file-control" accept="image/jpeg,image/png,image/gif,image/bmp" /><small>Required; automatically optimized below 1 MB.</small></div>
                    <div class="fund-field"><label for="<%= fuExpenseReceipts.ClientID %>">Trader receipts / invoices</label><asp:FileUpload ID="fuExpenseReceipts" runat="server" CssClass="fund-file-control" AllowMultiple="true" accept="image/jpeg,image/png,image/gif,image/bmp" /><small>Attach up to 8 receipt images.</small></div>
                    <div class="fund-field fund-field-wide"><label for="<%= txtUtilizationRemarks.ClientID %>">Remarks</label><asp:TextBox ID="txtUtilizationRemarks" runat="server" CssClass="fund-control" MaxLength="500" placeholder="Optional approval or procurement notes" /></div>
                </div>
                <asp:Button ID="btnSaveUtilization" runat="server" Text="Save FTF Utilization" CssClass="fund-button fund-button-danger" OnClick="btnSaveUtilization_Click" />
            </article>
        </section>

        <section class="fund-statement-card" aria-labelledby="ftfStatementTitle">
            <div class="fund-statement-toolbar no-print">
                <div><span>Reconciliation and reporting</span><h2 id="ftfStatementTitle">FTF Collection and Account Statement</h2></div>
                <div class="fund-filter-row">
                    <div class="fund-field"><label for="<%= ddlStatementPeriod.ClientID %>">Period</label><asp:DropDownList ID="ddlStatementPeriod" runat="server" CssClass="fund-control" AutoPostBack="true" OnSelectedIndexChanged="ddlStatementPeriod_SelectedIndexChanged"><asp:ListItem Value="current">Current month</asp:ListItem><asp:ListItem Value="last">Last month</asp:ListItem><asp:ListItem Value="six">Previous 6 months</asp:ListItem><asp:ListItem Value="year">Previous 12 months</asp:ListItem><asp:ListItem Value="custom">Custom period</asp:ListItem></asp:DropDownList></div>
                    <div class="fund-field"><label for="<%= txtFromDate.ClientID %>">From</label><asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" CssClass="fund-control" /></div>
                    <div class="fund-field"><label for="<%= txtToDate.ClientID %>">To</label><asp:TextBox ID="txtToDate" runat="server" TextMode="Date" CssClass="fund-control" /></div>
                    <asp:Button ID="btnLoadStatement" runat="server" Text="Load Statement" CssClass="fund-button fund-button-secondary" CausesValidation="false" OnClick="btnLoadStatement_Click" />
                    <button type="button" class="fund-button fund-button-print" onclick="window.print();">Print Statement</button>
                </div>
            </div>

            <div id="ftfStatementPrintArea" class="fund-print-area">
                <header class="fund-print-header"><img src="<%= ResolveUrl("~/images/SchoolLogo.png") %>" alt="School logo" /><div><span>Government of the Punjab</span><h2>Government Higher Secondary School Maankot</h2><strong>Farogh-e-Taleem Fund Collection and Account Statement</strong><small><asp:Label ID="lblStatementPeriod" runat="server" /></small></div></header>
                <div class="fund-print-bank-line"><strong>FTF Bank Account:</strong> <asp:Label ID="lblPrintBankName" runat="server" /> | Branch: <asp:Label ID="lblPrintBranchCode" runat="server" /> | IBAN: <asp:Label ID="lblPrintAccountIBAN" runat="server" /> | <asp:Label ID="lblPrintBranchAddress" runat="server" /></div>

                <section class="fund-performance-panel" aria-labelledby="ftfPerformanceTitle">
                    <div class="fund-subheading"><div><span>Collection performance</span><h3 id="ftfPerformanceTitle">Expected vs Actual FTF</h3></div><small>Expected amount uses current active students and configured class FTF rates.</small></div>
                    <div class="fund-performance-grid">
                        <div><span>Active Students</span><strong><asp:Label ID="lblTotalStudents" runat="server" /></strong><small><asp:Label ID="lblStudentsWithRate" runat="server" /> with an FTF rate</small></div>
                        <div><span>Monthly Target</span><strong><asp:Label ID="lblMonthlyExpected" runat="server" /></strong><small>Current enrolment</small></div>
                        <div><span>Expected in Period</span><strong><asp:Label ID="lblExpectedForPeriod" runat="server" /></strong><small><asp:Label ID="lblPeriodMonths" runat="server" /> month(s)</small></div>
                        <div><span>Actually Collected</span><strong><asp:Label ID="lblActualCollected" runat="server" /></strong><small><asp:Label ID="lblCollectionPercentage" runat="server" /> of target</small></div>
                        <div><span>Variance</span><strong><asp:Label ID="lblCollectionVariance" runat="server" /></strong><small>Actual less expected</small></div>
                    </div>
                </section>

                <section class="fund-statement-section">
                    <div class="fund-subheading"><div><span>Wallet activity</span><h3>Student FTF Collections</h3></div><small>Credited automatically from saved student fee vouchers.</small></div>
                    <div class="fund-print-totals fund-wallet-totals"><div><span>Opening Cash in Hand</span><strong><asp:Label ID="lblWalletOpening" runat="server" /></strong></div><div><span>Collected in Period</span><strong><asp:Label ID="lblWalletCollected" runat="server" /></strong></div><div><span>Deposited in Period</span><strong><asp:Label ID="lblWalletDeposited" runat="server" /></strong></div><div><span>Closing Cash in Hand</span><strong><asp:Label ID="lblWalletClosing" runat="server" /></strong></div></div>
                    <div class="fund-table-wrap"><table class="fund-statement-table"><thead><tr><th>No.</th><th>Date</th><th>Voucher No.</th><th>Student</th><th>Registration</th><th>Class</th><th>FTF Collected</th></tr></thead><tbody><asp:Repeater ID="rptStudentCollections" runat="server"><ItemTemplate><tr><td><%# Container.ItemIndex + 1 %></td><td><%#: Eval("DateOfDeposit", "{0:dd MMM yyyy}") %></td><td><strong><%#: Eval("VoucherNo") %></strong></td><td><%#: Eval("StudentName") %></td><td><%#: Eval("RegistrationNo") %></td><td><%#: Eval("ClassName") %></td><td class="fund-money"><%# String.Format(System.Globalization.CultureInfo.InvariantCulture, "Rs. {0:N2}", Eval("Amount")) %></td></tr></ItemTemplate></asp:Repeater></tbody></table><asp:Panel ID="pnlNoStudentCollections" runat="server" Visible="false" CssClass="fund-empty-state">No student FTF collections were found in the selected period.</asp:Panel></div>
                </section>

                <section class="fund-statement-section">
                    <div class="fund-subheading"><div><span>Bank account activity</span><h3>FTF Deposits and Utilization</h3></div><small>All saved deposit slips, cheques, and trader receipts remain attached.</small></div>
                    <div class="fund-print-totals"><div><span>Opening Account Balance</span><strong><asp:Label ID="lblOpeningBalance" runat="server" /></strong></div><div><span>Deposited in Period</span><strong><asp:Label ID="lblPeriodDeposited" runat="server" /></strong></div><div><span>Utilized in Period</span><strong><asp:Label ID="lblPeriodUtilized" runat="server" /></strong></div><div><span>Closing Account Balance</span><strong><asp:Label ID="lblClosingBalance" runat="server" /></strong></div></div>
                    <div class="fund-table-wrap"><table class="fund-statement-table"><thead><tr><th>No.</th><th>Date</th><th>Transaction No.</th><th>Type</th><th>Reference / Cheque</th><th>Payee / Work Type</th><th>Purpose</th><th>Deposit</th><th>Utilized</th><th class="no-print">Evidence</th></tr></thead><tbody><asp:Repeater ID="rptTransactions" runat="server"><ItemTemplate><tr><td><%# Container.ItemIndex + 1 %></td><td><%#: Eval("TransactionDate", "{0:dd MMM yyyy}") %></td><td><strong><%#: Eval("TransactionNumber") %></strong></td><td><span class='<%# "fund-type-pill type-" + Eval("TransactionType").ToString().ToLowerInvariant() %>'><%#: Eval("TransactionType") %></span></td><td><%#: Eval("ReferenceNo") %><%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("ChequeNo"))) ? "" : " / Cheque " %><%#: Eval("ChequeNo") %></td><td><%#: Eval("SourceOrPayee") %><%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("WorkType"))) ? "" : " / " %><%#: Eval("WorkType") %></td><td><%#: Eval("Purpose") %></td><td class="fund-money"><%# Convert.ToString(Eval("TransactionType")) == "Deposit" ? String.Format(System.Globalization.CultureInfo.InvariantCulture, "Rs. {0:N2}", Eval("Amount")) : "-" %></td><td class="fund-money"><%# Convert.ToString(Eval("TransactionType")) == "Utilization" ? String.Format(System.Globalization.CultureInfo.InvariantCulture, "Rs. {0:N2}", Eval("Amount")) : "-" %></td><td class="no-print"><a class="fund-evidence-link" href='<%# "FundEvidence.aspx?transaction=" + Eval("FundTransactionID") %>'><%# Convert.ToInt32(Eval("DocumentCount")) > 0 ? "View " + Eval("DocumentCount") + " file(s)" : "No files" %></a></td></tr></ItemTemplate></asp:Repeater></tbody></table><asp:Panel ID="pnlNoTransactions" runat="server" Visible="false" CssClass="fund-empty-state">No FTF bank transactions were found in the selected period.</asp:Panel></div>
                </section>
                <footer class="fund-print-footer"><span>Prepared from Digital School Manager</span><span>Printed <%= DateTime.Now.ToString("dd MMM yyyy, hh:mm tt") %></span></footer>
            </div>
        </section>
    </div>
</asp:Content>
