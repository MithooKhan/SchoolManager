<%@ Page Title="Exam Subjects" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="ManageSubjectInExam.aspx.cs" Inherits="DigitalSchoolManager.WebForm19" %>
 
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    

    
    <!-- ═══ PAGE WRAPPER ═══ -->
    <div class="page-wrapper">

        <!-- ── Alerts ── -->
        <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="alert alert-success">
            
            <asp:Label ID="lblSuccess" runat="server"></asp:Label>
        </asp:Panel>
        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-danger">
            
            <asp:Label ID="lblError" runat="server"></asp:Label>
        </asp:Panel>

        <!-- ── Stats Row ── -->
        <div class="stats-row">
            <div class="stat-card">
                <div class="stat-icon-wrap stat-icon-green"></div>
                <div>
                    <div class="stat-value"><asp:Label ID="lblTotalSubjects" runat="server" Text="0"></asp:Label></div>
                    <div class="stat-label">Total Exam Subjects</div>
                </div>
            </div>
            <div class="stat-card">
                <div class="stat-icon-wrap stat-icon-blue"></div>
                <div>
                    <div class="stat-value"><asp:Label ID="lblTotalClasses" runat="server" Text="0"></asp:Label></div>
                    <div class="stat-label">Classes Covered</div>
                </div>
            </div>
            <div class="stat-card">
                <div class="stat-icon-wrap stat-icon-orange"></div>
                <div>
                    <div class="stat-value"><asp:Label ID="lblTotalExams" runat="server" Text="0"></asp:Label></div>
                    <div class="stat-label">Active Exams</div>
                </div>
            </div>
            <div class="stat-card">
                <div class="stat-icon-wrap stat-icon-purple"></div>
                <div>
                    <div class="stat-value"><asp:Label ID="lblAvgMarks" runat="server" Text="0"></asp:Label></div>
                    <div class="stat-label">Avg. Total Marks</div>
                </div>
            </div>
        </div>

        <!-- ═══ ENTRY FORM CARD ═══ -->
        <div class="section-header">
            <div class="section-accent"></div>
            <div>
                <h2>Exam Subject Entry</h2>
                <p>Add or update subjects assigned to examinations</p>
            </div>
        </div>

        <div class="card">
            <div class="card-header">
                <div class="card-header-icon"></div>
                <div class="card-header-text">
                    <h3>Exam Subject Details</h3>
                    <p>Fields marked with <span class="u-text-green-soft">*</span> are required</p>
                </div>
            </div>
            <div class="card-body">
                <asp:ValidationSummary ID="vsSummary" runat="server"
                    CssClass="alert alert-danger"
                    HeaderText="Please correct the following errors:"
                    DisplayMode="BulletList"
                    ValidationGroup="frmEntry"
                    ForeColor="" />

                <div class="form-grid">

                    <!-- Exam -->
                    <div class="field-group">
                        <label class="field-label"> Exam &nbsp;<span class="req">*</span></label>
                        <asp:DropDownList ID="ddlExam" runat="server" CssClass="field-select">
                            <asp:ListItem Value="">- Select Exam -</asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvExam" runat="server"
                            ControlToValidate="ddlExam" InitialValue=""
                            ErrorMessage="Exam is required."
                            Display="Dynamic" ValidationGroup="frmEntry"
                            CssClass="validator-msg">
                             Exam is required.
                        </asp:RequiredFieldValidator>
                    </div>

                    <!-- Subject -->
                    <div class="field-group">
                        <label class="field-label"> Subject &nbsp;<span class="req">*</span></label>
                        <asp:DropDownList ID="ddlSubject" runat="server" CssClass="field-select">
                            <asp:ListItem Value="">- Select Subject -</asp:ListItem>
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator ID="rfvSubject" runat="server"
                            ControlToValidate="ddlSubject" InitialValue=""
                            ErrorMessage="Subject is required."
                            Display="Dynamic" ValidationGroup="frmEntry"
                            CssClass="validator-msg">
                             Subject is required.
                        </asp:RequiredFieldValidator>
                    </div>

                    <!-- Total Marks -->
                    <div class="field-group">
                        <label class="field-label"> Total Marks &nbsp;<span class="req">*</span></label>
                        <asp:TextBox ID="txtTotalMarks" runat="server" CssClass="field-input"
                            placeholder="e.g. 100" MaxLength="5"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvMarks" runat="server"
                            ControlToValidate="txtTotalMarks"
                            ErrorMessage="Total Marks are required."
                            Display="Dynamic" ValidationGroup="frmEntry"
                            CssClass="validator-msg">
                             Total Marks are required.
                        </asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="revMarks" runat="server"
                            ControlToValidate="txtTotalMarks"
                            ValidationExpression="^[1-9][0-9]{0,4}$"
                            ErrorMessage="Enter a valid number between 1 and 99999."
                            Display="Dynamic" ValidationGroup="frmEntry"
                            CssClass="validator-msg">
                             Must be a positive number (1 - 99999).
                        </asp:RegularExpressionValidator>
                        <asp:RangeValidator ID="rvMarks" runat="server"
                            ControlToValidate="txtTotalMarks"
                            MinimumValue="1" MaximumValue="1000"
                            Type="Integer"
                            ErrorMessage="Total Marks must be between 1 and 1000."
                            Display="Dynamic" ValidationGroup="frmEntry"
                            CssClass="validator-msg">
                             Total Marks must be between 1 and 1000.
                        </asp:RangeValidator>
                    </div>
                                                                    <!-- Hidden ExamSubjectID for edit -->
                    <asp:HiddenField ID="hfExamSubjectID" runat="server" Value="0" />

                </div><!-- /form-grid -->

                <!-- Action Buttons -->
                <div class="btn-row">
                    <asp:Button ID="btnSave" runat="server" Text="Save Record"
                        CssClass="btn btn-primary" ValidationGroup="frmEntry"
                        OnClick="btnSave_Click">
                    </asp:Button>
                    <asp:Button ID="btnUpdate" runat="server" Text="Update Record"
                        CssClass="btn btn-secondary" ValidationGroup="frmEntry"
                        OnClick="btnUpdate_Click" Visible="false">
                    </asp:Button>
                    <asp:Button ID="btnDelete" runat="server" Text="Delete Record"
                        CssClass="btn btn-danger" CausesValidation="false"
                        OnClick="btnDelete_Click" Visible="false"
                        OnClientClick="return confirm('Are you sure you want to delete this record?');">
                    </asp:Button>
                    <asp:Button ID="btnClear" runat="server" Text="Clear Form"
                        CssClass="btn btn-outline" CausesValidation="false"
                        OnClick="btnClear_Click">
                    </asp:Button>
                </div>
            </div><!-- /card-body -->
        </div><!-- /card -->

        <!-- ═══ GRID SECTION ═══ -->
        <div class="section-header">
            <div class="section-accent"></div>
            <div>
                <h2>Subjects &amp; Marks Register</h2>
                <p>Complete list of exam subjects with total marks per class</p>
            </div>
        </div>

        <div class="card">
            <div class="card-header">
                <div class="card-header-icon"></div>
                <div class="card-header-text">
                    <h3>Exam Subjects List</h3>
                    <p>Filter by class to narrow results</p>
                </div>
            </div>
            <div class="card-body">
                <!-- Grid Controls -->
                <div class="grid-controls">
                    <div class="grid-filter">
                        <span class="filter-label"> Filter by Class:</span>
                        <asp:DropDownList ID="ddlFilterClass" runat="server" CssClass="filter-select"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlFilterClass_Changed">
                            <asp:ListItem Value="0">All Classes</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="record-count">
                        Showing <span><asp:Label ID="lblRecordCount" runat="server" Text="0"></asp:Label></span> records
                    </div>
                </div>

                <!-- GridView -->
                <div class="gv-wrap">
                    <asp:GridView ID="gvExamSubjects" runat="server"
                        CssClass="grid-table"
                        AutoGenerateColumns="false"
                        AllowPaging="true" PageSize="10"
                        OnPageIndexChanging="gvExamSubjects_PageIndexChanging"
                        OnRowCommand="gvExamSubjects_RowCommand"
                        DataKeyNames="ExamSubjectID"
                        EmptyDataText="No records found for the selected filter."
                        PagerStyle-CssClass="pager-row">

                        <Columns>
                            <asp:BoundField DataField="ExamSubjectID" HeaderText="#" ItemStyle-Width="50px" />
                            <asp:BoundField DataField="ExamName"    HeaderText="Examination"  />
                            <asp:BoundField DataField="ClassName"   HeaderText="Class"        />
                            <asp:BoundField DataField="SubjectName" HeaderText="Subject"      />
                              <asp:TemplateField HeaderText="Total Marks">
                                <ItemTemplate>
                                    <div class="marks-bar">
                                        <progress class="marks-progress" max="100"
                                            value='<%# GetMarksPercent(Eval("TotalMarks")) %>'></progress>
                                        <span class="bar-num"><%# Eval("TotalMarks") %></span>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Actions" ItemStyle-Width="140px">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lbEdit" runat="server"
                                        CommandName="EditRow"
                                        CommandArgument='<%# Eval("ExamSubjectID") %>'
                                        CssClass="btn btn-warning u-button-small-12">
                                         Edit
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                        <PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" PageButtonCount="5" />
                    </asp:GridView>
                </div>

            </div><!-- /card-body -->
        </div><!-- /card -->

    </div><!-- /page-wrapper -->
</asp:Content>
