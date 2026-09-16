<%@ Page Title="Class Incharge Management" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="ClassMentorManagement.aspx.cs" Inherits="DigitalSchoolManager.WebForm12" %>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
 
     
        <asp:ScriptManager ID="ScriptManager1" runat="server" />

       

        <!-- ══════════ PAGE WRAPPER ══════════ -->
        <div class="page-wrapper">

            <!-- Page Header -->
            <div class="page-header">
                <div class="breadcrumb">
                    Dashboard <span>/</span> Administration <span>/</span> Teachers &amp; Classes
                </div>
                <h1>Teachers &amp; Class Assignment</h1>
                <p>Assign a dedicated incharge / mentor teacher to each class. One teacher per class rule enforced.</p>
            </div>

            <!-- ══════════ STATUS MESSAGE ══════════ -->
            <asp:UpdatePanel ID="upStatus" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnlMessage" runat="server" Visible="false">
                        <div id="statusBar" class="status-bar status-success">
                            <span class="status-icon"></span>
                            <asp:Label ID="lblMessage" runat="server" Text="" />
                        </div>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>

            <!-- ══════════ ASSIGNMENT FORM CARD ══════════ -->
            <div class="card">
                <div class="card-header">
                    <div class="card-header-icon"></div>
                    <div>
                        <h2>Assign Teacher to Class</h2>
                        <p>Select a class and designate its incharge teacher</p>
                    </div>
                </div>
                <div class="card-body">

                    <!-- Business Rule Info -->
                    <div class="info-rule">
                        <span class="info-rule-icon"></span>
                        <span><strong>Business Rule:</strong> Each class may have <em>only one</em> incharge teacher, and each teacher may be incharge of <em>only one</em> class at a time. Existing assignments will be flagged before overwriting.</span>
                    </div>

                    <!-- Hidden field to store edit mode ClassID -->
                    <asp:HiddenField ID="hfEditClassID" runat="server" Value="0" />

                    <div class="form-grid">

                        <!-- Class Dropdown -->
                        <div class="form-group">
                            <label>
                                 Class <span class="req">*</span>
                                <span class="hint">Select target class</span>
                            </label>
                            <div class="input-wrap">
                                <span class="input-icon"></span>
                                <asp:DropDownList ID="ddlClass" runat="server" CssClass="form-control"
                                    AutoPostBack="false">
                                </asp:DropDownList>
                                <span class="select-arrow"></span>
                            </div>
                            <asp:RequiredFieldValidator ID="rfvClass" runat="server"
                                ControlToValidate="ddlClass"
                                InitialValue="0"
                                ErrorMessage=" Please select a class."
                                CssClass="validator-msg"
                                Display="Dynamic"
                                ValidationGroup="vgAssign" />
                        </div>

                        <!-- Teacher Dropdown -->
                        <div class="form-group">
                            <label>
                                 Incharge Teacher <span class="req">*</span>
                                <span class="hint">Select mentor / incharge</span>
                            </label>
                            <div class="input-wrap">
                                <span class="input-icon"></span>
                                <asp:DropDownList ID="ddlTeacher" runat="server" CssClass="form-control">
                                </asp:DropDownList>
                                <span class="select-arrow"></span>
                            </div>
                            <asp:RequiredFieldValidator ID="rfvTeacher" runat="server"
                                ControlToValidate="ddlTeacher"
                                InitialValue="0"
                                ErrorMessage=" Please select a teacher."
                                CssClass="validator-msg"
                                Display="Dynamic"
                                ValidationGroup="vgAssign" />
                        </div>

                    </div><!-- /form-grid -->

                    <!-- Button Row -->
                    <div class="btn-row">
                        <asp:Button ID="btnSave" runat="server" Text="Save Assignment"
                            CssClass="btn btn-primary"
                            OnClick="btnSave_Click"
                            ValidationGroup="vgAssign"
                            OnClientClick="return confirmSave();" />

                        <asp:Button ID="btnUpdate" runat="server" Text="Update Assignment"
                            CssClass="btn btn-secondary"
                            OnClick="btnUpdate_Click"
                            ValidationGroup="vgAssign"
                            Visible="false" />

                        <asp:Button ID="btnDelete" runat="server" Text="Delete Assignment"
                            CssClass="btn btn-danger"
                            OnClick="btnDelete_Click"
                            CausesValidation="false"
                            Visible="false"
                            OnClientClick="return confirm(' Are you sure you want to remove this teacher assignment?');" />

                        <asp:Button ID="btnClear" runat="server" Text="Clear Form"
                            CssClass="btn btn-ghost"
                            OnClick="btnClear_Click"
                            CausesValidation="false" />
                    </div>

                </div>
            </div><!-- /card -->

            <!-- ══════════ DATA GRID CARD ══════════ -->
            <div class="card grid-section">
                <div class="card-header">
                    <div class="card-header-icon"></div>
                    <div>
                        <h2>All Class-Teacher Assignments</h2>
                        <p>Live registry of all assigned incharge teachers</p>
                    </div>
                </div>
                <div class="card-body">

                    <div class="grid-meta">
                        <div class="grid-count">
                            Showing <strong><asp:Label ID="lblRecordCount" runat="server" Text="0" /></strong> assignment(s)
                        </div>
                        <asp:Button ID="btnRefresh" runat="server" Text="Refresh"
                            CssClass="btn btn-ghost mentor-small-action"
                            OnClick="btnRefresh_Click"
                            CausesValidation="false" />
                    </div>

                    <div class="DataGrid-container">
                        <asp:GridView ID="gvAssignments" runat="server"
                            AutoGenerateColumns="false"
                            CssClass="datagrid"
                            DataKeyNames="ClassID"
                            GridLines="None"
                            OnRowCommand="gvAssignments_RowCommand"
                            EmptyDataText="No assignments found. Use the form above to create the first assignment."
                            EmptyDataRowStyle-CssClass="empty-row">

                            <Columns>
                                <asp:TemplateField HeaderText="#" ItemStyle-Width="50px">
                                    <ItemTemplate>
                                        <span class="mentor-muted-label">
                                            <%# Container.DataItemIndex + 1 %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Class Name">
                                    <ItemTemplate>
                                        <span class="pill pill-blue">
                                            <span class="pill-dot"></span>
                                            <%# Eval("ClassName") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:BoundField DataField="ClassName" HeaderText="Class" Visible="false" />

                                <asp:TemplateField HeaderText="Incharge Teacher">
                                    <ItemTemplate>
                                        <div class="u-flex-center-gap-06">
                                            <div class="mentor-avatar">
                                                <%# GetInitials(Eval("Name").ToString()) %>
                                            </div>
                                            <div>
                                                <div class="mentor-name"><%# Eval("Name") %></div>
                                                
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Status" ItemStyle-Width="120px">
                                    <ItemTemplate>
                                        <span class="pill pill-green">
                                            <span class="pill-dot"></span>
                                            Assigned
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="Actions" ItemStyle-Width="160px">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lbEdit" runat="server"
                                            CommandName="EditRow"
                                            CommandArgument='<%# Eval("ClassID") %>'
                                            CssClass="btn-grid btn-grid-edit"
                                            CausesValidation="false"> Edit</asp:LinkButton>
                                        <br /> <br /> 
                                        <asp:LinkButton ID="lbDelete" runat="server"
                                            CommandName="DeleteRow"
                                            CommandArgument='<%# Eval("ClassID") %>'
                                            CssClass="btn-grid btn-grid-del"
                                            CausesValidation="false"
                                            OnClientClick="return confirm('Remove this assignment?');"> Del</asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>

                            <HeaderStyle CssClass="grid-header" />
                            <AlternatingRowStyle BackColor="#f8fafc" />
                        </asp:GridView>
                    </div><!-- /DataGrid-container -->

                </div>
            </div><!-- /card -->

        </div><!-- /page-wrapper -->

        <div class="page-footer">Copyright 2025 EduAdmin - School Management System &nbsp;|&nbsp; All rights reserved</div>

    </form>

    <script type="text/javascript">
        function confirmSave() {
            var cls = document.getElementById('<%= ddlClass.ClientID %>');
            var tch = document.getElementById('<%= ddlTeacher.ClientID %>');
            if (cls.value === '0' || tch.value === '0') return true;
            return confirm('Confirm assignment of selected teacher as Class Incharge?');
        }
    </script>
                       

</asp:Content>
