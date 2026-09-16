<%@ Page Title="Annual Budget Development" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="AnnualBudget.aspx.cs" Inherits="DigitalSchoolManager.AnnualBudget" %>
<asp:Content ID="AnnualBudgetHead" ContentPlaceHolderID="head" runat="server" />
<asp:Content ID="AnnualBudgetContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="budget-page">
        <header class="budget-hero budget-hero-development">
            <div class="budget-hero-brand"><img src="<%= ResolveUrl("~/images/SchoolLogo.png") %>" alt="School logo" /><div><span>July to June financial planning</span><h1>Annual Budget Development</h1><p>Create a permanent budget file, load all sanctioned posts and staff, enter pay and allowances, then produce the official 414 forms.</p></div></div>
            <div class="budget-hero-actions"><a class="budget-hero-link" href="BudgetSetup.aspx">Master Setup</a><asp:HyperLink ID="lnkReports" runat="server" NavigateUrl="~/BudgetReports.aspx" CssClass="budget-hero-link secondary">Budget Reports</asp:HyperLink></div>
        </header>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="budget-message"><asp:Label ID="lblMessage" runat="server" /></asp:Panel>

        <section class="budget-card budget-new-file">
            <div class="budget-card-heading"><div><span>Permanent annual file</span><h2>Create or Open a Budget</h2><p>A budget starts on 1 July and ends on 30 June of the following year.</p></div><span class="budget-chip">Saved in SQL Server</span></div>
            <div class="budget-open-row">
                <div class="budget-field budget-field-grow"><label for="<%= ddlBudgets.ClientID %>">Existing budget file</label><asp:DropDownList ID="ddlBudgets" runat="server" CssClass="budget-control" AutoPostBack="true" OnSelectedIndexChanged="ddlBudgets_SelectedIndexChanged" /></div>
                <asp:Button ID="btnRefreshBudgets" runat="server" Text="Refresh" CssClass="budget-button budget-button-muted" CausesValidation="false" OnClick="btnRefreshBudgets_Click" />
            </div>
            <details class="budget-create-panel" open>
                <summary>Create a new annual budget</summary>
                <div class="budget-form-grid">
                    <div class="budget-field"><label for="<%= txtFiscalStartYear.ClientID %>">Fiscal start year</label><asp:TextBox ID="txtFiscalStartYear" runat="server" CssClass="budget-control" TextMode="Number" min="2000" max="2200" /></div>
                    <div class="budget-field"><label for="<%= txtBudgetName.ClientID %>">Budget file name</label><asp:TextBox ID="txtBudgetName" runat="server" CssClass="budget-control" MaxLength="120" /></div>
                    <div class="budget-field budget-field-wide"><label for="<%= txtSchoolName.ClientID %>">Name of D.D.O. / School</label><asp:TextBox ID="txtSchoolName" runat="server" CssClass="budget-control" MaxLength="250" /></div>
                    <div class="budget-field"><label for="<%= txtLocalGovernment.ClientID %>">Local government</label><asp:TextBox ID="txtLocalGovernment" runat="server" CssClass="budget-control" MaxLength="200" /></div>
                    <div class="budget-field"><label for="<%= txtDemandName.ClientID %>">Demand</label><asp:TextBox ID="txtDemandName" runat="server" CssClass="budget-control" MaxLength="200" /></div>
                    <div class="budget-field"><label for="<%= txtGrantNo.ClientID %>">Grant number</label><asp:TextBox ID="txtGrantNo" runat="server" CssClass="budget-control" MaxLength="30" /></div>
                    <div class="budget-field"><label for="<%= txtCostCenter.ClientID %>">Cost center code</label><asp:TextBox ID="txtCostCenter" runat="server" CssClass="budget-control" MaxLength="30" /></div>
                    <div class="budget-field"><label for="<%= txtFunctionCode.ClientID %>">Detailed function code</label><asp:TextBox ID="txtFunctionCode" runat="server" CssClass="budget-control" MaxLength="30" /></div>
                    <div class="budget-field"><label for="<%= txtFunctionName.ClientID %>">Detailed function</label><asp:TextBox ID="txtFunctionName" runat="server" CssClass="budget-control" MaxLength="200" /></div>
                </div>
                <asp:Button ID="btnCreateBudget" runat="server" Text="Create Budget and Load Staff" CssClass="budget-button" OnClick="btnCreateBudget_Click" />
            </details>
        </section>

        <asp:Panel ID="pnlWorkspace" runat="server" Visible="false">
            <section class="budget-summary-band">
                <div><span>Active budget</span><strong><asp:Label ID="lblActiveBudget" runat="server" /></strong><small><asp:Label ID="lblBudgetStatus" runat="server" /></small></div>
                <div><span>Annual Basic Pay</span><strong><asp:Label ID="lblPayTotal" runat="server" /></strong><small>Basic pay plus increments</small></div>
                <div><span>Annual Allowances</span><strong><asp:Label ID="lblAllowanceTotal" runat="server" /></strong><small>All active allowance heads</small></div>
                <div><span>Grand Estimate</span><strong><asp:Label ID="lblGrandTotal" runat="server" /></strong><small>Pay, allowances and other objects</small></div>
            </section>

            <section class="budget-card">
                <div class="budget-card-heading"><div><span>Automatically loaded establishment</span><h2>Staff, Vacancies and Basic Pay</h2><p>Teaching and non-teaching employees and vacant positions are copied into this annual file. Saved figures remain unchanged in future years.</p></div><asp:Button ID="btnSyncStaff" runat="server" Text="Synchronize Staff and Vacancies" CssClass="budget-button budget-button-muted" CausesValidation="false" OnClick="btnSyncStaff_Click" /></div>
                <div class="budget-table-wrap wide-table">
                    <asp:GridView ID="gvStaff" runat="server" AutoGenerateColumns="false" DataKeyNames="BudgetStaffLineID" CssClass="budget-data-table budget-staff-table">
                        <Columns>
                            <asp:BoundField DataField="EmployeeName" HeaderText="Officer / Official" />
                            <asp:BoundField DataField="StaffCategory" HeaderText="Category" />
                            <asp:BoundField DataField="Designation" HeaderText="Designation" />
                            <asp:BoundField DataField="PostCode" HeaderText="Post Code" />
                            <asp:BoundField DataField="BPS" HeaderText="BPS" />
                            <asp:BoundField DataField="Gender" HeaderText="Gender" />
                            <asp:TemplateField HeaderText="Basic Pay"><ItemTemplate><asp:TextBox ID="txtBasicPay" runat="server" Text='<%# Bind("BasicPay", "{0:0.00}") %>' TextMode="Number" CssClass="budget-grid-input money-input" min="0" step="0.01" /></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Increment Date"><ItemTemplate><asp:TextBox ID="txtIncrementDate" runat="server" Text='<%# Eval("IncrementDate") == DBNull.Value ? "" : Eval("IncrementDate", "{0:yyyy-MM-dd}") %>' TextMode="Date" CssClass="budget-grid-input date-input" /></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Increment Rate"><ItemTemplate><asp:TextBox ID="txtIncrementRate" runat="server" Text='<%# Bind("IncrementRate", "{0:0.00}") %>' TextMode="Number" CssClass="budget-grid-input money-input" min="0" step="0.01" /></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Recruit Next Year"><ItemTemplate><asp:CheckBox ID="chkRecruitment" runat="server" Checked='<%# Bind("RecruitmentPlanned") %>' Enabled='<%# Convert.ToBoolean(Eval("IsVacant")) %>' /></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Remarks"><ItemTemplate><asp:TextBox ID="txtRemarks" runat="server" Text='<%# Bind("Remarks") %>' CssClass="budget-grid-input remarks-input" MaxLength="300" /></ItemTemplate></asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <asp:Button ID="btnSaveStaff" runat="server" Text="Save Staff Pay Figures" CssClass="budget-button" OnClick="btnSaveStaff_Click" />
            </section>

            <section class="budget-card budget-split-card">
                <div class="budget-card-heading"><div><span>Form 414-BDO-4 (Allow)</span><h2>Monthly Allowance Entry</h2><p>Select an employee or vacant post, enter the monthly figures, and the annual provision will be calculated automatically.</p></div></div>
                <div class="budget-open-row">
                    <div class="budget-field budget-field-grow"><label for="<%= ddlAllowanceStaff.ClientID %>">Officer / official / vacancy</label><asp:DropDownList ID="ddlAllowanceStaff" runat="server" CssClass="budget-control" AutoPostBack="true" OnSelectedIndexChanged="ddlAllowanceStaff_SelectedIndexChanged" /></div>
                </div>
                <div class="budget-table-wrap compact-table">
                    <asp:GridView ID="gvStaffAllowances" runat="server" AutoGenerateColumns="false" DataKeyNames="AllowanceID" CssClass="budget-data-table">
                        <Columns>
                            <asp:BoundField DataField="AllowanceCode" HeaderText="Code" />
                            <asp:BoundField DataField="AllowanceName" HeaderText="Allowance" />
                            <asp:TemplateField HeaderText="Monthly Amount (Rs.)"><ItemTemplate><asp:TextBox ID="txtMonthlyAmount" runat="server" Text='<%# Bind("MonthlyAmount", "{0:0.00}") %>' TextMode="Number" CssClass="budget-grid-input money-input" min="0" step="0.01" /></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Annual Provision"><ItemTemplate><span class="budget-calculated"><%# String.Format(System.Globalization.CultureInfo.InvariantCulture, "Rs. {0:N2}", Convert.ToDecimal(Eval("MonthlyAmount")) * 12) %></span></ItemTemplate></asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <asp:Button ID="btnSaveAllowances" runat="server" Text="Save Selected Staff Allowances" CssClass="budget-button" OnClick="btnSaveAllowances_Click" />
            </section>

            <section class="budget-card">
                <div class="budget-card-heading"><div><span>Non-pay object classification</span><h2>Utilities, Travel, Pension and Rewards</h2><p>Enter comparison and proposed figures for the non-salary object heads shown in Summary 414.</p></div></div>
                <div class="budget-table-wrap compact-table">
                    <asp:GridView ID="gvObjects" runat="server" AutoGenerateColumns="false" DataKeyNames="BudgetObjectEntryID" CssClass="budget-data-table">
                        <Columns>
                            <asp:BoundField DataField="ObjectCode" HeaderText="Object Code" />
                            <asp:BoundField DataField="ObjectName" HeaderText="Object Head" />
                            <asp:TemplateField HeaderText="Previous Budget"><ItemTemplate><asp:TextBox ID="txtPrevious" runat="server" Text='<%# Bind("PreviousBudget", "{0:0.00}") %>' TextMode="Number" CssClass="budget-grid-input money-input" min="0" /></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Current Revised"><ItemTemplate><asp:TextBox ID="txtRevised" runat="server" Text='<%# Bind("CurrentRevised", "{0:0.00}") %>' TextMode="Number" CssClass="budget-grid-input money-input" min="0" /></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Proposed Estimate"><ItemTemplate><asp:TextBox ID="txtProposed" runat="server" Text='<%# Bind("ProposedBudget", "{0:0.00}") %>' TextMode="Number" CssClass="budget-grid-input money-input" min="0" /></ItemTemplate></asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
                <div class="budget-action-row"><asp:Button ID="btnSaveObjects" runat="server" Text="Save Other Budget Heads" CssClass="budget-button" OnClick="btnSaveObjects_Click" /><asp:Button ID="btnFinalize" runat="server" Text="Finalize Budget" CssClass="budget-button budget-button-gold" CausesValidation="false" OnClick="btnFinalize_Click" /><asp:HyperLink ID="lnkOpenReports" runat="server" CssClass="budget-button budget-button-dark" Text="Open Official Forms" /></div>
            </section>
        </asp:Panel>
    </div>
</asp:Content>
