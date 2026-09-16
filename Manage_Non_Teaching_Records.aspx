<%@ Page Title="Manage Non-Teaching Staff" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="Manage_Non_Teaching_Records.aspx.cs" Inherits="DigitalSchoolManager.WebForm9" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-title">
    Manage Non-Teaching Staff
</div>
    <div class="form-card">

<div class="section-title">
Search Staff Record
</div>

<div class="row">

<div class="col-md-4">
<label class="label-title">
Select Staff
</label>

<asp:DropDownList ID="ddlStaff"
runat="server"
CssClass="form-control"
AutoPostBack="true"
OnSelectedIndexChanged="ddlStaff_SelectedIndexChanged">
</asp:DropDownList>
</div>

<div class="col-md-4">
<label class="label-title">Search by Name / CNIC</label>

<asp:TextBox ID="txtSearchStaff"
runat="server"
CssClass="form-control"
placeholder="Enter staff name or CNIC" />

<div class="u-mt-8">
<asp:Button ID="btnSearch"
runat="server"
Text="Search / Load Record"
CssClass="btn btn-success"
CausesValidation="false"
OnClick="btnSearch_Click" />
</div>

</div>

</div>

</div>
<div class="form-card mt-3">

<div class="section-title">
Staff Information
</div>

<asp:HiddenField ID="hfSelectedStaffID" runat="server" />

<div class="alert alert-info u-mb-15">
    <strong>Selected Record:</strong>
    <asp:Label ID="lblSelectedStaff" runat="server" Text="No staff record selected." />
</div>

<div class="row">

<div class="col-md-3 text-center">

<asp:Image ID="imgStaff"
    runat="server"
    ImageUrl="~/images/noimage.png"
    Width="180px"
    Height="220px"
    CssClass="img-thumbnail" />

<br /><br />

<asp:FileUpload ID="fuImage"
    runat="server"
    CssClass="form-control"
    accept=".jpg,.jpeg,.png,.gif,.bmp"
    onchange="previewImage(this);" />
<small class="text-muted d-block mt-2">Replacement images are automatically optimized to a maximum database size of 1 MB.</small>

</div>

<div class="col-md-9">

<div class="row">

<div class="col-md-4">
<label>Name</label>
<asp:TextBox ID="txtName"
runat="server"
CssClass="form-control" />
    <asp:RequiredFieldValidator
    runat="server"
    ControlToValidate="txtName"
    ValidationGroup="UpdateStaff"
    ErrorMessage="Name Required"
    ForeColor="Red" />
</div>

<div class="col-md-4">
<label>Father Name</label>
<asp:TextBox ID="txtFatherName"
runat="server"
CssClass="form-control" />
</div>

<div class="col-md-4">
<label>Gender</label>
<asp:DropDownList ID="ddlGender"
runat="server"
CssClass="form-control">

<asp:ListItem Text="Male" />
<asp:ListItem Text="Female" />

</asp:DropDownList>
</div>

</div>

<div class="row">

<div class="col-md-4">
<label>Date of Birth</label>
<asp:TextBox ID="txtDOB"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

<div class="col-md-4">
<label>CNIC</label>
<asp:TextBox ID="txtCNIC"
runat="server"
CssClass="form-control" />
    <asp:RegularExpressionValidator
    runat="server"
    ControlToValidate="txtCNIC"
    ValidationGroup="UpdateStaff"
    ValidationExpression="^\d{5}-\d{7}-\d{1}$"
    ErrorMessage="12345-1234567-1"
    ForeColor="Red" />
</div>

<div class="col-md-4">
<label>Contact No</label>
<asp:TextBox ID="txtContact"
runat="server"
CssClass="form-control" />
    <asp:RegularExpressionValidator
    runat="server"
    ControlToValidate="txtContact"
    ValidationGroup="UpdateStaff"
    ValidationExpression="^03\d{9}$"
    ErrorMessage="03XXXXXXXXX"
    ForeColor="Red" />
</div>

</div>

<div class="row">

