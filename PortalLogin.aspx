<%@ Page Title="Teacher Portal Login" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="PortalLogin.aspx.cs" Inherits="DigitalSchoolManager.PortalLogin" %>
<asp:Content ID="PortalLoginHead" ContentPlaceHolderID="head" runat="server" />
<asp:Content ID="PortalLoginBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="portal-login-shell">
        <section class="portal-login-intro">
            <span class="portal-kicker">Class Incharge Workspace</span>
            <h1>Teacher Portal</h1>
            <p>Use your active teacher record to manage only the class entrusted to you.</p>
            <div class="portal-login-feature-grid">
                <article><strong>Attendance</strong><span>Mark the assigned class safely</span></article>
                <article><strong>Results</strong><span>Record examination performance</span></article>
                <article><strong>Fee Collection</strong><span>Prepare student vouchers</span></article>
                <article><strong>Teachers Diary</strong><span>Publish weekly learning plans</span></article>
            </div>
        </section>
        <section class="portal-login-card">
            <div class="portal-login-logo"><img src="images/SchoolLogo.png" alt="School logo" /></div>
            <span class="portal-kicker">Secure sign in</span>
            <h2>Welcome, Teacher</h2>
            <p>Username is your cell number. Password is your personal number saved in the Teachers table.</p>
            <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="portal-message">
                <asp:Label ID="lblMessage" runat="server" />
            </asp:Panel>
            <label for="<%= txtCellNumber.ClientID %>">Cell number</label>
            <asp:TextBox ID="txtCellNumber" runat="server" CssClass="portal-input" MaxLength="50"
                autocomplete="username" placeholder="e.g. 03001234567" />
            <label for="<%= txtPersonalNumber.ClientID %>">Personal number</label>
            <asp:TextBox ID="txtPersonalNumber" runat="server" CssClass="portal-input" MaxLength="80"
                TextMode="Password" autocomplete="current-password" placeholder="Teacher personal number" />
            <label class="portal-check"><asp:CheckBox ID="chkRemember" runat="server" /> Keep me signed in on this trusted device</label>
            <asp:Button ID="btnLogin" runat="server" Text="Open Teacher Dashboard" CssClass="portal-primary-button"
                OnClick="btnLogin_Click" />
            <div class="portal-login-links">
                <a href="StudentProfile.aspx">Open Student Profile and Diary</a>
                <a href="Login.aspx">Administrator login</a>
            </div>
        </section>
    </div>
</asp:Content>
