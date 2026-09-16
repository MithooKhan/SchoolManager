<%@ Page Title="Database Backup and Restore" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="DataBackup.aspx.cs" Inherits="DigitalSchoolManager.DataBackup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="data-backup-page">
        <section class="backup-hero" aria-labelledby="backupPageTitle">
            <div>
                <span class="system-module-label">System Administration</span>
                <h1 id="backupPageTitle">Database Backup and Restore</h1>
                <p>Create verified SQL Server backups and restore SchoolDatabase from an approved backup file.</p>
            </div>
            <div class="backup-hero-status">
                <small>Protected operation</small>
                <strong>Administrator access</strong>
            </div>
        </section>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="system-message" role="status" aria-live="polite">
            <asp:Label ID="lblMessage" runat="server" />
        </asp:Panel>

        <section class="backup-environment-grid" aria-label="Database backup environment">
            <div class="backup-status-card">
                <span>Database</span>
                <asp:Label ID="lblDatabaseName" runat="server" Text="Not available" />
                <small>Configured through the SchoolDB connection string</small>
            </div>
            <div class="backup-status-card backup-folder-card">
                <span>Backup storage</span>
                <asp:Label ID="lblBackupFolder" runat="server" Text="Not available" />
                <small>SQL Server must have read and write access to this folder</small>
            </div>
            <div class="backup-status-card">
                <span>Stored backups</span>
                <asp:Label ID="lblBackupCount" runat="server" Text="0" />
                <small><asp:Label ID="lblLatestBackup" runat="server" Text="No backup created yet" /></small>
            </div>
        </section>

        <div class="backup-operation-grid">
            <section class="backup-operation-card create-backup-card" aria-labelledby="createBackupTitle">
                <div class="backup-card-number" aria-hidden="true">01</div>
                <div class="backup-card-heading">
                    <span>Create backup</span>
                    <h2 id="createBackupTitle">Protect the current database</h2>
                    <p>A full copy-only backup is written to the configured storage folder and verified before it is reported as successful.</p>
                </div>

                <div class="backup-feature-list">
                    <div><strong>Complete school data</strong><span>Students, staff, attendance, timetable, fees, and user accounts.</span></div>
                    <div><strong>Checksum verification</strong><span>The generated backup is checked by SQL Server after creation.</span></div>
                    <div><strong>Timestamped file</strong><span>Existing backup files are preserved.</span></div>
                </div>

                <asp:Button ID="btnCreateBackup" runat="server" Text="Create Database Backup"
                    CssClass="system-button system-button-primary system-button-full"
                    OnClick="btnCreateBackup_Click"
                    OnClientClick="this.value='Creating and Verifying Backup...';" />
            </section>

            <section class="backup-operation-card restore-backup-card" aria-labelledby="restoreBackupTitle">
                <div class="backup-card-number" aria-hidden="true">02</div>
                <div class="backup-card-heading">
                    <span>Restore backup</span>
                    <h2 id="restoreBackupTitle">Recover SchoolDatabase</h2>
                    <p>Select a stored backup or upload a SQL Server <code>.bak</code> file. The file is verified and its database name is checked before restore.</p>
                </div>

                <div class="restore-warning">
                    <strong>Important</strong>
                    <span>Restoring replaces the current SchoolDatabase. Create a fresh backup first if the present data may be needed.</span>
                </div>

                <div class="restore-source-picker">
                    <asp:RadioButtonList ID="rblRestoreSource" runat="server" CssClass="restore-source-options"
                        RepeatDirection="Horizontal" ClientIDMode="Static" onchange="toggleRestoreSource();">
                        <asp:ListItem Text="Stored backup" Value="Stored" Selected="True" />
                        <asp:ListItem Text="Upload backup file" Value="Upload" />
                    </asp:RadioButtonList>
                </div>

                <div id="storedBackupSource" class="restore-source-panel">
                    <div class="system-field">
                        <asp:Label ID="lblStoredBackupPrompt" runat="server" AssociatedControlID="ddlStoredBackups"
                            Text="Select stored backup" CssClass="system-field-label" />
                        <asp:DropDownList ID="ddlStoredBackups" runat="server" CssClass="system-control" />
                    </div>
                </div>

                <div id="uploadedBackupSource" class="restore-source-panel is-hidden">
                    <div class="system-field">
                        <asp:Label ID="lblUploadPrompt" runat="server" AssociatedControlID="fuRestoreBackup"
                            Text="Choose SQL Server backup file" CssClass="system-field-label" />
                        <asp:FileUpload ID="fuRestoreBackup" runat="server" CssClass="system-control system-file-control" accept=".bak" />
                        <small>Select a valid <code>.bak</code> file. No fixed backup-size restriction is applied by the restore page.</small>
                    </div>
                </div>

                <div class="restore-confirmation">
                    <asp:CheckBox ID="chkConfirmRestore" runat="server"
                        Text="I understand that the selected backup will replace the current SchoolDatabase." />
                </div>

                <asp:Button ID="btnRestoreBackup" runat="server" Text="Verify and Restore Database"
                    CssClass="system-button system-button-danger system-button-full"
                    OnClick="btnRestoreBackup_Click"
                    OnClientClick="return confirm('Restore the selected backup and replace the current SchoolDatabase?');" />
            </section>
        </div>

        <section class="stored-backups-card" aria-labelledby="storedBackupsTitle">
            <div class="backup-list-heading">
                <div>
                    <span class="system-section-kicker">Backup storage</span>
                    <h2 id="storedBackupsTitle">Available backup files</h2>
                    <p>Download a copy for safe external storage or select one above for restoration.</p>
                </div>
                <asp:Button ID="btnRefreshBackups" runat="server" Text="Refresh List"
                    CssClass="system-button system-button-secondary" CausesValidation="false"
                    OnClick="btnRefreshBackups_Click" />
            </div>

            <div class="system-grid-wrap">
                <asp:GridView ID="gvBackups" runat="server" AutoGenerateColumns="false"
                    CssClass="system-data-grid" GridLines="None" DataKeyNames="FileName"
                    EmptyDataText="No database backup files are currently stored."
                    OnRowCommand="gvBackups_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="No.">
                            <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="FileName" HeaderText="Backup file" />
                        <asp:BoundField DataField="CreatedOn" HeaderText="Created" />
                        <asp:BoundField DataField="FileSize" HeaderText="Size" />
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnDownloadBackup" runat="server" Text="Download"
                                    CssClass="backup-download-button" CommandName="DownloadBackup"
                                    CommandArgument='<%# Eval("FileName") %>' CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>

        <section class="backup-permission-note">
            <strong>SQL Server permission requirement</strong>
            <p>The website database identity needs SQL backup/restore permission, and the Windows account running the SQL Server service needs read and write permission on the configured backup folder.</p>
        </section>
    </div>

    <script type="text/javascript">
        function toggleRestoreSource() {
            var selected = document.querySelector('#rblRestoreSource input:checked');
            var storedPanel = document.getElementById('storedBackupSource');
            var uploadPanel = document.getElementById('uploadedBackupSource');
            var useUpload = selected && selected.value === 'Upload';

            if (storedPanel) {
                storedPanel.classList.toggle('is-hidden', useUpload);
            }
            if (uploadPanel) {
                uploadPanel.classList.toggle('is-hidden', !useUpload);
            }
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', toggleRestoreSource);
        } else {
            toggleRestoreSource();
        }
    </script>
</asp:Content>
