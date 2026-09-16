<%@ Page Title="Class Subject Groups" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="ClassSubjectsManagement.aspx.cs" Inherits="DigitalSchoolManager.WebForm14" %>
<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server" />
<asp:Content ID="MainContentBlock" ContentPlaceHolderID="MainContent" runat="server">
<section class="class-subject-groups-page resource-page">
    <header class="resource-hero subject-group-hero"><div><span class="module-kicker">ACADEMIC STRUCTURE</span><h1>Class Subjects and Choice Groups</h1><p>Assign compulsory and optional subjects to each class, define student choices, and control religion-based eligibility.</p></div><div class="resource-hero-badge"><strong>Registration-ready</strong><span>Class selections appear automatically on the Student Registration form</span></div></header>
    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="module-message"><asp:Label ID="lblMessage" runat="server" /></asp:Panel>

    <div class="subject-guidance-grid">
        <div><span>COMPULSORY</span><strong>Assigned automatically</strong><p>Use for subjects studied by every eligible student in the selected class.</p></div>
        <div><span>FAITH CHOICE</span><strong>Muslim / Non-Muslim</strong><p>Assign Islamiat to Muslim students and Ethics / Akhlaqiat to non-Muslim students.</p></div>
        <div><span>CHOICE GROUP 2</span><strong>Arabic or Computer Science</strong><p>Students in Classes 6 to 8 can select one configured subject.</p></div>
        <div><span>CHOICE GROUP 3</span><strong>Skills and humanities</strong><p>Agriculture, Electric Wiring, History, Geography, Home Economics or another approved option.</p></div>
    </div>

    <article class="module-card subject-assignment-editor">
        <div class="module-card-head"><div><span class="module-kicker">SUBJECT ASSIGNMENT</span><h2><asp:Label ID="lblEditorTitle" runat="server" Text="Assign a subject to a class" /></h2></div><asp:Button ID="btnNew" runat="server" Text="New Assignment" CssClass="module-button secondary" OnClick="btnNew_Click" CausesValidation="false" /></div>
        <asp:HiddenField ID="hidRecordID" runat="server" />
        <div class="module-form-grid three-column">
            <label>Class<asp:DropDownList ID="ddlClass" runat="server" CssClass="module-input" /><asp:RequiredFieldValidator ID="rfvClass" runat="server" ControlToValidate="ddlClass" InitialValue="" ErrorMessage="Select a class." CssClass="validator-msg" ValidationGroup="SubjectAssignment" /></label>
            <label>Subject<asp:DropDownList ID="ddlSubject" runat="server" CssClass="module-input" /><asp:RequiredFieldValidator ID="rfvSubject" runat="server" ControlToValidate="ddlSubject" InitialValue="" ErrorMessage="Select a subject." CssClass="validator-msg" ValidationGroup="SubjectAssignment" /></label>
            <label>Subject nature<asp:DropDownList ID="ddlSubjectGroup" runat="server" CssClass="module-input"><asp:ListItem Value="Compulsory">Compulsory Subject</asp:ListItem><asp:ListItem Value="Optional">Optional Subject</asp:ListItem></asp:DropDownList></label>
            <label>Student choice group<asp:DropDownList ID="ddlOptionGroup" runat="server" CssClass="module-input"><asp:ListItem Value="">No choice group</asp:ListItem><asp:ListItem Value="FAITH">Faith / Religion Choice</asp:ListItem><asp:ListItem Value="GROUP2">Group 2 - Arabic / Computer Science</asp:ListItem><asp:ListItem Value="GROUP3">Group 3 - Skills / Humanities</asp:ListItem><asp:ListItem Value="CUSTOM">Custom Choice Group</asp:ListItem></asp:DropDownList></label>
            <label>Custom group name<asp:TextBox ID="txtCustomGroupName" runat="server" CssClass="module-input" placeholder="Used only for Custom Choice Group" /></label>
            <label>Religion eligibility<asp:DropDownList ID="ddlReligionEligibility" runat="server" CssClass="module-input"><asp:ListItem Value="All">All Students</asp:ListItem><asp:ListItem Value="Muslim">Muslim Students</asp:ListItem><asp:ListItem Value="Non-Muslim">Non-Muslim Students</asp:ListItem></asp:DropDownList></label>
            <label class="check-field"><asp:CheckBox ID="chkStudentSelectable" runat="server" Text="Student must select this through a choice group" /></label>
            <label>Display order<asp:TextBox ID="txtDisplayOrder" runat="server" CssClass="module-input" TextMode="Number" min="0" Text="0" /><small>Lower numbers appear first during registration.</small></label>
        </div>
        <div class="subject-rule-note"><strong>Primary classes:</strong> the administrator may freely decide subject nature and custom groups. <strong>Classes 6 to 8:</strong> configure the Faith, Group 2 and Group 3 options above; the registration form applies the configured choices.</div>
        <div class="module-actions"><asp:Button ID="btnSave" runat="server" Text="Save Assignment" CssClass="module-button" OnClick="btnSave_Click" ValidationGroup="SubjectAssignment" /><asp:Button ID="btnCancel" runat="server" Text="Cancel Edit" CssClass="module-button secondary" OnClick="btnCancel_Click" CausesValidation="false" Visible="false" /></div>
    </article>

    <div class="resource-stat-grid subject-stat-grid"><div><span>Classes</span><strong><asp:Label ID="lblTotalClasses" runat="server" Text="0" /></strong></div><div><span>Assignments</span><strong><asp:Label ID="lblTotalAssignments" runat="server" Text="0" /></strong></div><div><span>Compulsory</span><strong><asp:Label ID="lblCompulsoryCount" runat="server" Text="0" /></strong></div><div><span>Student choices</span><strong><asp:Label ID="lblChoiceCount" runat="server" Text="0" /></strong></div></div>

    <article class="module-card resource-register">
        <div class="module-card-head"><div><span class="module-kicker">CLASS CURRICULUM REGISTER</span><h2>Configured class subjects</h2></div></div>
        <div class="resource-filters"><asp:DropDownList ID="ddlFilterClass" runat="server" CssClass="module-input" /><asp:Button ID="btnFilter" runat="server" Text="Load Class Subjects" CssClass="module-button" OnClick="btnFilter_Click" CausesValidation="false" /></div>
        <div class="module-table-wrap"><asp:GridView ID="gvClassSubjects" runat="server" AutoGenerateColumns="false" CssClass="module-table" EmptyDataText="No class-subject assignments match the selected class." OnRowCommand="gvClassSubjects_RowCommand"><Columns>
            <asp:BoundField DataField="ClassName" HeaderText="Class" /><asp:BoundField DataField="SubjectName" HeaderText="Subject" /><asp:BoundField DataField="SubjectGroup" HeaderText="Nature" /><asp:BoundField DataField="OptionGroupName" HeaderText="Choice Group" NullDisplayText="Fixed / no group" /><asp:BoundField DataField="ReligionEligibility" HeaderText="Eligibility" /><asp:CheckBoxField DataField="IsStudentSelectable" HeaderText="Student Selects" /><asp:BoundField DataField="DisplayOrder" HeaderText="Order" />
            <asp:TemplateField HeaderText="Actions"><ItemTemplate><asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CommandName="EditAssignment" CommandArgument='<%# Eval("ClassSubjectID") %>' CssClass="table-action table-action-edit" CausesValidation="false" /><asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CommandName="DeleteAssignment" CommandArgument='<%# Eval("ClassSubjectID") %>' CssClass="table-action table-action-delete" CausesValidation="false" OnClientClick="return confirm('Delete this class-subject assignment? Existing student selection history will be retained.');" /></ItemTemplate></asp:TemplateField>
        </Columns></asp:GridView></div>
    </article>
</section>
</asp:Content>
