<%@ Page Title="Government Higher Secondary School Maankot" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="PublicDashboard.aspx.cs" Inherits="DigitalSchoolManager.PublicDashboard" %>
<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server" />
<asp:Content ID="MainContentBlock" ContentPlaceHolderID="MainContent" runat="server">
<section class="public-dashboard-page">
    <asp:Panel ID="pnlStatus" runat="server" Visible="false" CssClass="module-message"><asp:Label ID="lblStatus" runat="server" /></asp:Panel>
    <header class="public-school-hero">
        <div class="public-school-hero-copy"><span class="public-kicker">KNOWLEDGE | CHARACTER | SERVICE</span><h1>Government Higher Secondary School Maankot</h1><p>Tehsil Kabirwala, District Khanewal</p><div class="public-hero-actions"><a href="#announcements">School News</a><a href="PortalLogin.aspx" class="secondary">Portal Login</a></div></div>
        <div class="public-school-emblem"><img src="images/SchoolLogo.png" alt="Government Higher Secondary School Maankot logo" /></div>
    </header>

    <div class="public-stat-grid" aria-label="School totals">
        <div><span>Students</span><strong><asp:Label ID="lblTotalStudents" runat="server" Text="-" /></strong><small>Currently enrolled</small></div>
        <div><span>Teachers</span><strong><asp:Label ID="lblTotalTeachers" runat="server" Text="-" /></strong><small>Active teaching staff</small></div>
        <div><span>Classes</span><strong><asp:Label ID="lblTotalClasses" runat="server" Text="-" /></strong><small>Academic sections</small></div>
    </div>

    <div class="public-introduction-grid">
        <article class="public-principal-card">
            <div class="public-principal-photo"><asp:Image ID="imgPrincipal" runat="server" AlternateText="School head or principal" /></div>
            <div><span class="public-kicker">MESSAGE FROM THE SCHOOL HEAD</span><h2><asp:Label ID="lblPrincipalName" runat="server" /></h2><strong><asp:Label ID="lblPrincipalDesignation" runat="server" /></strong><p><asp:Label ID="lblPrincipalMessage" runat="server" /></p></div>
        </article>
        <article class="public-about-card"><span class="public-kicker">ABOUT OUR SCHOOL</span><h2>Learning with purpose</h2><p><asp:Label ID="lblIntroduction" runat="server" /></p><div class="public-history-block"><h3>School History</h3><p><asp:Label ID="lblHistory" runat="server" /></p></div></article>
    </div>

    <section id="announcements" class="public-content-section">
        <div class="public-section-heading"><div><span class="public-kicker">NOTICE BOARD AND CAMPUS LIFE</span><h2>Announcements and School Updates</h2></div><asp:Panel ID="pnlManageContent" runat="server" Visible="false"><a class="public-manage-link" href="PublicContentManagement.aspx">Manage Public Content</a></asp:Panel></div>
        <div class="public-blog-grid"><asp:Repeater ID="rptAnnouncements" runat="server"><ItemTemplate>
            <article class='<%# Convert.ToBoolean(Eval("IsPinned")) ? "public-blog-card pinned" : "public-blog-card" %>'>
                <img src='<%# "PublicSchoolImage.ashx?announcement=" + Eval("AnnouncementID") %>' alt="Announcement cover" />
                <%# BuildAnnouncementMedia(Eval("MediaType"), Eval("MediaUrl"), Eval("MediaTitle")) %>
                <div class="public-blog-card-body"><div class="public-blog-meta"><span><%#: Eval("AnnouncementCategory") %></span><time><%# FormatAnnouncementDate(Eval("EventDate"), Eval("PublishedAtUtc")) %></time></div><h3><%#: Eval("Title") %></h3><p class="public-blog-summary"><%#: Eval("Summary") %></p><p class="public-blog-detail"><%#: Eval("AnnouncementBody") %></p></div>
            </article>
        </ItemTemplate></asp:Repeater></div>
        <asp:Panel ID="pnlNoAnnouncements" runat="server" Visible="false" CssClass="public-empty-state">No public announcements have been published yet.</asp:Panel>
    </section>

    <section class="public-content-section public-exams-section">
        <div class="public-section-heading"><div><span class="public-kicker">ACADEMIC CALENDAR</span><h2>Upcoming Examinations</h2></div></div>
        <div class="public-exam-grid"><asp:Repeater ID="rptUpcomingExams" runat="server"><ItemTemplate><article><span class="public-exam-date"><%# Eval("StartDate", "{0:dd MMM}") %></span><div><h3><%#: Eval("ExamName") %></h3><p><%# Eval("StartDate", "{0:dd MMM yyyy}") %> to <%# Eval("EndDate", "{0:dd MMM yyyy}") %></p></div></article></ItemTemplate></asp:Repeater></div>
        <asp:Panel ID="pnlNoUpcomingExams" runat="server" Visible="false" CssClass="public-empty-state">There are no upcoming examinations on the published calendar.</asp:Panel>
    </section>
</section>
</asp:Content>
