<%@ Page Title="Manage Teachers" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="ManageTeachersData.aspx.cs" Inherits="DigitalSchoolManager.WebForm3" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<asp:Panel ID="pnlPageMessage" runat="server" Visible="false"
CssClass="module-message error" role="alert" aria-live="assertive">
    <asp:Label ID="lblPageMessage" runat="server" />
</asp:Panel>

<div class="section-title">
Search Teacher
</div>

<div class="row">

<div class="col-md-4">

<label class="lbl">
Search By CNIC
</label>

<asp:TextBox ID="txtSearchCNIC"
runat="server"
CssClass="form-control" />

</div>

<div class="col-md-4">

<label class="lbl">
Select Teacher
</label>

<asp:DropDownList ID="ddlTeachers"
runat="server"
CssClass="form-control"
AutoPostBack="true"
OnSelectedIndexChanged="ddlTeachers_SelectedIndexChanged">
</asp:DropDownList>

</div>

<div class="col-md-4">

<br />

<asp:Button ID="btnSearch"
runat="server"
Text="Search"
CssClass="btn btn-success"
OnClick="btnSearch_Click" />

</div>

</div>
<div class="col-md-3 text-center">

<asp:Image ID="imgTeacher"
runat="server"
CssClass="teacher-img"
ImageUrl="~/images/noimage.png" />

<br /><br />

<asp:FileUpload ID="fuImage"
runat="server"
CssClass="form-control"
accept=".jpg,.jpeg,.png,.gif,.bmp"
onchange="previewTeacher(this);" />
<small class="text-muted d-block mt-2">Replacement images are automatically optimized to a maximum database size of 1 MB.</small>

</div>
<div class="col-md-3">

<label class="lbl">
Teacher ID
</label>

<asp:TextBox ID="txtTeacherID"
runat="server"
CssClass="form-control"
ReadOnly="true" />

</div>

<div class="col-md-3">

<label class="lbl">
Teacher Name
</label>

<asp:TextBox ID="txtName"
runat="server"
CssClass="form-control" />

<asp:RequiredFieldValidator
runat="server"
ControlToValidate="txtName"
ErrorMessage="Required"
ForeColor="Red" />

</div>

<div class="col-md-3">

<label class="lbl">
Father Name
</label>

<asp:TextBox ID="txtFatherName"
runat="server"
CssClass="form-control" />

</div>

<div class="row">

<div class="col-md-4">
<label class="lbl">Gender</label>
<asp:DropDownList ID="ddlGender"
runat="server"
CssClass="form-control">
<asp:ListItem>Male</asp:ListItem>
<asp:ListItem>Female</asp:ListItem>
</asp:DropDownList>
</div>

<div class="col-md-4">
<label class="lbl">CNIC Number *</label>
<asp:TextBox ID="txtCNIC"
runat="server"
CssClass="form-control" />

<asp:RegularExpressionValidator
runat="server"
ControlToValidate="txtCNIC"
ValidationExpression="^\d{5}-\d{7}-\d{1}$"
ErrorMessage="12345-1234567-1"
ForeColor="Red" />

</div>

<div class="col-md-4">
<label class="lbl">Contact Number *</label>
<asp:TextBox ID="txtContact"
runat="server"
CssClass="form-control" />

<asp:RegularExpressionValidator
runat="server"
ControlToValidate="txtContact"
ValidationExpression="^03\d{9}$"
ErrorMessage="03XXXXXXXXX"
ForeColor="Red" />

</div>

</div>

<br />

<div class="row">

<div class="col-md-4">
<label class="lbl">Email Address</label>
<asp:TextBox ID="txtEmail"
runat="server"
CssClass="form-control" />
</div>

<div class="col-md-4">
<label class="lbl">Marital Status</label>
<asp:DropDownList ID="ddlMaritalStatus"
runat="server"
CssClass="form-control">

<asp:ListItem>Single</asp:ListItem>
<asp:ListItem>Married</asp:ListItem>

</asp:DropDownList>
</div>

<div class="col-md-4">
<label class="lbl">Designation / Post</label>
<asp:DropDownList ID="ddlPosts"
runat="server"
CssClass="form-control">
</asp:DropDownList>
</div>

</div>

<asp:Panel ID="pnlWorkingAsHead" runat="server" Visible="false" CssClass="working-head-panel">
    <div class="working-head-copy">
        <span class="working-head-kicker">SCHOOL LEADERSHIP DUTY</span>
        <strong>Is this teacher currently working as Head?</strong>
        <small><asp:Label ID="lblWorkingHeadReason" runat="server" Text="This option is available because a Principal, Senior Headmaster or Headmaster position is vacant." /></small>
    </div>
    <asp:RadioButtonList ID="rblWorkingAsHead" runat="server" CssClass="working-head-options" RepeatDirection="Horizontal">
        <asp:ListItem Value="Yes">Yes</asp:ListItem>
        <asp:ListItem Value="No" Selected="True">No</asp:ListItem>
    </asp:RadioButtonList>
