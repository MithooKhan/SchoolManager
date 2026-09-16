<%@ Page Title="NSB Fund Management" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="NSBFundManagement.aspx.cs" Inherits="DigitalSchoolManager.NSBFundManagement" %>

<asp:Content ID="NSBFundContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="fund-page fund-page-nsb">
        <section class="fund-hero" aria-labelledby="nsbPageTitle">
            <div class="fund-hero-brand">
                <img src="<%= ResolveUrl("~/images/SchoolLogo.png") %>" alt="School logo" />
                <div>
                    <span>Funds and Accounts</span>
                    <h1 id="nsbPageTitle">Non-Salary Budget Management</h1>
                    <p>Record every NSB receipt and utilization with a clear, auditable evidence trail.</p>
                </div>
            </div>
            <div class="fund-hero-balance">
                <span>Current NSB Balance</span>
                <strong><asp:Label ID="lblNsbHeroBalance" runat="server" Text="Rs. 0.00" /></strong>
                <small>Receipts less approved utilization</small>
            </div>
        </section>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="fund-message" role="status" aria-live="polite">
            <asp:Label ID="lblMessage" runat="server" />
        </asp:Panel>

        <section class="fund-bank-account-card" aria-labelledby="nsbBankAccountTitle">
            <div class="fund-bank-account-heading"><div><span>Official bank profile</span><h2 id="nsbBankAccountTitle">NSB Bank Account Details</h2><p>This verified account information remains visible with the NSB workspace and statements.</p></div><asp:Button ID="btnEditBankAccount" runat="server" Text="Add or Edit Details" CssClass="fund-button fund-button-secondary no-print" CausesValidation="false" OnClick="btnEditBankAccount_Click" /></div>
            <div class="fund-bank-account-display"><div><span>Bank name</span><strong><asp:Label ID="lblBankName" runat="server" Text="Not configured" /></strong></div><div><span>Branch code</span><strong><asp:Label ID="lblBranchCode" runat="server" Text="-" /></strong></div><div><span>Branch address</span><strong><asp:Label ID="lblBranchAddress" runat="server" Text="-" /></strong></div><div><span>Account No. / IBAN</span><strong class="fund-iban"><asp:Label ID="lblAccountIBAN" runat="server" Text="-" /></strong></div></div>
            <asp:Panel ID="pnlBankAccountEdit" runat="server" Visible="false" CssClass="fund-bank-account-editor no-print">
                <div class="fund-field"><label for="<%= txtBankName.ClientID %>">Bank name</label><asp:TextBox ID="txtBankName" runat="server" CssClass="fund-control" MaxLength="150" /></div>
                <div class="fund-field"><label for="<%= txtBranchCode.ClientID %>">Branch code</label><asp:TextBox ID="txtBranchCode" runat="server" CssClass="fund-control" MaxLength="50" /></div>
                <div class="fund-field fund-field-wide"><label for="<%= txtBranchAddress.ClientID %>">Branch address</label><asp:TextBox ID="txtBranchAddress" runat="server" CssClass="fund-control" MaxLength="300" /></div>
                <div class="fund-field fund-field-wide"><label for="<%= txtAccountIBAN.ClientID %>">Account No. / IBAN</label><asp:TextBox ID="txtAccountIBAN" runat="server" CssClass="fund-control" MaxLength="50" /></div>
                <div class="fund-bank-account-actions"><asp:Button ID="btnSaveBankAccount" runat="server" Text="Save Account Details" CssClass="fund-button fund-button-primary" OnClick="btnSaveBankAccount_Click" /><asp:Button ID="btnCancelBankAccount" runat="server" Text="Cancel" CssClass="fund-button fund-button-secondary" CausesValidation="false" OnClick="btnCancelBankAccount_Click" /></div>
            </asp:Panel>
        </section>

        <section class="fund-summary-grid" aria-label="NSB financial summary">
            <article class="fund-summary-card fund-summary-credit"><span>Total NSB Received</span><strong><asp:Label ID="lblNsbReceived" runat="server" /></strong><small>All receipt entries</small></article>
            <article class="fund-summary-card fund-summary-debit"><span>Total NSB Utilized</span><strong><asp:Label ID="lblNsbUtilized" runat="server" /></strong><small>Approved expenses</small></article>
            <article class="fund-summary-card fund-summary-balance"><span>Available Balance</span><strong><asp:Label ID="lblNsbBalance" runat="server" /></strong><small>Available for future work</small></article>
        </section>

        <section class="fund-entry-grid">
            <article class="fund-entry-card">
                <header class="fund-card-heading">
                    <span class="fund-card-number">01</span>
                    <div><span>Incoming allocation</span><h2>Record NSB Receipt</h2><p>Register a grant, allocation, or other NSB receipt.</p></div>
                </header>
                <div class="fund-form-grid">
                    <div class="fund-field"><label for="<%= txtReceiptDate.ClientID %>">Receipt date</label><asp:TextBox ID="txtReceiptDate" runat="server" TextMode="Date" CssClass="fund-control" /></div>
                    <div class="fund-field"><label for="<%= txtReceiptAmount.ClientID %>">Amount received (Rs.)</label><asp:TextBox ID="txtReceiptAmount" runat="server" TextMode="Number" CssClass="fund-control" min="0.01" step="0.01" placeholder="0.00" /></div>
                    <div class="fund-field fund-field-wide"><label for="<%= txtReceiptSource.ClientID %>">Issuing authority / source</label><asp:TextBox ID="txtReceiptSource" runat="server" CssClass="fund-control" MaxLength="200" placeholder="e.g. School Education Department" /></div>
                    <div class="fund-field"><label for="<%= txtReceiptReference.ClientID %>">Advice / reference number</label><asp:TextBox ID="txtReceiptReference" runat="server" CssClass="fund-control" MaxLength="100" placeholder="Official reference" /></div>
                    <div class="fund-field"><label for="<%= fuReceiptEvidence.ClientID %>">Receipt / advice image</label><asp:FileUpload ID="fuReceiptEvidence" runat="server" CssClass="fund-file-control" AllowMultiple="true" accept="image/jpeg,image/png,image/gif,image/bmp" /><small>Up to 4 images; each is automatically reduced below 1 MB.</small></div>
                    <div class="fund-field fund-field-wide"><label for="<%= txtReceiptPurpose.ClientID %>">Allocation details</label><asp:TextBox ID="txtReceiptPurpose" runat="server" CssClass="fund-control" TextMode="MultiLine" Rows="3" MaxLength="500" placeholder="Purpose, sanction details, or funding period" /></div>
                    <div class="fund-field fund-field-wide"><label for="<%= txtReceiptRemarks.ClientID %>">Remarks</label><asp:TextBox ID="txtReceiptRemarks" runat="server" CssClass="fund-control" MaxLength="500" placeholder="Optional notes" /></div>
                </div>
                <asp:Button ID="btnSaveReceipt" runat="server" Text="Save NSB Receipt" CssClass="fund-button fund-button-primary" OnClick="btnSaveReceipt_Click" />
            </article>

            <article class="fund-entry-card">
                <header class="fund-card-heading">
                    <span class="fund-card-number">02</span>
                    <div><span>Budget utilization</span><h2>Record NSB Expense</h2><p>Save the cheque, purpose, vendor, and supporting receipts.</p></div>
                </header>
                <div class="fund-form-grid">
                    <div class="fund-field"><label for="<%= txtUtilizationDate.ClientID %>">Utilization date</label><asp:TextBox ID="txtUtilizationDate" runat="server" TextMode="Date" CssClass="fund-control" /></div>
                    <div class="fund-field"><label for="<%= txtUtilizationAmount.ClientID %>">Amount utilized (Rs.)</label><asp:TextBox ID="txtUtilizationAmount" runat="server" TextMode="Number" CssClass="fund-control" min="0.01" step="0.01" placeholder="0.00" /></div>
                    <div class="fund-field"><label for="<%= txtChequeNo.ClientID %>">Cheque number</label><asp:TextBox ID="txtChequeNo" runat="server" CssClass="fund-control" MaxLength="100" placeholder="Required" /></div>
                    <div class="fund-field"><label for="<%= ddlWorkType.ClientID %>">Work / expense type</label><asp:DropDownList ID="ddlWorkType" runat="server" CssClass="fund-control"><asp:ListItem Text="Utility Bills" /><asp:ListItem Text="Stationery" /><asp:ListItem Text="Repair Work" /><asp:ListItem Text="Furniture and Equipment" /><asp:ListItem Text="School Improvement" /><asp:ListItem Text="Other" /></asp:DropDownList></div>
                    <div class="fund-field fund-field-wide"><label for="<%= txtPayee.ClientID %>">Payee / vendor</label><asp:TextBox ID="txtPayee" runat="server" CssClass="fund-control" MaxLength="200" placeholder="Person, firm, or service provider" /></div>
                    <div class="fund-field fund-field-wide"><label for="<%= txtUtilizationPurpose.ClientID %>">Purpose of utilization</label><asp:TextBox ID="txtUtilizationPurpose" runat="server" CssClass="fund-control" TextMode="MultiLine" Rows="3" MaxLength="500" placeholder="Describe the work, goods, or service paid for" /></div>
                    <div class="fund-field"><label for="<%= fuChequeImage.ClientID %>">Cheque image</label><asp:FileUpload ID="fuChequeImage" runat="server" CssClass="fund-file-control" accept="image/jpeg,image/png,image/gif,image/bmp" /><small>Required. Automatically optimized below 1 MB.</small></div>
                    <div class="fund-field"><label for="<%= fuExpenseReceipts.ClientID %>">Trader receipts / invoices</label><asp:FileUpload ID="fuExpenseReceipts" runat="server" CssClass="fund-file-control" AllowMultiple="true" accept="image/jpeg,image/png,image/gif,image/bmp" /><small>Attach up to 8 receipt images.</small></div>
                    <div class="fund-field fund-field-wide"><label for="<%= txtUtilizationRemarks.ClientID %>">Remarks</label><asp:TextBox ID="txtUtilizationRemarks" runat="server" CssClass="fund-control" MaxLength="500" placeholder="Optional approval or procurement notes" /></div>
                </div>
                <asp:Button ID="btnSaveUtilization" runat="server" Text="Save NSB Utilization" CssClass="fund-button fund-button-danger" OnClick="btnSaveUtilization_Click" />
            </article>
        </section>

        <section class="fund-statement-card" aria-labelledby="nsbStatementTitle">
            <div class="fund-statement-toolbar no-print">
                <div><span>Audit and reporting</span><h2 id="nsbStatementTitle">NSB Fund Statement</h2></div>
                <div class="fund-filter-row">
                    <div class="fund-field"><label for="<%= ddlStatementPeriod.ClientID %>">Period</label><asp:DropDownList ID="ddlStatementPeriod" runat="server" CssClass="fund-control" AutoPostBack="true" OnSelectedIndexChanged="ddlStatementPeriod_SelectedIndexChanged"><asp:ListItem Value="current">Current month</asp:ListItem><asp:ListItem Value="last">Last month</asp:ListItem><asp:ListItem Value="six">Previous 6 months</asp:ListItem><asp:ListItem Value="year">Previous 12 months</asp:ListItem><asp:ListItem Value="custom">Custom period</asp:ListItem></asp:DropDownList></div>
                    <div class="fund-field"><label for="<%= txtFromDate.ClientID %>">From</label><asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" CssClass="fund-control" /></div>
                    <div class="fund-field"><label for="<%= txtToDate.ClientID %>">To</label><asp:TextBox ID="txtToDate" runat="server" TextMode="Date" CssClass="fund-control" /></div>
                    <asp:Button ID="btnLoadStatement" runat="server" Text="Load Statement" CssClass="fund-button fund-button-secondary" CausesValidation="false" OnClick="btnLoadStatement_Click" />
                    <button type="button" class="fund-button fund-button-print" onclick="window.print();">Print Statement</button>
                </div>
            </div>

            <div id="nsbStatementPrintArea" class="fund-print-area">
                <header class="fund-print-header">
                    <img src="<%= ResolveUrl("~/images/SchoolLogo.png") %>" alt="School logo" />
                    <div><span>Government of the Punjab</span><h2>Government Higher Secondary School Maankot</h2><strong>Non-Salary Budget Fund Statement</strong><small><asp:Label ID="lblStatementPeriod" runat="server" /></small></div>
                </header>
                <div class="fund-print-bank-line"><strong>NSB Bank Account:</strong> <asp:Label ID="lblPrintBankName" runat="server" /> | Branch: <asp:Label ID="lblPrintBranchCode" runat="server" /> | IBAN: <asp:Label ID="lblPrintAccountIBAN" runat="server" /> | <asp:Label ID="lblPrintBranchAddress" runat="server" /></div>
                <div class="fund-print-totals">
                    <div><span>Opening Balance</span><strong><asp:Label ID="lblOpeningBalance" runat="server" /></strong></div>
                    <div><span>Received in Period</span><strong><asp:Label ID="lblPeriodReceived" runat="server" /></strong></div>
                    <div><span>Utilized in Period</span><strong><asp:Label ID="lblPeriodUtilized" runat="server" /></strong></div>
                    <div><span>Closing Balance</span><strong><asp:Label ID="lblClosingBalance" runat="server" /></strong></div>
                </div>
                <div class="fund-table-wrap">
                    <table class="fund-statement-table">
                        <thead><tr><th>No.</th><th>Date</th><th>Transaction No.</th><th>Type</th><th>Source / Payee</th><th>Reference / Cheque</th><th>Purpose</th><th>Received</th><th>Utilized</th><th class="no-print">Evidence</th></tr></thead>
                        <tbody>
                            <asp:Repeater ID="rptTransactions" runat="server">
                                <ItemTemplate><tr><td><%# Container.ItemIndex + 1 %></td><td><%#: Eval("TransactionDate", "{0:dd MMM yyyy}") %></td><td><strong><%#: Eval("TransactionNumber") %></strong></td><td><span class='<%# "fund-type-pill type-" + Eval("TransactionType").ToString().ToLowerInvariant() %>'><%#: Eval("TransactionType") %></span></td><td><%#: Eval("SourceOrPayee") %></td><td><%#: Eval("ReferenceNo") %><%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("ChequeNo"))) ? "" : " / Cheque " %><%#: Eval("ChequeNo") %></td><td><%#: Eval("Purpose") %></td><td class="fund-money"><%# Convert.ToString(Eval("TransactionType")) == "Receipt" ? String.Format(System.Globalization.CultureInfo.InvariantCulture, "Rs. {0:N2}", Eval("Amount")) : "-" %></td><td class="fund-money"><%# Convert.ToString(Eval("TransactionType")) == "Utilization" ? String.Format(System.Globalization.CultureInfo.InvariantCulture, "Rs. {0:N2}", Eval("Amount")) : "-" %></td><td class="no-print"><a class="fund-evidence-link" href='<%# "FundEvidence.aspx?transaction=" + Eval("FundTransactionID") %>'><%# Convert.ToInt32(Eval("DocumentCount")) > 0 ? "View " + Eval("DocumentCount") + " file(s)" : "No files" %></a></td></tr></ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                    <asp:Panel ID="pnlNoTransactions" runat="server" Visible="false" CssClass="fund-empty-state">No NSB transactions were found in the selected period.</asp:Panel>
                </div>
                <footer class="fund-print-footer"><span>Prepared from Digital School Manager</span><span>Printed <%= DateTime.Now.ToString("dd MMM yyyy, hh:mm tt") %></span></footer>
            </div>
        </section>
    </div>
</asp:Content>
