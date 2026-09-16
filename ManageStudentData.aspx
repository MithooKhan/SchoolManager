<%@ Page Title="Manage Students" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="ManageStudentData.aspx.cs" Inherits="DigitalSchoolManager.WebForm16" %>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    


<div class="spinner-bg" id="spinBg"><div class="spinner"></div></div>

<!-- ══ TOPBAR ══ -->

<asp:ScriptManager ID="SM1" runat="server" />

<div class="pw student-management-v4">

    <!-- PAGE HEADER -->
    <div class="ph-row">
        <div class="ph-left page-header-copy-v4">
            <span class="page-eyebrow-v4">Student Records Office</span>
            <div class="breadcrumb">
                <a href="AdminDashboard.aspx">Dashboard</a>
                <a href="StudentRegistrationForm.aspx">Students</a>
                <span>Edit / Update</span>
            </div>
            <h2>Edit <em>Student</em> Record</h2>
            <p>Search, review, and update student academic and personal records from one secure workspace.</p>
        </div>
        <div class="page-header-side-v4">
            <span>Records Workflow</span>
            <strong>Student Data Management</strong>
            <a href="StudentRegistrationForm.aspx">Register New Student</a>
        </div>
    </div>

    <!-- STATS -->
    <div class="stats-row">
        <div class="stat-card">
            <div class="stat-icon"></div>
            <div>
                <div class="stat-val"><asp:Label ID="lblTotal" runat="server" Text="0"/></div>
                <div class="stat-lbl">Total Students</div>
            </div>
        </div>
        <div class="stat-card">
            <div class="stat-icon"></div>
            <div>
                <div class="stat-val"><asp:Label ID="lblClasses" runat="server" Text="0"/></div>
                <div class="stat-lbl">Active Classes</div>
            </div>
        </div>
        <div class="stat-card">
            <div class="stat-icon"></div>
            <div>
                <div class="stat-val"><asp:Label ID="lblMale" runat="server" Text="0"/></div>
                <div class="stat-lbl">Male Students</div>
            </div>
        </div>
        <div class="stat-card">
            <div class="stat-icon"></div>
            <div>
                <div class="stat-val"><asp:Label ID="lblFemale" runat="server" Text="0"/></div>
                <div class="stat-lbl">Female Students</div>
            </div>
        </div>
    </div>

    <!-- MESSAGES -->
    <asp:UpdatePanel ID="upMsg" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlOk"  runat="server" Visible="false" CssClass="alert alert-success">
                <asp:Label ID="lblOk"  runat="server"/>
            </asp:Panel>
            <asp:Panel ID="pnlErr" runat="server" Visible="false" CssClass="alert alert-error">
                <asp:Label ID="lblErr" runat="server"/>
            </asp:Panel>
            <asp:Panel ID="pnlInfo" runat="server" Visible="false" CssClass="alert alert-info">
                <asp:Label ID="lblInfo" runat="server"/>
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- SEARCH PANEL -->
    <div class="search-card no-print">
        <div class="field filter-width-220">
            <label> Search by Reg No or Name</label>
            <asp:TextBox ID="txtSearchStudent" runat="server" placeholder="e.g. 2024-001 or Ali Khan" />
        </div>
        <div class="field filter-width-200">
            <label> Filter by Class</label>
            <asp:DropDownList ID="ddlSearchClass" runat="server" CssClass="sel-filter" />
        </div>
        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary"
            OnClick="btnSearch_Click" CausesValidation="false" />
        <asp:Button ID="btnClearSearch" runat="server" Text="Clear" CssClass="btn btn-secondary"
            OnClick="btnClearSearch_Click" CausesValidation="false" />
    </div>

    <!-- EDIT FORM CARD -->
    <asp:Panel ID="pnlEditForm" runat="server" Visible="false">
    <div class="card">
        <div class="card-hd">
            <div class="card-hd-left">
                <div class="card-hd-icon"></div>
                <div>
                    <h3>Editing Student Record</h3>
                    <p>Modify the fields below and click Update Student to save changes</p>
                </div>
            </div>
            <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn btn-secondary u-button-compact"
                OnClick="btnCancelEdit_Click" CausesValidation="false" />
        </div>
        <div class="card-body">

            <!-- student status strip -->
            <asp:Panel ID="pnlStrip" runat="server" CssClass="status-strip">
                <asp:Image ID="imgStripPhoto" runat="server" CssClass="ss-avatar" AlternateText="Photo" />
                <div>
                    <div class="ss-name"><asp:Label ID="lblStripName" runat="server" /></div>
                    <div class="ss-meta">
                        Reg: <asp:Label ID="lblStripRegno" runat="server" /> &nbsp;|&nbsp;
                        Class: <asp:Label ID="lblStripClass" runat="server" /> &nbsp;|&nbsp;
                        Roll No: <asp:Label ID="lblStripRoll" runat="server" />
                    </div>
                </div>
                <div class="ss-badge"> Editing</div>
            </asp:Panel>

            <asp:ValidationSummary ID="vsSummary" runat="server"
                CssClass="alert alert-error"
                HeaderText="Please fix these errors:"
                DisplayMode="BulletList"
                ShowMessageBox="false" ShowSummary="true" />

            <!-- hidden student ID -->
            <asp:HiddenField ID="hfStudentID" runat="server" />

            <!-- SECTION 1: Personal -->
            <div class="sec-div"> Personal Information</div>
            <div class="fg">

                <div class="field">
                    <label> Registration No <span class="req">*</span></label>
                    <asp:TextBox ID="txtRegNo" runat="server" placeholder="GHSS-Maankot-YYYYMMDD-0000" MaxLength="60"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtRegNo"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Reg No required.">
                         Required
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtRegNo"
                        ValidationExpression="^(?:\d{4}-\d{1,5}|GHSS-Maankot-\d{8}-\d{4})$"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Use the generated GHSS-Maankot-YYYYMMDD-0000 format.">
                        Use the generated school registration number format.
                    </asp:RegularExpressionValidator>
                </div>

                <div class="field">
                    <label> Student Full Name <span class="req">*</span></label>
                    <asp:TextBox ID="txtName" runat="server" placeholder="Full name" MaxLength="100"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtName"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Name required.">
                         Required
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtName"
                        ValidationExpression="^[a-zA-Z\s\.]{3,100}$"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Letters only, min 3 chars.">
                         Letters only, min 3 chars
                    </asp:RegularExpressionValidator>
                </div>

                <div class="field">
                    <label> Father's Name <span class="req">*</span></label>
                    <asp:TextBox ID="txtFatherName" runat="server" placeholder="Father's full name" MaxLength="100"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFatherName"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Father name required.">
                         Required
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtFatherName"
                        ValidationExpression="^[a-zA-Z\s\.]{3,100}$"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Letters only, min 3 chars.">
                         Letters only, min 3 chars
                    </asp:RegularExpressionValidator>
                </div>

                <div class="field">
                    <label> Form-B No <span class="req">*</span></label>
                    <asp:TextBox ID="txtFormBNo" runat="server" placeholder="12345-1234567-1" MaxLength="15"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFormBNo"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Form-B required.">
                         Required
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtFormBNo"
                        ValidationExpression="^\d{5}-\d{7}-\d{1}$"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Format: 12345-1234567-1">
                         Format: 12345-1234567-1
                    </asp:RegularExpressionValidator>
                </div>

                <div class="field">
                    <label> Father CNIC No <span class="req">*</span></label>
                    <asp:TextBox ID="txtCNIC" runat="server" placeholder="12345-1234567-1" MaxLength="15"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCNIC"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="CNIC required.">
                         Required
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtCNIC"
                        ValidationExpression="^\d{5}-\d{7}-\d{1}$"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Format: 12345-1234567-1">
                         Format: 12345-1234567-1
                    </asp:RegularExpressionValidator>
                </div>

                <div class="field">
                    <label> Date of Birth <span class="req">*</span></label>
                    <asp:TextBox ID="txtDOB" runat="server" TextMode="Date"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDOB"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="DOB required.">
                         Required
                    </asp:RequiredFieldValidator>
                </div>

            </div>

            <!-- SECTION 2: Contact -->
            <div class="sec-div"> Contact & Address</div>
            <div class="fg">

                <div class="field cs2">
                    <label> Address <span class="req">*</span></label>
                    <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" placeholder="Complete residential address" MaxLength="300"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtAddress"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Address required.">
                         Required
                    </asp:RequiredFieldValidator>
                </div>

                <div class="field">
                    <label> Contact No <span class="req">*</span></label>
                    <asp:TextBox ID="txtContact" runat="server" placeholder="0300-1234567" MaxLength="13"/>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtContact"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Contact required.">
                         Required
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtContact"
                        ValidationExpression="^0\d{3}-\d{7}$"
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Format: 0300-1234567">
                         Format: 0300-1234567
                    </asp:RegularExpressionValidator>
                </div>

            </div>

            <!-- SECTION 3: Protected admission and current enrolment -->
            <div class="sec-div"> Admission and Current Enrolment</div>
            <div class="fg fg4">

                <div class="field">
                    <label> Gender <span class="req">*</span></label>
                    <div class="g-group">
                        <label class="g-opt"><asp:RadioButton ID="rbMale"   runat="server" GroupName="Gen" Text="Male"   Checked="true"/>Male</label>
                        <label class="g-opt"><asp:RadioButton ID="rbFemale" runat="server" GroupName="Gen" Text="Female"/>Female</label>
                    </div>
                </div>

                <div class="field">
                    <label> Current Class</label>
                    <asp:DropDownList ID="ddlClass" runat="server" Enabled="false" />
                    <small>Use Student Promotion to change the current class.</small>
                </div>

                <div class="field">
                    <label> Date of Admission <span class="req">*</span></label>
                    <asp:TextBox ID="txtAdmDate" runat="server" TextMode="Date" ReadOnly="true" CssClass="readonly-field" />
                    <small>Permanent original admission date.</small>
                </div>

                <div class="field">
                    <label> Admission Class</label>
                    <asp:TextBox ID="txtAdmissionClass" runat="server" ReadOnly="true" CssClass="readonly-field" />
                    <small>Permanent original admission class.</small>
                </div>

                <div class="field">
                    <label> Current Class Enrolled On</label>
                    <asp:TextBox ID="txtCurrentEnrollmentDate" runat="server" TextMode="Date" ReadOnly="true" CssClass="readonly-field" />
                    <small>Date of the latest class enrolment.</small>
                </div>

                <div class="field">
                    <label> Roll No (Auto)</label>
                    <asp:TextBox ID="txtRollNo" runat="server" ReadOnly="true" placeholder="Select a class first"/>
                    <asp:Label ID="lblRollPill" runat="server" Visible="false">
                        <span class="roll-pill"> Auto-Assigned</span>
                    </asp:Label>
                </div>

            </div>

            <div class="student-promotion-guidance">
                <div><strong>Need to change this student's class?</strong><span>Use the promotion register to preserve the old class and create a new dated enrolment.</span></div>
                <a href="StudentPromotion.aspx">Open Student Promotion</a>
            </div>

            <div class="fg fg2">

                <div class="field">
                    <label> Study Medium <span class="req">*</span></label>
                    <asp:DropDownList ID="ddlMedium" runat="server">
                        <asp:ListItem Value="">Select Study Medium</asp:ListItem>
                        <asp:ListItem Value="English">English Medium</asp:ListItem>
                        <asp:ListItem Value="Urdu">Urdu Medium</asp:ListItem>
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlMedium" InitialValue=""
                        CssClass="valmsg" Display="Dynamic" ErrorMessage="Select study medium.">
                         Select medium
                    </asp:RequiredFieldValidator>
                </div>

                <!-- Photo Upload -->
                <div class="field">
                    <label> Update Photo (optional)</label>
                    <div class="photo-wrap">
                        <div class="photo-frame" id="photoFrame">
                            <asp:Image ID="imgPreview" runat="server" Visible="false" AlternateText="Current Photo"/>
                            <div class="photo-placeholder" id="phPh">
                                
                                <small>No new photo</small>
                            </div>
                        </div>
                        <div>
                            <asp:FileUpload ID="fuPhoto" runat="server" accept=".jpg,.jpeg,.png,.gif,.bmp" onchange="livePreview(this)"/>
                            <div class="photo-info u-mt-04">
                                <strong>Accepted:</strong> JPG, PNG, GIF, BMP<br/>
                                <strong>Database size:</strong> Maximum 1 MB; larger images are optimized automatically<br/>
                                <span class="u-text-success-strong">Leave blank</span> to keep existing photo
                            </div>
                        </div>
                    </div>
                </div>

            </div>

            <!-- ACTION BUTTONS -->
            <div class="btn-row">
                <asp:Button ID="btnUpdate" runat="server" Text="Update Student" CssClass="btn btn-primary"
                    OnClick="btnUpdate_Click" OnClientClick="showSpin()"/>
                <asp:Button ID="btnReset" runat="server" Text="Reload Original" CssClass="btn btn-amber"
                    OnClick="btnReset_Click" CausesValidation="false"/>
                <asp:Button ID="btnCancelEdit2" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                    OnClick="btnCancelEdit_Click" CausesValidation="false"/>
            </div>

        </div><!-- /card-body -->
    </div><!-- /card -->
    </asp:Panel><!-- /pnlEditForm -->

  <!-- STUDENT LIST CARD -->
