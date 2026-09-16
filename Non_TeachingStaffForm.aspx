<%@ Page Title="Register Non-Teaching Staff" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="Non_TeachingStaffForm.aspx.cs" Inherits="DigitalSchoolManager.WebForm7" %>
  
<asp:Content ID="Content1"
ContentPlaceHolderID="MainContent"
runat="server">


<div class="container-fluid">

<div class="page-title">
    Non-Teaching Staff Registration
</div>

<div class="form-card">

<!-- Personal Information -->

<div class="section-title">
    Personal Information
</div>

<div class="row">

<div class="col-md-3 text-center">

<asp:Image ID="imgStaff"
    runat="server"
    ImageUrl="~/images/noimage.png"
    CssClass="photo-box" />

<br /><br />

<asp:FileUpload ID="fuImage"
    runat="server"
    CssClass="form-control"
    accept=".jpg,.jpeg,.png,.gif,.bmp"
    onchange="PreviewImage(this);" />
<small class="text-muted d-block mt-2">Stored in the database at a maximum of 1 MB. Large images are optimized automatically.</small>

</div>

<div class="col-md-9">

<div class="row">

<div class="col-md-4">
<label class="label-title">Name</label>
<asp:TextBox ID="txtName"
runat="server"
CssClass="form-control"></asp:TextBox>
    <asp:RequiredFieldValidator
ID="rfvName"
runat="server"
ControlToValidate="txtName"
ErrorMessage="Name Required"
ForeColor="Red"
Display="Dynamic" />
</div>

<div class="col-md-4">
<label class="label-title">Father Name</label>
<asp:TextBox ID="txtFatherName"
runat="server"
CssClass="form-control"></asp:TextBox>
    <asp:RequiredFieldValidator
ID="rfvFather"
runat="server"
ControlToValidate="txtFatherName"
ErrorMessage="Father Name Required"
ForeColor="Red" />
</div>

<div class="col-md-4">
<label class="label-title">Gender</label>
<asp:DropDownList ID="ddlGender"
runat="server"
CssClass="form-control">
    <asp:ListItem Text="-- Select --" Value="" />
    <asp:ListItem Text="Male" Value="Male" />
    <asp:ListItem Text="Female" Value="Female" />
</asp:DropDownList>
    <asp:RequiredFieldValidator
ID="rfvGender"
runat="server"
ControlToValidate="ddlGender"
InitialValue=""
ErrorMessage="Select Gender"
ForeColor="Red" />
</div>

</div>

<div class="row">

<div class="col-md-4">
<label class="label-title">Date of Birth</label>
<asp:TextBox ID="txtDOB"
runat="server"
TextMode="Date"
CssClass="form-control"></asp:TextBox>
</div>

<div class="col-md-4">
<label class="label-title">CNIC No</label>
<asp:TextBox ID="txtCNIC"
runat="server"
CssClass="form-control"></asp:TextBox>
    <asp:RegularExpressionValidator
ID="revCNIC"
runat="server"
ControlToValidate="txtCNIC"
ValidationExpression="^\d{5}-\d{7}-\d{1}$"
ErrorMessage="Format: 12345-1234567-1"
ForeColor="Red" />
</div>

<div class="col-md-4">
<label class="label-title">Contact No</label>
<asp:TextBox ID="txtContact"
runat="server"
CssClass="form-control"></asp:TextBox>
    <asp:RegularExpressionValidator
ID="revMobile"
runat="server"
ControlToValidate="txtContact"
ValidationExpression="^03\d{9}$"
ErrorMessage="Format: 03XXXXXXXXX"
ForeColor="Red" />
</div>

</div>

</div>

</div>

<!-- Contact Information -->

<div class="section-title">
    Contact Information
</div>

<div class="row">

<div class="col-md-6">
<label class="label-title">Email Address</label>
<asp:TextBox ID="txtEmail"
runat="server"
CssClass="form-control"></asp:TextBox>
    <asp:RegularExpressionValidator
ID="revEmail"
runat="server"
ControlToValidate="txtEmail"
ValidationExpression="\w+([-.+']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
ErrorMessage="Invalid Email"
ForeColor="Red" />
</div>

<div class="col-md-6">
<label class="label-title">Address</label>
<asp:TextBox ID="txtAddress"
runat="server"
CssClass="form-control"></asp:TextBox>
</div>

</div>

<!-- Academic Information -->

<div class="section-title">
    Academic Information
</div>

<div class="row">

<div class="col-md-4">
<label class="label-title">Marital Status</label>
<asp:DropDownList ID="ddlMaritalStatus"
runat="server"
CssClass="form-control">
    <asp:ListItem Text="Single" Value="Single" />
    <asp:ListItem Text="Married" Value="Married" />
</asp:DropDownList>
</div>

<div class="col-md-4">
<label class="label-title">Qualification</label>
<asp:TextBox ID="txtQualification"
runat="server"
CssClass="form-control"></asp:TextBox>
</div>

<div class="col-md-4">
<label class="label-title">Personal No</label>
<asp:TextBox ID="txtPersonalNo"
runat="server"
CssClass="form-control"></asp:TextBox>
</div>

</div>

<!-- Service Information -->

<div class="section-title">
    Service Information
</div>

<div class="row">

<div class="col-md-3">
<label class="label-title">Date of Joining</label>
<asp:TextBox ID="txtDateJoining"
runat="server"
TextMode="Date"
CssClass="form-control"></asp:TextBox>
</div>

<div class="col-md-3">
<label class="label-title">Joining in School</label>
<asp:TextBox ID="txtJoiningSchool"
runat="server"
TextMode="Date"
CssClass="form-control"></asp:TextBox>
</div>

