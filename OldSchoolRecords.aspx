<%@ Page Title="Old School Records Archive" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="OldSchoolRecords.aspx.cs" Inherits="DigitalSchoolManager.OldSchoolRecords" %>
<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server" />
<asp:Content ID="MainContentBlock" ContentPlaceHolderID="MainContent" runat="server">
<section class="archive-page">
    <div class="archive-hero"><div><span class="module-kicker">DIGITAL RECORD ROOM</span><h1>Old School Records Archive</h1><p>Preserve registers, staff files, accounts, letters and official orders as indexed database records.</p></div><div class="archive-trust"><strong>Controlled digital custody</strong><span>Numbered, searchable and protected</span></div></div>
    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="module-message"><asp:Label ID="lblMessage" runat="server" /></asp:Panel>
    <div class="archive-layout">
        <article class="module-card archive-editor">
            <div class="module-card-head"><div><span class="module-kicker">CATALOGUE ENTRY</span><h2><asp:Label ID="lblEditorTitle" runat="server" Text="Create archive record" /></h2></div><asp:Button ID="btnNew" runat="server" Text="New record" CssClass="module-button secondary" OnClick="btnNew_Click" CausesValidation="false" /></div>
            <asp:HiddenField ID="hidRecordID" runat="server" />
            <div class="module-form-grid two-column">
                <label>Record category<asp:DropDownList ID="ddlCategory" runat="server" CssClass="module-input"><asp:ListItem>School Register</asp:ListItem><asp:ListItem>Teacher File</asp:ListItem><asp:ListItem>Accounts File</asp:ListItem><asp:ListItem>Letter</asp:ListItem><asp:ListItem>Order</asp:ListItem><asp:ListItem>Student Record</asp:ListItem><asp:ListItem>Examination Record</asp:ListItem><asp:ListItem>Other</asp:ListItem></asp:DropDownList></label>
                <label>Record title<asp:TextBox ID="txtTitle" runat="server" CssClass="module-input" placeholder="e.g. Admission Register No. 4" /></label>
                <label>Reference / register number<asp:TextBox ID="txtReference" runat="server" CssClass="module-input" /></label>
                <label>Register year / session<asp:TextBox ID="txtYear" runat="server" CssClass="module-input" placeholder="e.g. 1998-2002" /></label>
                <label>Record start date<asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" CssClass="module-input" /></label>
                <label>Record end date<asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="module-input" /></label>
                <label>Physical file location<asp:TextBox ID="txtLocation" runat="server" CssClass="module-input" placeholder="Room, cupboard, shelf and file number" /></label>
                <label>Confidentiality<asp:DropDownList ID="ddlConfidentiality" runat="server" CssClass="module-input"><asp:ListItem>Official</asp:ListItem><asp:ListItem>Restricted</asp:ListItem><asp:ListItem>Confidential</asp:ListItem></asp:DropDownList></label>
                <label class="span-two">Description<asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" CssClass="module-input" /></label>
                <label class="span-two">Search keywords<asp:TextBox ID="txtKeywords" runat="server" CssClass="module-input" placeholder="Names, subjects, years, departments" /></label>
                <label class="span-two archive-upload">Scanned pages or PDF files<asp:FileUpload ID="fuDocuments" runat="server" CssClass="module-input" AllowMultiple="true" accept=".jpg,.jpeg,.png,.gif,.bmp,.pdf" /><small>Select several scanned pages together. Images are optimized to 1 MB; each PDF may be up to 5 MB.</small></label>
            </div>
            <div class="module-actions"><asp:Button ID="btnSave" runat="server" Text="Save Archive Record" CssClass="module-button" OnClick="btnSave_Click" /></div>
        </article>

        <article class="module-card archive-register">
            <div class="module-card-head"><div><span class="module-kicker">PERMANENT CATALOGUE</span><h2>Search archived records</h2></div></div>
            <div class="archive-filters"><asp:TextBox ID="txtSearch" runat="server" CssClass="module-input" placeholder="Archive no., title, reference or keyword" /><asp:DropDownList ID="ddlFilterCategory" runat="server" CssClass="module-input"><asp:ListItem Value="">All categories</asp:ListItem><asp:ListItem>School Register</asp:ListItem><asp:ListItem>Teacher File</asp:ListItem><asp:ListItem>Accounts File</asp:ListItem><asp:ListItem>Letter</asp:ListItem><asp:ListItem>Order</asp:ListItem><asp:ListItem>Student Record</asp:ListItem><asp:ListItem>Examination Record</asp:ListItem><asp:ListItem>Other</asp:ListItem></asp:DropDownList><asp:Button ID="btnSearch" runat="server" Text="Search records" CssClass="module-button" OnClick="btnSearch_Click" CausesValidation="false" /></div>
            <div class="module-table-wrap"><asp:GridView ID="gvRecords" runat="server" AutoGenerateColumns="false" CssClass="module-table" EmptyDataText="No archive records match the selected filters." OnRowCommand="gvRecords_RowCommand">
                <Columns><asp:BoundField DataField="ArchiveNumber" HeaderText="Archive No." /><asp:BoundField DataField="Category" HeaderText="Category" /><asp:BoundField DataField="RecordTitle" HeaderText="Record title" /><asp:BoundField DataField="RegisterYear" HeaderText="Year" /><asp:BoundField DataField="DocumentCount" HeaderText="Files" />
                <asp:TemplateField HeaderText="Action"><ItemTemplate><asp:LinkButton ID="btnFiles" runat="server" Text="Open files" CommandName="OpenFiles" CommandArgument='<%# Eval("RecordID") %>' CssClass="table-action table-action-view" CausesValidation="false" /><asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CommandName="EditRecord" CommandArgument='<%# Eval("RecordID") %>' CssClass="table-action table-action-edit" CausesValidation="false" /></ItemTemplate></asp:TemplateField></Columns>
            </asp:GridView></div>
        </article>
    </div>

    <asp:Panel ID="pnlDocuments" runat="server" Visible="false" CssClass="module-card archive-documents">
        <div class="module-card-head"><div><span class="module-kicker">DIGITAL FILE</span><h2><asp:Label ID="lblSelectedArchive" runat="server" /></h2><p><asp:Label ID="lblSelectedTitle" runat="server" /></p></div><asp:Button ID="btnCloseDocuments" runat="server" Text="Close" CssClass="module-button secondary" OnClick="btnCloseDocuments_Click" CausesValidation="false" /></div>
        <asp:HiddenField ID="hidDocumentRecordID" runat="server" />
        <div class="module-table-wrap"><asp:GridView ID="gvDocuments" runat="server" AutoGenerateColumns="false" CssClass="module-table" EmptyDataText="No scanned pages are attached."><Columns><asp:BoundField DataField="PageNumber" HeaderText="Page" /><asp:BoundField DataField="OriginalFileName" HeaderText="File" /><asp:BoundField DataField="ContentType" HeaderText="Type" /><asp:TemplateField HeaderText="Size"><ItemTemplate><%# FormatFileSize(Eval("FileSizeBytes")) %></ItemTemplate></asp:TemplateField><asp:TemplateField HeaderText="Actions"><ItemTemplate><a class="table-action table-action-view" target="_blank" href='<%# "OldSchoolRecordDocument.ashx?id="+Eval("DocumentID") %>'>View</a><a class="table-action table-action-edit" href='<%# "OldSchoolRecordDocument.ashx?id="+Eval("DocumentID")+"&download=1" %>'>Download</a></ItemTemplate></asp:TemplateField></Columns></asp:GridView></div>
        <div class="archive-add-files"><label>Add more scanned pages / PDFs<asp:FileUpload ID="fuMoreDocuments" runat="server" CssClass="module-input" AllowMultiple="true" accept=".jpg,.jpeg,.png,.gif,.bmp,.pdf" /></label><asp:Button ID="btnAddDocuments" runat="server" Text="Upload Additional Files" CssClass="module-button" OnClick="btnAddDocuments_Click" /></div>
    </asp:Panel>
</section>
</asp:Content>