<div id="printArea">
<div class="card">
        <div class="card-hd">
            <div class="card-hd-left">
                <div class="card-hd-icon"></div>
                <div>
                    <h3>Student List</h3>
                    <p id="printDate" class="print-date-caption"></p>
                </div>
            </div>
        </div>
        <div class="card-body">

            <div class="grid-tb no-print">
                <div class="grid-tb-left">
                    <div class="s-box">
                        
                        <asp:TextBox ID="txtGridSearch" runat="server" placeholder="Search name or registration number"
                            AutoPostBack="true" OnTextChanged="txtGridSearch_Changed"/>
                    </div>
                    <asp:DropDownList ID="ddlGridClass" runat="server" CssClass="sel-filter"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlGridClass_Changed"/>
                    <asp:DropDownList ID="ddlGridMedium" runat="server" CssClass="sel-filter"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlGridMedium_Changed">
                        <asp:ListItem Value="">All Mediums</asp:ListItem>
                        <asp:ListItem Value="English">English Medium</asp:ListItem>
                        <asp:ListItem Value="Urdu">Urdu Medium</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="grid-tb-right">
                    <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-secondary"
                        OnClick="btnRefresh_Click" CausesValidation="false"/>
                    <button type="button" class="btn btn-primary" onclick="printGrid()">
                         Print List
                    </button>
                </div>
            </div>

            <div class="dgv-wrap">
                <asp:GridView ID="gvStudents" runat="server"
                    AutoGenerateColumns="false"
                    CssClass="sgrid"
                    AllowPaging="true"
                    PageSize="12"
                    OnPageIndexChanging="gvStudents_PageIndexChanging"
                    OnRowCommand="gvStudents_RowCommand"
                    DataKeyNames="StudentID"
                    EmptyDataText="No students found for the selected filters."
                    GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="RowNum"          HeaderText="#"             ItemStyle-Width="42px"/>
                        <asp:TemplateField HeaderText="Photo" ItemStyle-Width="82px">
                            <ItemTemplate>
                                <asp:Image ID="imgStudentGrid" runat="server"
                                    CssClass="student-grid-photo"
                                    ImageUrl='<%# GetStudentImageUrl(Eval("Image")) %>'
                                    AlternateText='<%# "Photo of " + Eval("Name") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Regno"           HeaderText="Reg No"        />
                        <asp:BoundField DataField="Name"            HeaderText="Student Name"  />
                        <asp:BoundField DataField="FatherName"      HeaderText="Father Name"   />
                        <asp:BoundField DataField="ClassName"       HeaderText="Current Class" />
                        <asp:TemplateField HeaderText="Roll No">
                            <ItemTemplate>
                                <span class="roll-cell"><%# Eval("StudentRollNo") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Gender">
                            <ItemTemplate>
                                <span class='badge <%# Eval("Gender").ToString()=="Male"?"b-male":"b-female" %>'>
                                    <%# Eval("Gender") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Medium">
                            <ItemTemplate>
                                <span class='badge <%# Eval("StudyMedium").ToString()=="English"?"b-en":"b-ur" %>'>
                                    <%# Eval("StudyMedium") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ContactNo"       HeaderText="Contact"       />
                        <asp:BoundField DataField="DateofAdmission" HeaderText="Adm. Date"     DataFormatString="{0:dd-MMM-yyyy}"/>
                        <asp:BoundField DataField="CurrentClassEnrollmentDate" HeaderText="Current Class Since" DataFormatString="{0:dd-MMM-yyyy}"/>
                        <asp:TemplateField HeaderText="Action" ItemStyle-Width="80px">
                            <ItemTemplate>
                                <asp:LinkButton ID="lbEdit" runat="server"
                                    CommandName="EditStudent"
                                    CommandArgument='<%# Eval("StudentID") %>'
                                    CssClass="btn-edit-row"
                                    OnClientClick="showSpin()">
                                     Edit
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pager" HorizontalAlign="Right"/>
                    <PagerSettings Mode="NumericFirstLast" PageButtonCount="5"/>
                </asp:GridView>
            </div>

        </div>
    </div><!-- /grid card -->
    </div><!-- /printArea -->
</div><!-- /pw -->

<script>
    // set print date
    document.getElementById('printDate').textContent =
        'Printed: ' + new Date().toLocaleDateString('en-PK',
            {weekday:'long',year:'numeric',month:'long',day:'numeric'});

    function showSpin()  { document.getElementById('spinBg').classList.add('on'); }
    function hideSpin()  { document.getElementById('spinBg').classList.remove('on'); }
    window.onload = hideSpin;

    function livePreview(input) {
        if (input.files && input.files[0]) {
            const r = new FileReader();
            r.onload = e => {
                const frame = document.getElementById('photoFrame');
                let img = frame.querySelector('img');
                const ph = document.getElementById('phPh');
                if (!img) { img = document.createElement('img'); frame.appendChild(img); }
                img.src = e.target.result;
                img.style.cssText = 'width:100%;height:100%;object-fit:cover;';
                if (ph) ph.style.display = 'none';
            };
            r.readAsDataURL(input.files[0]);
        }
    }

    function printGrid() {
        document.getElementById('printDate').textContent =
            'Printed: ' + new Date().toLocaleDateString('en-PK',
                {weekday:'long',year:'numeric',month:'long',day:'numeric'});
        window.print();
    }
</script>

</asp:Content>