<div class="col-md-3">
<label class="label-title">First Appointment</label>
<asp:TextBox ID="txtFirstAppointment"
runat="server"
TextMode="Date"
CssClass="form-control"></asp:TextBox>
</div>

<div class="col-md-3">
<label class="label-title">Contract Appointment</label>
<asp:TextBox ID="txtContractAppointment"
runat="server"
TextMode="Date"
CssClass="form-control"></asp:TextBox>
</div>

</div>

<div class="row">

<div class="col-md-3">
<label class="label-title">Regular Appointment Date</label>
<asp:TextBox ID="txtRegularAppointmentDate"
runat="server"
TextMode="Date"
CssClass="form-control"></asp:TextBox>
</div>

<div class="col-md-3">
<label class="label-title">Date of Award Current Grade</label>
<asp:TextBox ID="txtDateOfAwardCurrentGrade"
runat="server"
TextMode="Date"
CssClass="form-control"></asp:TextBox>
</div>

</div>
<div class="row">

<div class="col-md-6">
<label class="label-title">Designation / Post</label>
<asp:DropDownList ID="ddlPost"
runat="server"
CssClass="auto-style1" Height="43px" Width="192px">
</asp:DropDownList>
    <asp:RequiredFieldValidator
ID="rfvPost"
runat="server"
ControlToValidate="ddlPost"
InitialValue=""
ErrorMessage="Select Designation"
ForeColor="Red" />
</div>

</div>

<br />

<asp:Button ID="btnSave"
runat="server"
Text="Save Record"
CssClass="btn btn-success btn-lg"
OnClick="btnSave_Click" />
&nbsp;

<asp:Button ID="btnClear"
runat="server"
Text="Clear"
CssClass="btn btn-secondary btn-lg" OnClick="btnClear_Click" />
</div>

</div>
<!-- ========== STAFF RECORDS GRIDVIEW ========== -->
<div class="form-card mt-4">

<div class="section-title">
    Non-Teaching Staff Records
</div>

<!-- Print Button - will NOT appear on print -->
<div class="mb-3 no-print">
<button type="button"
    onclick="PrintGrid()"
    class="btn btn-primary">
     Print Staff Statement
</button>
</div>

<!-- Printable area starts here -->
<div id="printArea">

<div id="printHeader" class="print-heading-hidden">
    <h3 class="u-text-success-bold">Non-Teaching Staff Statement</h3>
    <hr/>
</div>

<div class="table-responsive">
<asp:GridView ID="gvStaff"
    runat="server"
     AutoGenerateColumns="False"
    OnRowDataBound="gvStaff_RowDataBound"
    CssClass="table table-bordered table-striped table-hover"
    EmptyDataText="No records found."
    HeaderStyle-BackColor="#198754"
    HeaderStyle-ForeColor="White"
    AllowPaging="True"
    PageSize="10"
    OnPageIndexChanging="gvStaff_PageIndexChanging">
    
   
    <Columns>

   <asp:TemplateField HeaderText="Photo">
    <ItemTemplate>

        <asp:Image ID="imgGridStaff"
            runat="server"
            Width="80"
            Height="100" />
    </ItemTemplate>
</asp:TemplateField>

        <asp:BoundField DataField="StaffID"                   HeaderText="ID"                  />
        <asp:BoundField DataField="Name"                      HeaderText="Name"                />
        <asp:BoundField DataField="FatherName"                HeaderText="Father Name"         />
        <asp:BoundField DataField="Gender"                    HeaderText="Gender"              />
        <asp:BoundField DataField="DOB"                       HeaderText="Date of Birth"       DataFormatString="{0:dd-MMM-yyyy}" />
        <asp:BoundField DataField="CNICNo"                    HeaderText="CNIC"                />
        <asp:BoundField DataField="ContactNo"                 HeaderText="Contact No"          />
        <asp:BoundField DataField="EmailID"                   HeaderText="Email"               />
        <asp:BoundField DataField="MaritalStatus"             HeaderText="Marital Status"      />
        <asp:BoundField DataField="Qualification"             HeaderText="Qualification"       />
        <asp:BoundField DataField="PersonalNo"                HeaderText="Personal No"         />
        <asp:BoundField DataField="DateOfJoining"             HeaderText="Date of Joining"     DataFormatString="{0:dd-MMM-yyyy}" />
        <asp:BoundField DataField="DateOfJoiningInThisSchool" HeaderText="Joining in School"   DataFormatString="{0:dd-MMM-yyyy}" />
        <asp:BoundField DataField="DateOfFirstAppointment"    HeaderText="First Appointment"   DataFormatString="{0:dd-MMM-yyyy}" />
        <asp:BoundField DataField="DateOfContractAppointment" HeaderText="Contract Appointment" DataFormatString="{0:dd-MMM-yyyy}" />
        <asp:BoundField DataField="RegularAppointmentDate"    HeaderText="Regular Appointment" DataFormatString="{0:dd-MMM-yyyy}" />
        <asp:BoundField DataField="DateOfAwardCurrentGrade"   HeaderText="Award Current Grade" DataFormatString="{0:dd-MMM-yyyy}" />
        <asp:BoundField DataField="Description"               HeaderText="Designation"         />

    </Columns>

</asp:GridView>
</div>

</div>
<!-- Printable area ends here -->

</div>
<script type="text/javascript">

    function PreviewImage(input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                document.getElementById('<%= imgStaff.ClientID %>')
                    .src = e.target.result;
            };
            reader.readAsDataURL(input.files[0]);
        }
    }

    function PrintGrid() {
        // Show print header only during print
        document.getElementById('printHeader').style.display = 'block';
        window.print();
        document.getElementById('printHeader').style.display = 'none';
    }

</script>
</asp:Content>
