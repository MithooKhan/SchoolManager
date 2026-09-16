using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class DataBackup : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SystemUserSecurity.IsAdministrator(Context))
            {
                Response.Redirect(ResolveUrl("~/AdminDashboard.aspx"), true);
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    LoadEnvironmentSummary();
                    LoadBackupFiles();
                }
                catch (Exception ex)
                {
                    ShowMessage("Backup storage could not be loaded. " + ex.Message, "error");
                }
            }
        }

        protected void btnCreateBackup_Click(object sender, EventArgs e)
        {
            string backupPath = string.Empty;
            try
            {
                string databaseName = GetDatabaseName();
                string backupFolder = GetBackupFolder();
                string fileName = databaseName + "_Full_" +
                                  DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) +
                                  ".bak";
                backupPath = Path.Combine(backupFolder, fileName);

                string quotedDatabase = QuoteIdentifier(databaseName);
                string backupSql = @"
BACKUP DATABASE " + quotedDatabase + @"
TO DISK = @BackupPath
WITH COPY_ONLY,
     INIT,
     CHECKSUM,
     NAME = @BackupName,
     DESCRIPTION = N'Digital School Manager verified full backup',
     STATS = 10;";

                using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
                using (SqlCommand command = new SqlCommand(backupSql, connection))
                {
                    command.CommandTimeout = 0;
                    command.Parameters.Add("@BackupPath", SqlDbType.NVarChar, 4000).Value = backupPath;
                    command.Parameters.Add("@BackupName", SqlDbType.NVarChar, 128).Value =
                        databaseName + " Full Backup " + DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    connection.Open();
                    command.ExecuteNonQuery();
                }

                BackupHeader header = ReadAndVerifyBackup(backupPath);
                if (!string.Equals(header.DatabaseName, databaseName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "The generated backup does not identify the configured database.");
                }

                LoadEnvironmentSummary();
                LoadBackupFiles();
                ShowMessage(
                    "Database backup created and verified successfully: " + fileName,
                    "success");
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(backupPath) && File.Exists(backupPath))
                {
                    try
                    {
                        File.Delete(backupPath);
                    }
                    catch
                    {
                        // Preserve the original backup error if an incomplete file cannot be removed.
                    }
                }

                ShowMessage(BuildOperationError("create", ex), "error");
                LoadBackupFilesSafely();
            }
        }

        protected void btnRestoreBackup_Click(object sender, EventArgs e)
        {
            if (!chkConfirmRestore.Checked)
            {
                ShowMessage("Confirm that you understand the restore operation before continuing.", "error");
                return;
            }

            try
            {
                string backupPath = ResolveRestoreFile();
                string databaseName = GetDatabaseName();
                BackupHeader header = ReadAndVerifyBackup(backupPath);

                if (!string.Equals(header.DatabaseName, databaseName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Restore stopped: the selected file contains database '" +
                        header.DatabaseName + "', not the configured database '" + databaseName + "'.");
                }

                RestoreDatabase(databaseName, backupPath, header.Position);
                SqlConnection.ClearAllPools();
                SystemUserSecurity.SignOut(Context);
                Response.Redirect(ResolveUrl("~/Login.aspx?restored=1"), true);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ShowMessage(BuildOperationError("restore", ex), "error");
                LoadBackupFilesSafely();
            }
        }

        protected void btnRefreshBackups_Click(object sender, EventArgs e)
        {
            try
            {
                LoadEnvironmentSummary();
                LoadBackupFiles();
                ShowMessage("Backup file list refreshed.", "info");
            }
            catch (Exception ex)
            {
                ShowMessage("Backup file list could not be refreshed. " + ex.Message, "error");
            }
        }

        protected void gvBackups_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "DownloadBackup", StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                string filePath = ResolveStoredBackupPath(Convert.ToString(e.CommandArgument));
                string fileName = Path.GetFileName(filePath);

                Response.Clear();
                Response.BufferOutput = false;
                Response.ContentType = "application/octet-stream";
                Response.AddHeader(
                    "Content-Disposition",
                    "attachment; filename=\"" + fileName.Replace("\"", string.Empty) + "\"");
                Response.AddHeader("Content-Length", new FileInfo(filePath).Length.ToString(CultureInfo.InvariantCulture));
                Response.TransmitFile(filePath);
                Response.Flush();
                Response.End();
            }
            catch (System.Threading.ThreadAbortException)
            {
                // Response.End terminates the request after the file has been sent.
                throw;
            }
            catch (Exception ex)
            {
                ShowMessage("The backup file could not be downloaded. " + ex.Message, "error");
            }
        }

        private void LoadEnvironmentSummary()
        {
            lblDatabaseName.Text = GetDatabaseName();
            lblBackupFolder.Text = GetBackupFolder();
        }

        private void LoadBackupFiles()
        {
            string backupFolder = GetBackupFolder();
            FileInfo[] backupFiles = new DirectoryInfo(backupFolder)
                .GetFiles("*.bak", SearchOption.TopDirectoryOnly)
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToArray();

            DataTable table = new DataTable();
            table.Columns.Add("FileName", typeof(string));
            table.Columns.Add("CreatedOn", typeof(string));
            table.Columns.Add("FileSize", typeof(string));

            foreach (FileInfo file in backupFiles)
            {
                table.Rows.Add(
                    file.Name,
                    file.LastWriteTime.ToString("dd MMM yyyy, hh:mm tt", CultureInfo.InvariantCulture),
                    FormatFileSize(file.Length));
            }

            gvBackups.DataSource = table;
            gvBackups.DataBind();
            lblBackupCount.Text = backupFiles.Length.ToString(CultureInfo.InvariantCulture);
            lblLatestBackup.Text = backupFiles.Length == 0
                ? "No backup created yet"
                : "Latest: " + backupFiles[0].LastWriteTime.ToString(
                    "dd MMM yyyy, hh:mm tt",
                    CultureInfo.InvariantCulture);

            string selectedValue = ddlStoredBackups.SelectedValue;
            ddlStoredBackups.Items.Clear();
            ddlStoredBackups.Items.Add(new ListItem("Select backup file", string.Empty));
            foreach (FileInfo file in backupFiles)
            {
                ddlStoredBackups.Items.Add(new ListItem(
                    file.Name + " (" + FormatFileSize(file.Length) + ")",
                    file.Name));
            }

            if (!string.IsNullOrEmpty(selectedValue) &&
                ddlStoredBackups.Items.FindByValue(selectedValue) != null)
            {
                ddlStoredBackups.SelectedValue = selectedValue;
            }
        }

        private void LoadBackupFilesSafely()
        {
            try
            {
                LoadEnvironmentSummary();
                LoadBackupFiles();
            }
            catch
            {
                // Do not replace the operation error with a secondary list-loading error.
            }
        }

        private string ResolveRestoreFile()
        {
            if (string.Equals(rblRestoreSource.SelectedValue, "Upload", StringComparison.OrdinalIgnoreCase))
            {
                if (!fuRestoreBackup.HasFile)
                {
                    throw new InvalidOperationException("Choose a SQL Server .bak file to restore.");
                }

                if (!string.Equals(
                    Path.GetExtension(fuRestoreBackup.FileName),
                    ".bak",
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Only SQL Server .bak files can be restored.");
                }

                if (fuRestoreBackup.PostedFile.ContentLength <= 0)
                {
                    throw new InvalidOperationException("The uploaded backup file is empty.");
                }

                string originalName = Path.GetFileNameWithoutExtension(fuRestoreBackup.FileName);
                string safeName = new string(originalName
                    .Where(character => char.IsLetterOrDigit(character) || character == '-' || character == '_')
                    .Take(60)
                    .ToArray());
                if (safeName.Length == 0)
                {
                    safeName = "DatabaseBackup";
                }

                string uploadedFileName = "Imported_" +
                                          DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) +
                                          "_" + safeName + ".bak";
                string uploadedPath = Path.Combine(GetBackupFolder(), uploadedFileName);
                fuRestoreBackup.SaveAs(uploadedPath);
                return uploadedPath;
            }

            if (string.IsNullOrWhiteSpace(ddlStoredBackups.SelectedValue))
            {
                throw new InvalidOperationException("Select a stored backup file to restore.");
            }

            return ResolveStoredBackupPath(ddlStoredBackups.SelectedValue);
        }

        private BackupHeader ReadAndVerifyBackup(string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("The selected backup file was not found.", backupPath);
            }

            string masterConnectionString = GetMasterConnectionString();
            BackupHeader selectedHeader = null;

            using (SqlConnection connection = new SqlConnection(masterConnectionString))
            using (SqlCommand command = new SqlCommand(
                "RESTORE HEADERONLY FROM DISK = @BackupPath;",
                connection))
            {
                command.CommandTimeout = 0;
                command.Parameters.Add("@BackupPath", SqlDbType.NVarChar, 4000).Value = backupPath;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int backupType = Convert.ToInt32(reader["BackupType"]);
                        if (backupType != 1)
                        {
                            continue;
                        }

                        selectedHeader = new BackupHeader
                        {
                            DatabaseName = Convert.ToString(reader["DatabaseName"]),
                            Position = Convert.ToInt32(reader["Position"])
                        };
                    }
                }
            }

            if (selectedHeader == null || string.IsNullOrWhiteSpace(selectedHeader.DatabaseName))
            {
                throw new InvalidOperationException("The file does not contain a complete SQL Server database backup.");
            }

            using (SqlConnection connection = new SqlConnection(masterConnectionString))
            using (SqlCommand command = new SqlCommand(
                "RESTORE VERIFYONLY FROM DISK = @BackupPath WITH FILE = @BackupPosition;",
                connection))
            {
                command.CommandTimeout = 0;
                command.Parameters.Add("@BackupPath", SqlDbType.NVarChar, 4000).Value = backupPath;
                command.Parameters.Add("@BackupPosition", SqlDbType.Int).Value = selectedHeader.Position;
                connection.Open();
                command.ExecuteNonQuery();
            }

            return selectedHeader;
        }

        private void RestoreDatabase(
            string databaseName,
            string backupPath,
            int backupPosition)
        {
            string quotedDatabase = QuoteIdentifier(databaseName);
            string masterConnectionString = GetMasterConnectionString();
            bool singleUserModeSet = false;

            SqlConnection.ClearAllPools();
            try
            {
                using (SqlConnection connection = new SqlConnection(masterConnectionString))
                {
                    connection.Open();

                    using (SqlCommand singleUserCommand = new SqlCommand(
                        "ALTER DATABASE " + quotedDatabase +
                        " SET SINGLE_USER WITH ROLLBACK IMMEDIATE;",
                        connection))
                    {
                        singleUserCommand.CommandTimeout = 0;
                        singleUserCommand.ExecuteNonQuery();
                        singleUserModeSet = true;
                    }

                    using (SqlCommand restoreCommand = new SqlCommand(@"
RESTORE DATABASE " + quotedDatabase + @"
FROM DISK = @BackupPath
WITH FILE = @BackupPosition,
     REPLACE,
     RECOVERY,
     STATS = 10;", connection))
                    {
                        restoreCommand.CommandTimeout = 0;
                        restoreCommand.Parameters.Add("@BackupPath", SqlDbType.NVarChar, 4000).Value = backupPath;
                        restoreCommand.Parameters.Add("@BackupPosition", SqlDbType.Int).Value = backupPosition;
                        restoreCommand.ExecuteNonQuery();
                    }

                    using (SqlCommand multiUserCommand = new SqlCommand(
                        "ALTER DATABASE " + quotedDatabase + " SET MULTI_USER;",
                        connection))
                    {
                        multiUserCommand.CommandTimeout = 0;
                        multiUserCommand.ExecuteNonQuery();
                        singleUserModeSet = false;
                    }
                }
            }
            catch
            {
                if (singleUserModeSet)
                {
                    TrySetMultiUser(masterConnectionString, quotedDatabase);
                }

                throw;
            }
            finally
            {
                SqlConnection.ClearAllPools();
            }
        }

        private static void TrySetMultiUser(string masterConnectionString, string quotedDatabase)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(masterConnectionString))
                using (SqlCommand command = new SqlCommand(
                    "ALTER DATABASE " + quotedDatabase + " SET MULTI_USER WITH ROLLBACK IMMEDIATE;",
                    connection))
                {
                    command.CommandTimeout = 60;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                // The original restore error is more useful; SQL Server can be returned
                // to MULTI_USER manually if the recovery command also fails.
            }
        }

        private string GetBackupFolder()
        {
            string configuredFolder = ConfigurationManager.AppSettings["DatabaseBackupFolder"];
            string folder;

            if (string.IsNullOrWhiteSpace(configuredFolder) || configuredFolder.StartsWith("~/", StringComparison.Ordinal))
            {
                folder = Server.MapPath(string.IsNullOrWhiteSpace(configuredFolder)
                    ? "~/App_Data/DatabaseBackups"
                    : configuredFolder);
            }
            else
            {
                folder = Path.GetFullPath(Environment.ExpandEnvironmentVariables(configuredFolder));
            }

            Directory.CreateDirectory(folder);
            return Path.GetFullPath(folder);
        }

        private string ResolveStoredBackupPath(string fileName)
        {
            string safeFileName = Path.GetFileName(fileName ?? string.Empty);
            if (!string.Equals(safeFileName, fileName, StringComparison.Ordinal) ||
                !string.Equals(Path.GetExtension(safeFileName), ".bak", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The selected backup filename is invalid.");
            }

            string folder = GetBackupFolder();
            string fullPath = Path.GetFullPath(Path.Combine(folder, safeFileName));
            string folderPrefix = folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                                  Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The selected backup is outside the configured storage folder.");
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The selected stored backup no longer exists.", safeFileName);
            }

            return fullPath;
        }

        private static string GetDatabaseName()
        {
            SqlConnectionStringBuilder builder =
                new SqlConnectionStringBuilder(SystemUserSecurity.ConnectionString);
            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                throw new ConfigurationErrorsException(
                    "SchoolDB must specify an Initial Catalog database name.");
            }

            return builder.InitialCatalog.Trim();
        }

        private static string GetMasterConnectionString()
        {
            SqlConnectionStringBuilder builder =
                new SqlConnectionStringBuilder(SystemUserSecurity.ConnectionString)
                {
                    InitialCatalog = "master",
                    Pooling = false
                };
            return builder.ConnectionString;
        }

        private static string QuoteIdentifier(string identifier)
        {
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        private static string FormatFileSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double size = bytes;
            int unitIndex = 0;
            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return size.ToString(unitIndex == 0 ? "N0" : "N2", CultureInfo.InvariantCulture) +
                   " " + units[unitIndex];
        }

        private static string BuildOperationError(string operation, Exception exception)
        {
            SqlException sqlException = exception as SqlException;
            if (sqlException != null)
            {
                return "Database " + operation + " failed. Confirm that the website database identity has " +
                       "SQL Server " + operation + " permission and that the SQL Server service account can " +
                       "read and write the backup folder. Details: " + sqlException.Message;
            }

            return "Database " + operation + " failed. " + exception.Message;
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "system-message system-message-" + type;
            lblMessage.Text = message;
        }

        private sealed class BackupHeader
        {
            public string DatabaseName { get; set; }
            public int Position { get; set; }
        }
    }
}
