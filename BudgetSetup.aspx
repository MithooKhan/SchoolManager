<%@ Page Title="Budget Master Setup" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="BudgetSetup.aspx.cs" Inherits="DigitalSchoolManager.BudgetSetup" %>
<asp:Content ID="BudgetSetupHead" ContentPlaceHolderID="head" runat="server" />
<asp:Content ID="BudgetSetupContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="budget-page">
        <header class="budget-hero">
            <div class="budget-hero-brand"><img src="<%= ResolveUrl("~/images/SchoolLogo.png") %>" alt="School logo" /><div><span>Annual budget control centre</span><h1>Budget Master Setup</h1><p>Maintain allowance codes, official pay scales, and permanent post codes once for every future budget.</p></div></div>
            <a class="budget-hero-link" href="AnnualBudget.aspx">Open Budget Development</a>
        </header>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="budget-message"><asp:Label ID="lblMessage" runat="server" /></asp:Panel>

        <nav class="budget-step-nav" aria-label="Budget workflow">
            <a class="active" href="#allowanceSetup"><strong>01</strong><span>Allowances</span></a>
            <a href="#postSetup"><strong>02</strong><span>Post Codes</span></a>
            <a href="#payScaleSetup"><strong>03</strong><span>Pay Scales</span></a>
        </nav>

        <section id="allowanceSetup" class="budget-card">
            <div class="budget-card-heading"><div><span>Reusable salary heads</span><h2>Allowance Names and Codes</h2><p>Codes are seeded from form 414-BDO-4 (Allow) and can be maintained here.</p></div><span class="budget-chip">One-time master data</span></div>
            <div class="budget-inline-entry">
                <div class="budget-field"><label for="<%= txtAllowanceCode.ClientID %>">Allowance code</label><asp:TextBox ID="txtAllowanceCode" runat="server" CssClass="budget-control" MaxLength="25" placeholder="e.g. A01202" /></div>
                <div class="budget-field budget-field-grow"><label for="<%= txtAllowanceName.ClientID %>">Allowance name</label><asp:TextBox ID="txtAllowanceName" runat="server" CssClass="budget-control" MaxLength="160" placeholder="Official allowance description" /></div>
                <div class="budget-field"><label for="<%= txtAllowanceOrder.ClientID %>">Display order</label><asp:TextBox ID="txtAllowanceOrder" runat="server" CssClass="budget-control" TextMode="Number" Text="300" min="0" /></div>
                <asp:Button ID="btnAddAllowance" runat="server" Text="Add Allowance" CssClass="budget-button" OnClick="btnAddAllowance_Click" />
            </div>
            <div class="budget-table-wrap">
                <asp:GridView ID="gvAllowances" runat="server" AutoGenerateColumns="false" DataKeyNames="AllowanceID" CssClass="budget-data-table">
                    <Columns>
                        <asp:TemplateField HeaderText="Code"><ItemTemplate><asp:TextBox ID="txtCode" runat="server" Text='<%# Bind("AllowanceCode") %>' CssClass="budget-grid-input code-input" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Allowance Name"><ItemTemplate><asp:TextBox ID="txtName" runat="server" Text='<%# Bind("AllowanceName") %>' CssClass="budget-grid-input" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Order"><ItemTemplate><asp:TextBox ID="txtOrder" runat="server" Text='<%# Bind("SortOrder") %>' TextMode="Number" CssClass="budget-grid-input number-input" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Active"><ItemTemplate><asp:CheckBox ID="chkActive" runat="server" Checked='<%# Bind("IsActive") %>' /></ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Button ID="btnSaveAllowances" runat="server" Text="Save Allowance Master" CssClass="budget-button" OnClick="btnSaveAllowances_Click" />
        </section>

        <section id="postSetup" class="budget-card">
            <div class="budget-card-heading"><div><span>Automatic vacancy integration</span><h2>Teaching and Non-Teaching Post Codes</h2><p>All posts are loaded directly from the vacancy-position tables. Assign each post its permanent budget code.</p></div><asp:Button ID="btnRefreshPosts" runat="server" Text="Refresh Vacancies" CssClass="budget-button budget-button-muted" CausesValidation="false" OnClick="btnRefreshPosts_Click" /></div>
            <div class="budget-table-wrap">
                <asp:GridView ID="gvPosts" runat="server" AutoGenerateColumns="false" DataKeyNames="StaffCategory,SourcePostID" CssClass="budget-data-table">
                    <Columns>
                        <asp:BoundField DataField="StaffCategory" HeaderText="Category" />
                        <asp:BoundField DataField="PostName" HeaderText="Post / Designation" />
                        <asp:BoundField DataField="BPS" HeaderText="BPS" />
                        <asp:BoundField DataField="Sanctioned" HeaderText="Sanctioned" />
                        <asp:BoundField DataField="Working" HeaderText="Working" />
                        <asp:BoundField DataField="Vacant" HeaderText="Vacant" />
                        <asp:TemplateField HeaderText="Permanent Post Code"><ItemTemplate><asp:TextBox ID="txtPostCode" runat="server" Text='<%# Bind("PostCode") %>' CssClass="budget-grid-input code-input" placeholder="Assign code" /></ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Button ID="btnSavePosts" runat="server" Text="Save All Post Codes" CssClass="budget-button" OnClick="btnSavePosts_Click" />
        </section>

        <section id="payScaleSetup" class="budget-card">
            <div class="budget-card-heading"><div><span>Form 414-BM-10 reference</span><h2>Basic Pay Scales</h2><p>The sample 2024 pay-scale values are preloaded. Update them when an official revised pay scale is issued.</p></div><span class="budget-chip">BPS 1-20 loaded</span></div>
            <div class="budget-table-wrap compact-table">
                <asp:GridView ID="gvPayScales" runat="server" AutoGenerateColumns="false" DataKeyNames="BPS" CssClass="budget-data-table">
                    <Columns>
                        <asp:BoundField DataField="BPS" HeaderText="BPS" ReadOnly="true" />
                        <asp:TemplateField HeaderText="Minimum Pay"><ItemTemplate><asp:TextBox ID="txtMinimum" runat="server" Text='<%# Bind("MinimumPay", "{0:0.00}") %>' TextMode="Number" CssClass="budget-grid-input money-input" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Maximum Pay"><ItemTemplate><asp:TextBox ID="txtMaximum" runat="server" Text='<%# Bind("MaximumPay", "{0:0.00}") %>' TextMode="Number" CssClass="budget-grid-input money-input" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Annual Increment"><ItemTemplate><asp:TextBox ID="txtIncrement" runat="server" Text='<%# Bind("AnnualIncrement", "{0:0.00}") %>' TextMode="Number" CssClass="budget-grid-input money-input" /></ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <asp:Button ID="btnSavePayScales" runat="server" Text="Save Pay Scales" CssClass="budget-button" OnClick="btnSavePayScales_Click" />
        </section>
    </div>
</asp:Content>
