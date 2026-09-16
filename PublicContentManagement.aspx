<%@ Page Title="Public School Content" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="PublicContentManagement.aspx.cs" Inherits="DigitalSchoolManager.PublicContentManagement" %>
<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server" />
<asp:Content ID="MainContentBlock" ContentPlaceHolderID="MainContent" runat="server">
<section class="public-content-management-page resource-page">
    <header class="resource-hero"><div><span class="module-kicker">PUBLIC COMMUNICATIONS</span><h1>School Profile and Announcements</h1><p>Control the information, school-head message and blog-style updates displayed on the public dashboard.</p></div><div class="resource-hero-badge"><strong>Public view</strong><a href="PublicDashboard.aspx">Open Public Dashboard</a></div></header>
    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="module-message"><asp:Label ID="lblMessage" runat="server" /></asp:Panel>
    <div class="public-management-grid">
        <article class="module-card">
            <div class="module-card-head"><div><span class="module-kicker">SCHOOL PROFILE</span><h2>Principal and institutional information</h2></div></div>
            <div class="module-form-grid two-column">
                <label>School head / principal record<asp:DropDownList ID="ddlPrincipalTeacher" runat="server" CssClass="module-input" /></label><label>Display name override<asp:TextBox ID="txtPrincipalName" runat="server" CssClass="module-input" placeholder="Leave blank to use teacher name" /></label>
                <label>Designation override<asp:TextBox ID="txtPrincipalDesignation" runat="server" CssClass="module-input" placeholder="Principal / Headmaster" /></label><label class="span-two">Principal message<asp:TextBox ID="txtPrincipalMessage" runat="server" CssClass="module-input" TextMode="MultiLine" Rows="4" /></label>
                <label class="span-two">Brief school introduction<asp:TextBox ID="txtSchoolIntroduction" runat="server" CssClass="module-input" TextMode="MultiLine" Rows="5" /></label><label class="span-two">School history<asp:TextBox ID="txtSchoolHistory" runat="server" CssClass="module-input" TextMode="MultiLine" Rows="6" /></label>
            </div>
            <div class="module-actions"><asp:Button ID="btnSaveProfile" runat="server" Text="Save Public Profile" CssClass="module-button" OnClick="btnSaveProfile_Click" /></div>
        </article>

        <article class="module-card">
            <div class="module-card-head"><div><span class="module-kicker">NEWS EDITOR</span><h2><asp:Label ID="lblAnnouncementEditorTitle" runat="server" Text="Create announcement" /></h2></div><asp:Button ID="btnNewAnnouncement" runat="server" Text="New Post" CssClass="module-button secondary" OnClick="btnNewAnnouncement_Click" CausesValidation="false" /></div>
            <asp:HiddenField ID="hidAnnouncementID" runat="server" />
            <div class="module-form-grid two-column">
                <label>Title<asp:TextBox ID="txtAnnouncementTitle" runat="server" CssClass="module-input" /></label><label>Category<asp:DropDownList ID="ddlAnnouncementCategory" runat="server" CssClass="module-input"><asp:ListItem>Announcement</asp:ListItem><asp:ListItem>Admissions</asp:ListItem><asp:ListItem>Examinations</asp:ListItem><asp:ListItem>Academic Achievement</asp:ListItem><asp:ListItem>Sports and Activities</asp:ListItem><asp:ListItem>Holiday Notice</asp:ListItem><asp:ListItem>Parent Information</asp:ListItem><asp:ListItem>School News</asp:ListItem></asp:DropDownList></label>
                <label>Event / effective date<asp:TextBox ID="txtAnnouncementDate" runat="server" CssClass="module-input" TextMode="Date" /></label><label>Cover image<asp:FileUpload ID="fuAnnouncementImage" runat="server" CssClass="module-input" accept=".jpg,.jpeg,.png,.gif,.bmp" /><small>Optional; optimized to 1 MB and stored in the database.</small></label>
                <div class="span-two public-media-editor">
                    <div class="public-media-editor-heading"><div><span class="module-kicker">EMBEDDED MEDIA</span><h3>Add a video or audio player</h3></div><span class="public-media-security">Validated links only</span></div>
                    <div class="module-form-grid three-column">
                        <label>Media type<asp:DropDownList ID="ddlMediaType" runat="server" CssClass="module-input"><asp:ListItem Value="None">No media</asp:ListItem><asp:ListItem Value="Video">Video</asp:ListItem><asp:ListItem Value="Audio">Audio</asp:ListItem></asp:DropDownList></label>
                        <label class="span-two">Public media link<asp:TextBox ID="txtMediaUrl" runat="server" CssClass="module-input" TextMode="Url" placeholder="https://www.youtube.com/watch?v=..." /><small>Supports public YouTube, Facebook, Vimeo, SoundCloud, Spotify and direct HTTPS media links.</small></label>
                        <label class="span-two">Player title<asp:TextBox ID="txtMediaTitle" runat="server" CssClass="module-input" placeholder="e.g. Annual sports day highlights" /></label>
                        <div class="public-media-guidance"><strong>Safe publishing</strong><span>Paste the media page URL. Do not paste iframe or script code. Private or restricted posts may not play publicly.</span></div>
                    </div>
                </div>
                <label class="span-two">Short summary<asp:TextBox ID="txtAnnouncementSummary" runat="server" CssClass="module-input" TextMode="MultiLine" Rows="3" /></label><label class="span-two">Complete details<asp:TextBox ID="txtAnnouncementBody" runat="server" CssClass="module-input" TextMode="MultiLine" Rows="7" /></label>
                <label class="check-field"><asp:CheckBox ID="chkPublished" runat="server" Text="Publish on public dashboard" /></label><label class="check-field"><asp:CheckBox ID="chkPinned" runat="server" Text="Pin as important update" /></label>
            </div>
            <div class="module-actions"><asp:Button ID="btnSaveAnnouncement" runat="server" Text="Save Announcement" CssClass="module-button" OnClick="btnSaveAnnouncement_Click" /></div>
        </article>
    </div>

    <article class="module-card resource-register">
        <div class="module-card-head"><div><span class="module-kicker">CONTENT LIBRARY</span><h2>Saved announcements and updates</h2></div></div>
        <div class="resource-filters"><asp:TextBox ID="txtSearch" runat="server" CssClass="module-input" placeholder="Search title, category or summary" /><asp:Button ID="btnSearch" runat="server" Text="Search Posts" CssClass="module-button" OnClick="btnSearch_Click" CausesValidation="false" /></div>
        <div class="module-table-wrap"><asp:GridView ID="gvAnnouncements" runat="server" AutoGenerateColumns="false" CssClass="module-table" EmptyDataText="No announcements have been created." OnRowCommand="gvAnnouncements_RowCommand"><Columns>
            <asp:BoundField DataField="Title" HeaderText="Title" /><asp:BoundField DataField="AnnouncementCategory" HeaderText="Category" /><asp:BoundField DataField="EventDate" HeaderText="Event Date" DataFormatString="{0:dd MMM yyyy}" /><asp:TemplateField HeaderText="Media"><ItemTemplate><span class='<%# string.IsNullOrWhiteSpace(Convert.ToString(Eval("MediaType"))) ? "public-media-badge none" : "public-media-badge" %>'><%#: string.IsNullOrWhiteSpace(Convert.ToString(Eval("MediaProvider"))) ? "None" : Eval("MediaProvider") %></span></ItemTemplate></asp:TemplateField><asp:CheckBoxField DataField="IsPublished" HeaderText="Published" /><asp:CheckBoxField DataField="IsPinned" HeaderText="Pinned" />
            <asp:TemplateField HeaderText="Actions"><ItemTemplate><asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CommandName="EditAnnouncement" CommandArgument='<%# Eval("AnnouncementID") %>' CssClass="table-action table-action-edit" CausesValidation="false" /><asp:LinkButton ID="btnToggle" runat="server" Text='<%# Convert.ToBoolean(Eval("IsPublished")) ? "Unpublish" : "Publish" %>' CommandName="ToggleAnnouncement" CommandArgument='<%# Eval("AnnouncementID") + "|" + Eval("IsPublished") %>' CssClass="table-action table-action-view" CausesValidation="false" /></ItemTemplate></asp:TemplateField>
        </Columns></asp:GridView></div>
    </article>
</section>
</asp:Content>
