using System;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            try
            {
                SchoolLifecycleService.EnsureSchema();
                SystemUserSecurity.EnsureSchema();
                if (SystemUserSecurity.IsAuthenticated(Context))
                {
                    if (SystemUserSecurity.IsAdministrator(Context))
                    {
                        ShowAdministratorTransfer();
                    }
                    else
                    {
                        Response.Redirect(ResolveUrl(PortalAuthorizationService.RoleHome(Context)), false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                HideAllForms();
                ShowMessage(
                    "The login database could not be prepared automatically. Run Login_Database_Update.sql " +
                    "against SchoolDatabase, then reload this page. Details: " + ex.Message,
                    "error");
                return;
            }

            if (string.Equals(Request.QueryString["reason"], "login", StringComparison.OrdinalIgnoreCase))
            {
                ShowMessage("Please login to access the selected system page.", "info");
            }
            else if (string.Equals(Request.QueryString["restored"], "1", StringComparison.Ordinal))
            {
                ShowMessage("The database was restored successfully. Please login again.", "success");
            }
            else if (string.Equals(Request.QueryString["adminTransferred"], "1", StringComparison.Ordinal))
            {
                ShowMessage(
                    "Administrator responsibility was transferred successfully. The new administrator can now login.",
                    "success");
            }

            InitializeAuthenticationForms();
        }

        private void InitializeAuthenticationForms()
        {
            try
            {
                SystemUserSecurity.EnsureSchema();
                if (SystemUserSecurity.HasAnyUsers())
                {
                    ShowLoginForm();
                }
                else
                {
                    ShowFirstAdministratorSetup();
                }
            }
            catch (Exception ex)
            {
                HideAllForms();
                ShowMessage(
                    "The login database could not be prepared automatically. Run Login_Database_Update.sql " +
                    "against SchoolDatabase, then reload this page. Details: " + ex.Message,
                    "error");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = (txtUsername.Text ?? string.Empty).Trim();
            string password = txtPassword.Text ?? string.Empty;

            try
            {
                SystemUserSecurity.EnsureSchema();
                if (!SystemUserSecurity.HasAnyUsers())
                {
                    ShowMessage("Create the first administrator account before attempting to login.", "info");
                    ShowFirstAdministratorSetup();
                    return;
                }

                LoginResult result = SystemUserSecurity.Authenticate(username, password);
                if (!result.Success)
                {
                    ShowMessage(result.Message, "error");
                    txtPassword.Text = string.Empty;
                    return;
                }

                SystemUserSecurity.EstablishSession(Context, result, chkRememberMe.Checked);
                Response.Redirect(ResolveUrl(GetSafeReturnUrl()), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            catch (Exception ex)
            {
                ShowMessage("Login could not be completed. " + ex.Message, "error");
                txtPassword.Text = string.Empty;
            }
        }

        protected void btnCreateAdministrator_Click(object sender, EventArgs e)
        {
            int teacherId;
            if (!int.TryParse(ddlTeacher.SelectedValue, out teacherId) || teacherId <= 0)
            {
                ShowMessage("Select the teacher who will own the administrator account.", "error");
                return;
            }

            string username = (txtNewUsername.Text ?? string.Empty).Trim();
            string password = txtNewPassword.Text ?? string.Empty;
            string confirmation = txtConfirmPassword.Text ?? string.Empty;

            if (!string.Equals(password, confirmation, StringComparison.Ordinal))
            {
                ShowMessage("Password and confirmation do not match.", "error");
                return;
            }

            try
            {
                SystemUserSecurity.EnsureSchema();
                SystemUserSecurity.CreateFirstAdministrator(teacherId, username, password);

                txtUsername.Text = username;
                txtNewPassword.Text = string.Empty;
                txtConfirmPassword.Text = string.Empty;
                ShowLoginForm();
                ShowMessage(
                    "Administrator account created successfully. Use the new credentials to login.",
                    "success");
            }
            catch (Exception ex)
            {
                ShowMessage("The administrator account could not be created. " + ex.Message, "error");
                try
                {
                    if (SystemUserSecurity.HasAnyUsers())
                    {
                        ShowLoginForm();
                    }
                }
                catch
                {
                    // Keep the original account-creation error visible.
                }
            }
        }

        protected void btnShowForgotPassword_Click(object sender, EventArgs e)
        {
            ShowPasswordRecovery();
            HideMessage();
        }

        protected void btnCancelRecovery_Click(object sender, EventArgs e)
        {
            ClearRecoveryFields();
            ShowLoginForm();
            HideMessage();
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            string password = txtRecoveryPassword.Text ?? string.Empty;
            if (!string.Equals(password, txtRecoveryConfirm.Text ?? string.Empty, StringComparison.Ordinal))
            {
                ShowMessage("New password and confirmation do not match.", "error");
                return;
            }

            try
            {
                SystemUserSecurity.EnsureSchema();
                SystemUserSecurity.ResetPasswordWithTeacherIdentity(
                    txtRecoveryUsername.Text,
                    txtRecoveryCnic.Text,
                    txtRecoveryPersonalNo.Text,
                    password);

                string recoveredUsername = (txtRecoveryUsername.Text ?? string.Empty).Trim();
                ClearRecoveryFields();
                ShowLoginForm();
                txtUsername.Text = recoveredUsername;
                ShowMessage("Password reset successfully. Login with the new password.", "success");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning(
                    "[Login] Password recovery failed for {0}: {1}",
                    (txtRecoveryUsername.Text ?? string.Empty).Trim(),
                    ex.Message);
                txtRecoveryPassword.Text = string.Empty;
                txtRecoveryConfirm.Text = string.Empty;
                ShowMessage(
                    ex.Message.IndexOf("Password must", StringComparison.OrdinalIgnoreCase) >= 0
                        ? ex.Message
                        : "Password could not be reset. Verify the username, teacher CNIC, and personal number.",
                    "error");
            }
        }

        protected void btnTransferAdministrator_Click(object sender, EventArgs e)
        {
            if (!SystemUserSecurity.IsAdministrator(Context))
            {
                ShowMessage("Your administrator session has expired. Login again before transferring responsibility.", "error");
                return;
            }

            int newTeacherId;
            if (!int.TryParse(ddlNewAdministrator.SelectedValue, out newTeacherId) || newTeacherId <= 0)
            {
                ShowMessage("Select the teacher who will become the new administrator.", "error");
                return;
            }

            string newPassword = txtTransferPassword.Text ?? string.Empty;
            if (!string.Equals(newPassword, txtTransferConfirm.Text ?? string.Empty, StringComparison.Ordinal))
            {
                ShowMessage("New administrator password and confirmation do not match.", "error");
                return;
            }

            int currentUserId;
            if (!int.TryParse(Convert.ToString(Session["SystemUserID"]), out currentUserId) || currentUserId <= 0)
            {
                ShowMessage("The current administrator account could not be verified. Login again.", "error");
                return;
            }

            try
            {
                LoginResult confirmation = SystemUserSecurity.Authenticate(
                    Convert.ToString(Session["Username"]),
                    txtCurrentAdminPassword.Text ?? string.Empty);
                if (!confirmation.Success || confirmation.UserId != currentUserId)
                {
                    txtCurrentAdminPassword.Text = string.Empty;
                    ShowMessage("Current administrator password verification failed. " + confirmation.Message, "error");
                    return;
                }

                string newAdministrator = SystemUserSecurity.TransferAdministrator(
                    currentUserId,
                    newTeacherId,
                    txtTransferUsername.Text,
                    newPassword,
                    chkDeactivateCurrentAdmin.Checked);

                System.Diagnostics.Trace.TraceInformation(
                    "[Login] Administrator responsibility transferred to {0}.",
                    newAdministrator);
                SystemUserSecurity.SignOut(Context);
                Response.Redirect(ResolveUrl("~/Login.aspx?adminTransferred=1"), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[Login] Administrator transfer failed: {0}", ex.Message);
                txtTransferPassword.Text = string.Empty;
                txtTransferConfirm.Text = string.Empty;
                txtCurrentAdminPassword.Text = string.Empty;
                ShowMessage("Administrator responsibility could not be transferred. " + ex.Message, "error");
            }
        }

        private void ShowLoginForm()
        {
            pnlLoginForm.Visible = true;
            pnlFirstAdminSetup.Visible = false;
            pnlPasswordRecovery.Visible = false;
            pnlAdminTransfer.Visible = false;
            txtUsername.Focus();
        }

        private void ShowFirstAdministratorSetup()
        {
            pnlLoginForm.Visible = false;
            pnlFirstAdminSetup.Visible = true;
            pnlPasswordRecovery.Visible = false;
            pnlAdminTransfer.Visible = false;
            LoadTeachers();
            txtNewUsername.Focus();
        }

        private void ShowPasswordRecovery()
        {
            pnlLoginForm.Visible = false;
            pnlFirstAdminSetup.Visible = false;
            pnlPasswordRecovery.Visible = true;
            pnlAdminTransfer.Visible = false;
            txtRecoveryUsername.Text = txtUsername.Text;
            txtRecoveryUsername.Focus();
        }

        private void ShowAdministratorTransfer()
        {
            pnlLoginForm.Visible = false;
            pnlFirstAdminSetup.Visible = false;
            pnlPasswordRecovery.Visible = false;
            pnlAdminTransfer.Visible = true;

            lblCurrentAdministrator.Text =
                Convert.ToString(Session["DisplayName"]) + " (" +
                Convert.ToString(Session["Username"]) + ")";
            LoadAdministratorTransferTeachers();
            ddlNewAdministrator.Focus();
        }

        private void LoadTeachers()
        {
            DataTable teachers = SystemUserSecurity.LoadAvailableTeachers();
            ddlTeacher.Items.Clear();
            ddlTeacher.Items.Add(new ListItem("Select teacher", "0"));

            foreach (DataRow row in teachers.Rows)
            {
                ddlTeacher.Items.Add(new ListItem(
                    Convert.ToString(row["TeacherDisplay"]),
                    Convert.ToString(row["TeacherID"])));
            }

            btnCreateAdministrator.Enabled = teachers.Rows.Count > 0;
            if (teachers.Rows.Count == 0)
            {
                ShowMessage(
                    "No teacher record is available. Register a teacher in the Teachers table before creating the administrator account.",
                    "error");
            }
        }

        private void LoadAdministratorTransferTeachers()
        {
            int currentTeacherId;
            if (!int.TryParse(Convert.ToString(Session["TeacherID"]), out currentTeacherId))
            {
                throw new InvalidOperationException("The current administrator teacher record is unavailable.");
            }

            DataTable teachers = SystemUserSecurity.LoadTeachersForAdminTransfer(currentTeacherId);
            ddlNewAdministrator.Items.Clear();
            ddlNewAdministrator.Items.Add(new ListItem("Select new administrator teacher", "0"));

            foreach (DataRow row in teachers.Rows)
            {
                ddlNewAdministrator.Items.Add(new ListItem(
                    Convert.ToString(row["TeacherDisplay"]),
                    Convert.ToString(row["TeacherID"])));
            }

            btnTransferAdministrator.Enabled = teachers.Rows.Count > 0;
            if (teachers.Rows.Count == 0)
            {
                ShowMessage(
                    "No other teacher record is available. Register the replacement teacher before transferring administrator responsibility.",
                    "error");
            }
        }

        private void HideAllForms()
        {
            pnlLoginForm.Visible = false;
            pnlFirstAdminSetup.Visible = false;
            pnlPasswordRecovery.Visible = false;
            pnlAdminTransfer.Visible = false;
        }

        private void ClearRecoveryFields()
        {
            txtRecoveryUsername.Text = string.Empty;
            txtRecoveryCnic.Text = string.Empty;
            txtRecoveryPersonalNo.Text = string.Empty;
            txtRecoveryPassword.Text = string.Empty;
            txtRecoveryConfirm.Text = string.Empty;
        }

        private string GetSafeReturnUrl()
        {
            string returnUrl = Request.QueryString["ReturnUrl"];
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return "~/AdminDashboard.aspx";
            }

            returnUrl = returnUrl.Trim();
            if (!VirtualPathUtility.IsAppRelative(returnUrl) ||
                returnUrl.IndexOf("://", StringComparison.Ordinal) >= 0 ||
                returnUrl.IndexOf('\r') >= 0 ||
                returnUrl.IndexOf('\n') >= 0)
            {
                return "~/AdminDashboard.aspx";
            }

            return returnUrl;
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "system-message system-message-" + type;
            lblMessage.Text = message;
        }

        private void HideMessage()
        {
            pnlMessage.Visible = false;
            lblMessage.Text = string.Empty;
        }
    }
}
