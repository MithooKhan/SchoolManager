<%@ Page Title="Official Budget Forms" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="BudgetReports.aspx.cs" Inherits="DigitalSchoolManager.BudgetReports" %>
<asp:Content ID="BudgetReportHead" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function filterBudgetForms() {
            var select = document.getElementById('<%= ddlForm.ClientID %>');
            var value = select ? select.value : 'all';
            document.querySelectorAll('.official-budget-form').forEach(function (form) {
                form.style.display = value === 'all' || form.getAttribute('data-form') === value ? '' : 'none';
            });
        }
        function printBudget(allForms) {
            var select = document.getElementById('<%= ddlForm.ClientID %>');
            document.body.classList.toggle('print-all-budget-forms', !!allForms);
            if (allForms && select) { document.querySelectorAll('.official-budget-form').forEach(function (form) { form.style.display = ''; }); }
            window.print();
            document.body.classList.remove('print-all-budget-forms');
            filterBudgetForms();
        }
        document.addEventListener('DOMContentLoaded', filterBudgetForms);
    </script>
</asp:Content>
<asp:Content ID="BudgetReportContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="budget-page budget-report-page">
        <header class="budget-hero no-print">
            <div class="budget-hero-brand"><img src="<%= ResolveUrl("~/images/SchoolLogo.png") %>" alt="School logo" /><div><span>Official annual statements</span><h1>Budget Forms and PDF Print View</h1><p>Search a permanent budget file, review every required form, and print either one form or the complete budget to PDF.</p></div></div>
            <div class="budget-hero-actions"><a class="budget-hero-link" href="AnnualBudget.aspx">Budget Development</a><a class="budget-hero-link secondary" href="BudgetSetup.aspx">Master Setup</a></div>
        </header>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="budget-message no-print"><asp:Label ID="lblMessage" runat="server" /></asp:Panel>

        <section class="budget-card budget-report-toolbar no-print">
            <div class="budget-card-heading"><div><span>Permanent budget archive</span><h2>Find and Print a Budget File</h2></div><span class="budget-chip">Browser PDF ready</span></div>
            <div class="budget-report-filters">
                <div class="budget-field budget-field-grow"><label for="<%= txtSearch.ClientID %>">Search by budget name</label><asp:TextBox ID="txtSearch" runat="server" CssClass="budget-control" placeholder="e.g. Budget Estimate 2024-25" /></div>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="budget-button budget-button-muted" OnClick="btnSearch_Click" />
                <div class="budget-field budget-field-grow"><label for="<%= ddlBudgets.ClientID %>">Budget file</label><asp:DropDownList ID="ddlBudgets" runat="server" CssClass="budget-control" AutoPostBack="true" OnSelectedIndexChanged="ddlBudgets_SelectedIndexChanged" /></div>
                <div class="budget-field"><label for="<%= ddlForm.ClientID %>">Form to view</label><asp:DropDownList ID="ddlForm" runat="server" CssClass="budget-control" onchange="filterBudgetForms();"><asp:ListItem Value="all">All Official Forms</asp:ListItem><asp:ListItem>Front 414</asp:ListItem><asp:ListItem>Summary 414</asp:ListItem><asp:ListItem>414-Pay</asp:ListItem><asp:ListItem>414-BDO-3</asp:ListItem><asp:ListItem>414-BDO-4 (Allow)</asp:ListItem><asp:ListItem>414-BDC-2</asp:ListItem><asp:ListItem>414-BDC-3</asp:ListItem><asp:ListItem>414-BDC-4</asp:ListItem><asp:ListItem>414-BDC-5</asp:ListItem><asp:ListItem>414-BM-10</asp:ListItem></asp:DropDownList></div>
                <button type="button" class="budget-button budget-button-gold" onclick="printBudget(false);">Print / Save Selected PDF</button>
                <button type="button" class="budget-button budget-button-dark" onclick="printBudget(true);">Print Complete Budget PDF</button>
            </div>
            <p class="budget-print-note">In the browser print window, choose <strong>Save as PDF</strong>. Official A4 landscape pages, black text, table borders, school logo, and page breaks are applied automatically.</p>
        </section>

        <asp:Panel ID="pnlReport" runat="server" Visible="false" CssClass="budget-report-output">
            <asp:Literal ID="litReport" runat="server" />
        </asp:Panel>
    </div>
</asp:Content>
