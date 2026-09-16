<%@ Page Title="Fund Transaction Evidence" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="FundEvidence.aspx.cs" Inherits="DigitalSchoolManager.FundEvidence" %>

<asp:Content ID="FundEvidenceContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="fund-page fund-evidence-page">
        <section class="fund-hero fund-evidence-hero" aria-labelledby="evidenceTitle">
            <div class="fund-hero-brand"><img src="<%= ResolveUrl("~/images/SchoolLogo.png") %>" alt="School logo" /><div><span>Protected accounting record</span><h1 id="evidenceTitle">Fund Transaction Evidence</h1><p>Review database-backed cheque, receipt, and deposit images.</p></div></div>
            <a class="fund-back-link" href="javascript:history.back();">Back to statement</a>
        </section>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="fund-message fund-message-error"><asp:Label ID="lblMessage" runat="server" /></asp:Panel>

        <asp:Panel ID="pnlEvidence" runat="server" Visible="false">
            <section class="fund-evidence-summary">
                <div><span>Transaction No.</span><strong><asp:Label ID="lblTransactionNumber" runat="server" /></strong></div>
                <div><span>Fund / Type</span><strong><asp:Label ID="lblFundType" runat="server" /></strong></div>
                <div><span>Date</span><strong><asp:Label ID="lblTransactionDate" runat="server" /></strong></div>
                <div><span>Amount</span><strong><asp:Label ID="lblAmount" runat="server" /></strong></div>
                <div><span>Reference / Cheque</span><strong><asp:Label ID="lblReference" runat="server" /></strong></div>
                <div><span>Purpose</span><strong><asp:Label ID="lblPurpose" runat="server" /></strong></div>
            </section>

            <section class="fund-evidence-gallery" aria-label="Saved transaction evidence">
                <asp:Repeater ID="rptDocuments" runat="server">
                    <ItemTemplate>
                        <article class="fund-evidence-card">
                            <header><span><%#: Eval("DocumentType") %></span><strong><%#: Eval("OriginalFileName") %></strong></header>
                            <a href='<%# "FundDocument.ashx?id=" + Eval("FundDocumentID") %>' target="_blank" rel="noopener">
                                <img src='<%# "FundDocument.ashx?id=" + Eval("FundDocumentID") %>' alt='<%# Eval("DocumentType") + " evidence image" %>' />
                            </a>
                            <footer><span><%# String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:N0} KB", Convert.ToDecimal(Eval("FileSizeBytes")) / 1024m) %></span><a href='<%# "FundDocument.ashx?id=" + Eval("FundDocumentID") %>' target="_blank" rel="noopener">Open full image</a></footer>
                        </article>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoDocuments" runat="server" Visible="false" CssClass="fund-empty-state">No evidence images are attached to this transaction.</asp:Panel>
            </section>
        </asp:Panel>
    </div>
</asp:Content>