</asp:Panel>


<div class="section-title">
Academic Information
</div>

<div class="row">

<div class="col-md-4">
<label class="lbl">Qualification</label>
<asp:TextBox ID="txtQualification"
runat="server"
CssClass="form-control" />
</div>

<div class="col-md-4">
<label class="lbl">Professional Qualification</label>
<asp:TextBox ID="txtProfessionalQualification"
runat="server"
CssClass="form-control" />
</div>

<div class="col-md-4">
<label class="lbl">Personal Number</label>
<asp:TextBox ID="txtPersonalNo"
runat="server"
CssClass="form-control" />
</div>

</div>

<div class="section-title">
Service Information
</div>

<div class="row">

<div class="col-md-4">
<label class="lbl">Date of Birth</label>
<asp:TextBox ID="txtDOB"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

<div class="col-md-4">
<label class="lbl">Date of Joining Govt Service</label>
<asp:TextBox ID="txtJoiningGovt"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

<div class="col-md-4">
<label class="lbl">Date of Joining in School</label>
<asp:TextBox ID="txtJoiningSchool"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

</div>

<br />

<div class="row">

<div class="col-md-4">
<label class="lbl">Date of Regular Appointment</label>
<asp:TextBox ID="txtRegularAppointment"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

<div class="col-md-4">
<label class="lbl">Date of Contract Appointment</label>
<asp:TextBox ID="txtContractAppointment"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

<div class="col-md-4">
<label class="lbl">Date of Award of Current Grade</label>
<asp:TextBox ID="txtCurrentGrade"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

</div>

<div class="section-title">
Address Information
</div>

<div class="row">

<div class="col-md-12">
<label class="lbl">Address</label>
<asp:TextBox ID="txtAddress"
runat="server"
CssClass="form-control"
TextMode="MultiLine"
Rows="3" />
</div>

</div>

<br />

<div class="text-center mt-4">

<asp:Button ID="btnUpdate"
runat="server"
Text="Update Teacher"
CssClass="btn btn-success btn-lg"
OnClick="btnUpdate_Click" />

&nbsp;

<asp:Button ID="btnDelete"
runat="server"
Text="Delete"
CssClass="btn btn-danger btn-lg"
OnClick="btnDelete_Click"
OnClientClick="return confirm('Delete Teacher Record ?');" />

&nbsp;

<asp:Button ID="btnClear"
runat="server"
Text="Clear Form"
CssClass="btn btn-secondary btn-lg"
OnClick="btnClear_Click" />

</div>
<asp:GridView ID="gvTeachers"
runat="server"
AutoGenerateColumns="False"
CssClass="table table-bordered table-hover"
Width="100%"
OnRowDataBound="gvTeachers_RowDataBound"
DataKeyNames="teacherid">

<HeaderStyle CssClass="grid-header" />

<Columns>

<asp:BoundField
DataField="teacherid"
HeaderText="ID" />

<asp:TemplateField HeaderText="Photo">

<ItemTemplate>

<asp:Image ID="imgTeacherGrid"
runat="server"
CssClass="teacher-grid-photo"
AlternateText="Teacher photo" />

</ItemTemplate>

</asp:TemplateField>

<asp:BoundField
DataField="Name"
HeaderText="Teacher Name" />

<asp:BoundField
DataField="fathername"
HeaderText="Father Name" />

<asp:BoundField
DataField="cnicno"
HeaderText="CNIC" />

<asp:BoundField
DataField="contactno"
HeaderText="Contact" />

<asp:BoundField
DataField="qualification"
HeaderText="Qualification" />

<asp:BoundField
DataField="Professionalqualification"
HeaderText="Professional Qualification" />

<asp:TemplateField HeaderText="Head Duty">
<ItemTemplate>
<span class='<%# Convert.ToBoolean(Eval("IsWorkingAsHead")) ? "working-head-badge active" : "working-head-badge" %>'><%# Convert.ToBoolean(Eval("IsWorkingAsHead")) ? "Working as Head" : "No" %></span>
</ItemTemplate>
</asp:TemplateField>

</Columns>

</asp:GridView>
<script>

function previewTeacher(input)
{
    if(input.files && input.files[0])
    {
        var reader = new FileReader();

        reader.onload = function(e)
        {
            document.getElementById('<%= imgTeacher.ClientID %>').src =
                e.target.result;
        };

        reader.readAsDataURL(input.files[0]);
    }
}

</script>
</asp:Content>