<div class="col-md-4">
<label>Email</label>
<asp:TextBox ID="txtEmail"
runat="server"
CssClass="form-control" />
    <asp:RegularExpressionValidator
    runat="server"
    ControlToValidate="txtEmail"
    ValidationGroup="UpdateStaff"
    ValidationExpression="\w+([-.+']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
    ErrorMessage="Invalid Email"
    ForeColor="Red" />
</div>

<div class="col-md-4">
<label>Marital Status</label>
<asp:DropDownList ID="ddlMaritalStatus"
runat="server"
CssClass="form-control">

<asp:ListItem Text="Single" />
<asp:ListItem Text="Married" />

</asp:DropDownList>
</div>

<div class="col-md-4">
<label>Designation</label>
<asp:DropDownList ID="ddlPost"
runat="server"
CssClass="form-control">
</asp:DropDownList>
    <asp:RequiredFieldValidator
    runat="server"
    ControlToValidate="ddlPost"
    ValidationGroup="UpdateStaff"
    InitialValue=""
    ErrorMessage="Select Designation"
    ForeColor="Red" />
</div>

</div>

<div class="row">

<div class="col-md-12">
<label>Address</label>

<asp:TextBox ID="txtAddress"
runat="server"
CssClass="form-control" />
</div>

</div>

<div class="row">

<div class="col-md-4">
<label>Qualification</label>
<asp:TextBox ID="txtQualification"
runat="server"
CssClass="form-control" />
</div>

<div class="col-md-4">
<label>Personal No</label>
<asp:TextBox ID="txtPersonalNo"
runat="server"
CssClass="form-control" />
</div>

</div>

<div class="row">

<div class="col-md-3">
<label>Date of Joining</label>
<asp:TextBox ID="txtDateJoining"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

<div class="col-md-3">
<label>Joining In School</label>
<asp:TextBox ID="txtJoiningSchool"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

<div class="col-md-3">
<label>First Appointment</label>
<asp:TextBox ID="txtFirstAppointment"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>
<div class="col-md-3">
<label>Contract Appointment</label>
<asp:TextBox ID="txtContractAppointment"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

</div>

<div class="row mt-2">

<div class="col-md-3">
<label>Regular Appointment Date</label>
<asp:TextBox ID="txtRegularAppointmentDate"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

<div class="col-md-3">
<label>Date of Award Current Grade</label>
<asp:TextBox ID="txtDateOfAwardCurrentGrade"
runat="server"
TextMode="Date"
CssClass="form-control" />
</div>

</div>

</div>

</div>

<hr />

<div class="text-center">

<asp:Button ID="btnUpdate"
runat="server"
Text="Update Record"
CssClass="btn btn-success btn-lg"
ValidationGroup="UpdateStaff"
OnClick="btnUpdate_Click" />

&nbsp;

<asp:Button ID="btnClear"
runat="server"
Text="Clear"
CssClass="btn btn-secondary btn-lg"
CausesValidation="false"
OnClick="btnClear_Click" />

</div>

</div>
<!-- ===== STAFF QUICK VIEW GRIDVIEW ===== -->
<div class="form-card mt-4">

<div class="section-title">
    Staff Records - Quick View
</div>

<div class="table-responsive">
<asp:GridView ID="gvStaff"
    runat="server"
    OnRowDataBound="gvStaff_RowDataBound"
    CssClass="table table-bordered table-striped table-hover"
    AutoGenerateColumns="False"
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
            Width="80px"
            Height="100px"
            CssClass="staff-photo" />
    </ItemTemplate>
</asp:TemplateField>

        <asp:BoundField DataField="StaffID"   HeaderText="ID"         />
        <asp:BoundField DataField="Name"      HeaderText="Staff Name" />
        <asp:BoundField DataField="CNICNo"    HeaderText="CNIC No"    />
        <asp:BoundField DataField="ContactNo" HeaderText="Contact No" />

    </Columns>

</asp:GridView>
</div>

</div>
    <script type="text/javascript">

        function previewImage(input) {
            if (input.files && input.files[0]) {
                var reader = new FileReader();

                reader.onload = function (e) {
                    document.getElementById('<%= imgStaff.ClientID %>').src =
                        e.target.result;
                };

                reader.readAsDataURL(input.files[0]);
            }
        }

    </script>
</asp:Content>
