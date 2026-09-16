<%@ Page Title="System Login" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="DigitalSchoolManager.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="system-login-page">
        <section class="system-login-shell" aria-labelledby="loginPageTitle">
            <div class="system-login-introduction">
                <span class="system-module-label">Secure Administration</span>
                <h1 id="loginPageTitle">Digital School Manager Login</h1>
                <p>Authorized school staff can securely access student, staff, attendance, timetable, examination, finance, and system-management records.</p>

                <div class="login-security-points">
                    <div>
                        <span>01</span>
                        <p><strong>Teacher-linked identity</strong>Every system account belongs to a teacher record in SchoolDatabase.</p>
                    </div>
                    <div>
                        <span>02</span>
                        <p><strong>Protected credentials</strong>Passwords are stored as salted, one-way hashes and are never saved as readable text.</p>
                    </div>
                    <div>
                        <span>03</span>
                        <p><strong>Controlled access</strong>The dashboard and menu remain visible; all management pages require a valid login.</p>
                    </div>
                </div>

                <a class="login-dashboard-link" href="<%= ResolveUrl("~/AdminDashboard.aspx") %>">Return to public dashboard</a>
            </div>

            <div class="system-login-workspace">
                <div class="login-school-mark" aria-hidden="true">
                    <span>GHSS</span>
                </div>

                <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="system-message" role="status" aria-live="polite">
                    <asp:Label ID="lblMessage" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlLoginForm" runat="server" CssClass="login-form-panel">
                    <div class="login-card-heading">
                        <span>Account access</span>
                        <h2>Sign in to continue</h2>
                        <p>Enter the username and password assigned to your teacher-linked account.</p>
                    </div>

                    <div class="login-form-fields">
                        <div class="system-field">
                            <asp:Label ID="lblUsernamePrompt" runat="server" AssociatedControlID="txtUsername"
                                Text="Username" CssClass="system-field-label" />
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="system-control"
                                MaxLength="50" autocomplete="username" placeholder="Enter username" />
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblPasswordPrompt" runat="server" AssociatedControlID="txtPassword"
                                Text="Password" CssClass="system-field-label" />
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="system-control"
                                TextMode="Password" MaxLength="128" autocomplete="current-password"
                                placeholder="Enter password" />
                        </div>
                    </div>

                    <div class="login-remember-choice">
                        <asp:CheckBox ID="chkRememberMe" runat="server"
                            Text="Keep me logged in on this device" />
                        <small>Use this option only on a trusted computer. The login is remembered for 30 days.</small>
                    </div>

                    <asp:Button ID="btnLogin" runat="server" Text="Login to System"
                        CssClass="system-button system-button-primary system-button-full"
                        OnClick="btnLogin_Click" />

                    <div class="login-secondary-actions">
                        <asp:LinkButton ID="btnShowForgotPassword" runat="server"
                            Text="Forgot password?"
                            CssClass="login-text-action"
                            CausesValidation="false"
                            OnClick="btnShowForgotPassword_Click" />
                    </div>

                    <p class="login-lock-note">Five unsuccessful attempts temporarily lock the account for 15 minutes.</p>
                </asp:Panel>

                <asp:Panel ID="pnlPasswordRecovery" runat="server" Visible="false"
                    CssClass="login-form-panel password-recovery-panel">
                    <div class="login-card-heading">
                        <span>Account recovery</span>
                        <h2>Reset forgotten password</h2>
                        <p>Verify the CNIC and personal number stored in your linked teacher record, then choose a new password.</p>
                    </div>

                    <div class="login-identity-notice">
                        <strong>Teacher identity verification</strong>
                        <span>All three account details must match. No password is ever shown or sent in readable form.</span>
                    </div>

                    <div class="login-form-fields login-form-fields-two-column">
                        <div class="system-field">
                            <asp:Label ID="lblRecoveryUsernamePrompt" runat="server" AssociatedControlID="txtRecoveryUsername"
                                Text="Username" CssClass="system-field-label" />
                            <asp:TextBox ID="txtRecoveryUsername" runat="server" CssClass="system-control"
                                MaxLength="50" autocomplete="username" placeholder="Account username" />
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblRecoveryCnicPrompt" runat="server" AssociatedControlID="txtRecoveryCnic"
                                Text="Teacher CNIC number" CssClass="system-field-label" />
                            <asp:TextBox ID="txtRecoveryCnic" runat="server" CssClass="system-control"
                                MaxLength="25" placeholder="As saved in teacher record" />
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblRecoveryPersonalNoPrompt" runat="server" AssociatedControlID="txtRecoveryPersonalNo"
                                Text="Teacher personal number" CssClass="system-field-label" />
                            <asp:TextBox ID="txtRecoveryPersonalNo" runat="server" CssClass="system-control"
                                MaxLength="40" placeholder="Official personal number" />
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblRecoveryPasswordPrompt" runat="server" AssociatedControlID="txtRecoveryPassword"
                                Text="New password" CssClass="system-field-label" />
                            <asp:TextBox ID="txtRecoveryPassword" runat="server" CssClass="system-control"
                                TextMode="Password" MaxLength="128" autocomplete="new-password"
                                placeholder="Minimum 8 characters" />
                        </div>

                        <div class="system-field system-field-span-two">
                            <asp:Label ID="lblRecoveryConfirmPrompt" runat="server" AssociatedControlID="txtRecoveryConfirm"
                                Text="Confirm new password" CssClass="system-field-label" />
                            <asp:TextBox ID="txtRecoveryConfirm" runat="server" CssClass="system-control"
                                TextMode="Password" MaxLength="128" autocomplete="new-password"
                                placeholder="Re-enter new password" />
                        </div>
                    </div>

                    <div class="login-form-buttons">
                        <asp:Button ID="btnResetPassword" runat="server" Text="Verify and Reset Password"
                            CssClass="system-button system-button-primary"
                            OnClick="btnResetPassword_Click" />
                        <asp:Button ID="btnCancelRecovery" runat="server" Text="Back to Login"
                            CssClass="system-button system-button-secondary"
                            CausesValidation="false"
                            OnClick="btnCancelRecovery_Click" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlAdminTransfer" runat="server" Visible="false"
                    CssClass="login-form-panel admin-transfer-panel">
                    <div class="login-card-heading">
                        <span>Administrator continuity</span>
                        <h2>Transfer administrator responsibility</h2>
                        <p>Assign a different teacher as the system administrator before the current administrator leaves the school.</p>
                    </div>

                    <div class="login-current-admin">
                        <span>Current administrator</span>
                        <strong><asp:Label ID="lblCurrentAdministrator" runat="server" /></strong>
                    </div>

                    <div class="login-form-fields login-form-fields-two-column">
                        <div class="system-field system-field-span-two">
                            <asp:Label ID="lblTransferTeacherPrompt" runat="server" AssociatedControlID="ddlNewAdministrator"
                                Text="New administrator teacher" CssClass="system-field-label" />
                            <asp:DropDownList ID="ddlNewAdministrator" runat="server" CssClass="system-control" />
                            <small>An existing system account will be securely updated; otherwise, a new account is created.</small>
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblTransferUsernamePrompt" runat="server" AssociatedControlID="txtTransferUsername"
                                Text="New administrator username" CssClass="system-field-label" />
                            <asp:TextBox ID="txtTransferUsername" runat="server" CssClass="system-control"
                                MaxLength="50" autocomplete="off" placeholder="Example: school.admin" />
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblTransferPasswordPrompt" runat="server" AssociatedControlID="txtTransferPassword"
                                Text="New administrator password" CssClass="system-field-label" />
                            <asp:TextBox ID="txtTransferPassword" runat="server" CssClass="system-control"
                                TextMode="Password" MaxLength="128" autocomplete="new-password"
                                placeholder="Minimum 8 characters" />
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblTransferConfirmPrompt" runat="server" AssociatedControlID="txtTransferConfirm"
                                Text="Confirm new password" CssClass="system-field-label" />
                            <asp:TextBox ID="txtTransferConfirm" runat="server" CssClass="system-control"
                                TextMode="Password" MaxLength="128" autocomplete="new-password"
                                placeholder="Re-enter new password" />
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblCurrentPasswordPrompt" runat="server" AssociatedControlID="txtCurrentAdminPassword"
                                Text="Your current password" CssClass="system-field-label" />
                            <asp:TextBox ID="txtCurrentAdminPassword" runat="server" CssClass="system-control"
                                TextMode="Password" MaxLength="128" autocomplete="current-password"
                                placeholder="Confirm this transfer" />
                        </div>
                    </div>

                    <div class="login-transfer-choice">
                        <asp:CheckBox ID="chkDeactivateCurrentAdmin" runat="server" Checked="true"
                            Text="Deactivate my current account after a successful transfer" />
                        <small>Clear this option only when the current teacher will remain an authorized standard user.</small>
                    </div>

                    <div class="login-form-buttons">
                        <asp:Button ID="btnTransferAdministrator" runat="server"
                            Text="Transfer Administrator Responsibility"
                            CssClass="system-button system-button-danger"
                            OnClientClick="return confirm('Transfer administrator responsibility to the selected teacher?');"
                            OnClick="btnTransferAdministrator_Click" />
                        <a class="system-button system-button-secondary" href="<%= ResolveUrl("~/AdminDashboard.aspx") %>">Cancel</a>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlFirstAdminSetup" runat="server" Visible="false" CssClass="login-form-panel first-admin-panel">
                    <div class="login-card-heading">
                        <span>First-time setup</span>
                        <h2>Create the administrator</h2>
                        <p>No system account exists yet. Select an existing teacher and create the first administrator login.</p>
                    </div>

                    <div class="login-form-fields">
                        <div class="system-field">
                            <asp:Label ID="lblTeacherPrompt" runat="server" AssociatedControlID="ddlTeacher"
                                Text="Teacher account owner" CssClass="system-field-label" />
                            <asp:DropDownList ID="ddlTeacher" runat="server" CssClass="system-control" />
                            <small>The selected teacher becomes the first system administrator.</small>
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblNewUsernamePrompt" runat="server" AssociatedControlID="txtNewUsername"
                                Text="Administrator username" CssClass="system-field-label" />
                            <asp:TextBox ID="txtNewUsername" runat="server" CssClass="system-control"
                                MaxLength="50" autocomplete="username" placeholder="Example: admin" />
                            <small>Use 4 to 50 letters, numbers, dot, underscore, or hyphen.</small>
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblNewPasswordPrompt" runat="server" AssociatedControlID="txtNewPassword"
                                Text="Password" CssClass="system-field-label" />
                            <asp:TextBox ID="txtNewPassword" runat="server" CssClass="system-control"
                                TextMode="Password" MaxLength="128" autocomplete="new-password"
                                placeholder="Minimum 8 characters" />
                        </div>

                        <div class="system-field">
                            <asp:Label ID="lblConfirmPasswordPrompt" runat="server" AssociatedControlID="txtConfirmPassword"
                                Text="Confirm password" CssClass="system-field-label" />
                            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="system-control"
                                TextMode="Password" MaxLength="128" autocomplete="new-password"
                                placeholder="Re-enter password" />
                        </div>
                    </div>

                    <asp:Button ID="btnCreateAdministrator" runat="server" Text="Create Administrator Account"
                        CssClass="system-button system-button-primary system-button-full"
                        OnClick="btnCreateAdministrator_Click" />
                </asp:Panel>

                <div class="login-support-note">
                    <strong>Government Higher Secondary School Maankot</strong>
                    <span>System access is monitored through the linked user account.</span>
                </div>
            </div>
        </section>
    </div>
</asp:Content>
