<%@ Page Title="Class Management" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="ClassManagement.aspx.cs" Inherits="DigitalSchoolManager.WebForm11" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    
  
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  
<div class="cm-page">

    <!-- ── Page Header ──────────────────────────────────────────── -->
    <div class="cm-header">
        <div class="cm-header-icon">
            <svg viewBox="0 0 24 24"><path d="M5 3a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2H5zm0 2h14v2H5V5zm0 4h6v2H5V9zm0 4h6v2H5v-2zm8-4h6v6h-6V9z"/></svg>
        </div>
        <div class="cm-header-text">
            <h1>Classes Management</h1>
            <p>Add, edit, and manage all class records in the system</p>
        </div>
    </div>

    <!-- ── Stats Strip ──────────────────────────────────────────── -->
    <div class="cm-stats-strip">
        <div class="cm-stat">
            <div class="cm-stat-icon green">
                <svg viewBox="0 0 24 24"><path d="M4 6h16M4 10h16M4 14h10"/></svg>
            </div>
            <div>
                <div class="cm-stat-value">
                    <asp:Label ID="lblTotalClasses" runat="server" Text="0" />
                </div>
                <div class="cm-stat-label">Total Classes</div>
            </div>
        </div>
        <div class="cm-stat">
            <div class="cm-stat-icon blue">
                <svg viewBox="0 0 24 24"><path d="M12 2a5 5 0 1 1 0 10A5 5 0 0 1 12 2zm0 12c5.33 0 8 2.67 8 4v2H4v-2c0-1.33 2.67-4 8-4z"/></svg>
            </div>
            <div>
                <div class="cm-stat-value">
                    <asp:Label ID="lblActiveToday" runat="server" Text="-" />
                </div>
                <div class="cm-stat-label">Last Added</div>
            </div>
        </div>
        <div class="cm-stat">
            <div class="cm-stat-icon amber">
                <svg viewBox="0 0 24 24"><path d="M12 2a10 10 0 1 1 0 20A10 10 0 0 1 12 2zm1 5h-2v6l5.25 3.15.75-1.23-4-2.42V7z"/></svg>
            </div>
            <div>
                <div class="cm-stat-value">
                    <asp:Label ID="lblLastUpdated" runat="server" Text="-" />
                </div>
                <div class="cm-stat-label">Last Updated</div>
            </div>
        </div>
    </div>

    <!-- ── Notification Banner ───────────────────────────────────── -->
    <div class="u-max-900-centered">
        <asp:Panel ID="pnlMessage" runat="server" Visible="false">
            <div id="statusBanner" class="cm-alert cm-alert-success">
                <svg viewBox="0 0 24 24"><path d="M9 12l2 2 4-4m6 2a9 9 0 1 1-18 0 9 9 0 0 1 18 0z"/></svg>
                <asp:Label ID="lblMessage" runat="server" />
            </div>
        </asp:Panel>
    </div>

    <!-- ══════════════════════════════════════════════════════════ -->
    <!--  ADD / EDIT FORM CARD                                      -->
    <!-- ══════════════════════════════════════════════════════════ -->
    <asp:Panel ID="pnlForm" runat="server">
    <div class="cm-card">
        <div class="cm-card-header">
            <svg viewBox="0 0 24 24"><path d="M12 4v16m-8-8h16"/></svg>
            <h2><asp:Label ID="lblFormTitle" runat="server" Text="Add New Class" /></h2>
        </div>
        <div class="cm-card-body">

            <asp:HiddenField ID="hfClassID" runat="server" Value="0" />

            <div class="cm-form-grid">

                <!-- Class Name -->
                <div class="cm-field full">
                    <label for="txtClassName">Class Name <span class="req">*</span></label>
                    <asp:TextBox ID="txtClassName" runat="server" CssClass="form-control"
                        placeholder="e.g. Grade 10 - Section A"
                        MaxLength="100" />

                    <asp:RequiredFieldValidator ID="rfvClassName" runat="server"
                        ControlToValidate="txtClassName"
                        ErrorMessage="Class Name is required."
                        Display="Dynamic"
                        ValidationGroup="vgClass"
                        CssClass="validator-msg"
                        SetFocusOnError="true" />

                    <asp:RegularExpressionValidator ID="revClassName" runat="server"
                        ControlToValidate="txtClassName"
                        ValidationExpression="^[A-Za-z0-9\s\-\/\(\)\.]{2,100}$"
                        ErrorMessage="Class Name: 2-100 characters; letters, digits, spaces, hyphens, slashes, or parentheses only."
                        Display="Dynamic"
                        ValidationGroup="vgClass"
                        CssClass="validator-msg" />
                </div>

            </div><!-- /cm-form-grid -->

            <!-- Buttons -->
            <div class="cm-btn-row">
                <asp:Button ID="btnSave" runat="server"
                    Text="Save Class"
                    CssClass="cm-btn cm-btn-primary"
                    ValidationGroup="vgClass"
                    OnClick="btnSave_Click" />

                <asp:Button ID="btnClear" runat="server"
                    Text="Clear Form"
                    CssClass="cm-btn cm-btn-secondary"
                    CausesValidation="false"
                    OnClick="btnClear_Click" />
            </div>

        </div><!-- /cm-card-body -->
    </div><!-- /cm-card -->
    </asp:Panel>

    <!-- ══════════════════════════════════════════════════════════ -->
    <!--  CLASSES GRID CARD                                         -->
    <!-- ══════════════════════════════════════════════════════════ -->
    <div class="cm-card">
        <div class="cm-card-header">
            <svg viewBox="0 0 24 24"><path d="M3 4h18M3 9h18M3 14h12M3 19h8"/></svg>
            <h2>All Classes</h2>
        </div>
        <div class="cm-card-body">

            <!-- Search -->
            <div class="cm-search-row">
                <div class="cm-search-wrap">
                    <svg viewBox="0 0 24 24"><path d="M21 21l-4.35-4.35M17 11A6 6 0 1 1 5 11a6 6 0 0 1 12 0z"/></svg>
                    <asp:TextBox ID="txtSearch" runat="server"
                        placeholder="Search class name..."
                        AutoPostBack="true"
                        OnTextChanged="txtSearch_TextChanged" />
                </div>
                <asp:Button ID="btnRefresh" runat="server"
                    Text="Refresh"
                    CssClass="cm-btn cm-btn-secondary"
                    CausesValidation="false"
                    OnClick="btnRefresh_Click" />
            </div>

            <!-- GridView -->
            <div class="cm-grid-wrap">
                <asp:GridView ID="gvClasses" runat="server"
                    CssClass="cm-grid"
                    AutoGenerateColumns="false"
                    DataKeyNames="ClassID"
                    OnRowEditing="gvClasses_RowEditing"
                    OnRowUpdating="gvClasses_RowUpdating"
                    OnRowCancelingEdit="gvClasses_RowCancelingEdit"
                    OnRowDeleting="gvClasses_RowDeleting"
                    EmptyDataText=""
                    ShowHeaderWhenEmpty="true">

                    <EmptyDataTemplate>
                        <div class="cm-empty">
                            <svg viewBox="0 0 24 24"><path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2M9 5a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2M9 5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2"/></svg>
                            <p>No classes found. Add your first class above.</p>
                        </div>
                    </EmptyDataTemplate>

                    <Columns>
                        
                        <asp:TemplateField HeaderText="ID" ItemStyle-Width="70">
                            <ItemTemplate>
                                <span class="grid-id-badge">
                                    <%# Eval("ClassID") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        
                        <asp:TemplateField HeaderText="Class Name">
                            <ItemTemplate>
                                <span class="grid-name-cell"><%# Eval("ClassName") %></span>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtEditName" runat="server"
                                    CssClass="edit-tb"
                                    Text='<%# Bind("ClassName") %>'
                                    MaxLength="100" />
                                <asp:RequiredFieldValidator
                                    ID="rfvEditName" runat="server"
                                    ControlToValidate="txtEditName"
                                    ErrorMessage="Required"
                                    Display="Dynamic"
                                    ValidationGroup="vgEdit"
                                    CssClass="validator-msg" />
                                <asp:RegularExpressionValidator
                                    ID="revEditName" runat="server"
                                    ControlToValidate="txtEditName"
                                    ValidationExpression="^[A-Za-z0-9\s\-\/\(\)\.]{2,100}$"
                                    ErrorMessage="2-100 chars; letters, digits, spaces, hyphens only."
                                    Display="Dynamic"
                                    ValidationGroup="vgEdit"
                                    CssClass="validator-msg" />
                            </EditItemTemplate>
                        </asp:TemplateField>

                       
                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="160">
                            <ItemTemplate>
                                <div class="grid-actions">
                                    <asp:LinkButton ID="lbEdit" runat="server"
                                        CommandName="Edit"
                                        CausesValidation="false"
                                        CssClass="grid-btn-edit"
                                        ToolTip="Edit this class">
                                         Edit
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lbDelete" runat="server"
                                        CommandName="Delete"
                                        CausesValidation="false"
                                        CssClass="grid-btn-del"
                                        OnClientClick="return confirm('Delete this class? This cannot be undone.');"
                                        ToolTip="Delete this class">
                                         Delete
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <div class="grid-actions">
                                    <asp:LinkButton ID="lbUpdate" runat="server"
                                        CommandName="Update"
                                        ValidationGroup="vgEdit"
                                        CssClass="grid-btn-update">
                                         Save
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lbCancel" runat="server"
                                        CommandName="Cancel"
                                        CausesValidation="false"
                                        CssClass="grid-btn-cancel">
                                         Cancel
                                    </asp:LinkButton>
                                </div>
                            </EditItemTemplate>
                        </asp:TemplateField>

                    </Columns>
                </asp:GridView>
            </div><!-- /cm-grid-wrap -->

        </div><!-- /cm-card-body -->
    </div><!-- /cm-card -->

</div><!-- /cm-page -->

<script>
    // Auto-dismiss success banners after 4 s
    window.addEventListener('DOMContentLoaded', function () {
        var banner = document.getElementById('statusBanner');
        if (banner && banner.classList.contains('cm-alert-success')) {
            setTimeout(function () {
                banner.style.transition = 'opacity .5s';
                banner.style.opacity = '0';
                setTimeout(function () { banner.style.display = 'none'; }, 500);
            }, 4000);
        }
    });
</script>
</asp:Content>
