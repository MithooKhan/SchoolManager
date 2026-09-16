<%@ Page Title="Teacher Directory" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="PrintAllTeacherList.aspx.cs" Inherits="DigitalSchoolManager.WebForm5" %>
 <asp:Content ID="Content1"
ContentPlaceHolderID="MainContent"
runat="server">


<div class="container-fluid">

<div class="page-title no-print">
Teachers Records List
</div>

<div class="search-panel no-print">

<div class="row">

<div class="col-md-4">

<asp:TextBox
ID="txtSearch"
runat="server"
CssClass="form-control"
placeholder="Search by Name or CNIC">
</asp:TextBox>

</div>

<div class="col-md-2">

<asp:Button
ID="btnSearch"
runat="server"
Text="Search"
CssClass="btn btn-success"
OnClick="btnSearch_Click" />

</div>

<div class="col-md-2">

<asp:Button
ID="btnShowAll"
runat="server"
Text="Show All"
CssClass="btn btn-secondary"
OnClick="btnShowAll_Click" />

</div>

<div class="col-md-2">

<asp:Button
ID="btnPrint"
runat="server"
Text="Print List"
CssClass="btn btn-primary"
OnClientClick="window.print();return false;" OnClick="btnPrint_Click" />

</div>
   
<div>
    <asp:Button
ID="ButtonBioDataPrint"
runat="server"
Text="Print Bio Data Forms"
CssClass="btn btn-primary" PostBackUrl="~/PrintTeacherProfile.aspx" />
</div>
</div>
<div class="teacher-list-note">
    Click Teacher Name to open Bio Data/Profile. Click Teacher Photo to open the official Teacher Card.
</div>
</div>

<div id="printArea">

<div class="list-card">
<div class="directory-print-header">
    <img class="print-school-logo" src="<%= ResolveUrl("~/images/SchoolLogo.png") %>"
        alt="Government Higher Secondary School Maankot logo" />
    <strong>Government Higher Secondary School Maankot</strong>
    <span>Official Teacher Directory</span>
</div>

<asp:GridView
ID="gvTeachers"
runat="server"
AutoGenerateColumns="False"
CssClass="table table-bordered table-striped table-hover"
GridLines="Both">

<HeaderStyle CssClass="grid-header" />

<Columns>

<asp:BoundField DataField="teacherid"
HeaderText="ID" />

<asp:TemplateField HeaderText="Photo">
<ItemTemplate>

<asp:HyperLink ID="lnkTeacherCard"
    runat="server"
    NavigateUrl='<%# GetTeacherCardUrl(Eval("teacherid")) %>'
    Target="_blank"
    ToolTip="Click teacher image to open official Teacher Card">

    <asp:Image ID="imgTeacher"
        runat="server"
        Width="60"
        Height="70"
        CssClass="teacher-list-photo"
        ImageUrl='<%# GetImage(Eval("Image")) %>' />

</asp:HyperLink>

</ItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="Name">
<ItemTemplate>
    <asp:HyperLink
        ID="lnkTeacherProfile"
        runat="server"
        NavigateUrl='<%# GetTeacherProfileUrl(Eval("teacherid")) %>'
        Target="_blank"
        Text='<%# Eval("Name") %>'
        ToolTip="Open Teacher Bio Data / Profile"
        CssClass="teacher-profile-link">
    </asp:HyperLink>
</ItemTemplate>
</asp:TemplateField>

<asp:BoundField DataField="fathername"
HeaderText="Father Name" />

<asp:BoundField DataField="Gender"
HeaderText="Gender" />

<asp:BoundField DataField="dob"
HeaderText="DOB"
DataFormatString="{0:dd-MMM-yyyy}" />

<asp:BoundField DataField="cnicno"
HeaderText="CNIC" />

<asp:BoundField DataField="contactno"
HeaderText="Contact" />

<asp:BoundField DataField="emailId"
HeaderText="Email" />

<asp:BoundField DataField="address"
HeaderText="Address" />

<asp:BoundField DataField="MaritalStatus"
HeaderText="Marital Status" />

<asp:BoundField DataField="qualification"
HeaderText="Qualification" />

<asp:BoundField DataField="Professionalqualification"
HeaderText="Professional Qualification" />

<asp:BoundField DataField="personalNo"
HeaderText="Personal No" />

<asp:BoundField DataField="dateOfjoningGovtService"
HeaderText="Joining Date"
DataFormatString="{0:dd-MMM-yyyy}" />

<asp:BoundField DataField="dateOfjoininginthischool"
HeaderText="Joining School"
DataFormatString="{0:dd-MMM-yyyy}" />

<asp:BoundField DataField="DateofawardofCurrentGrade"
HeaderText="Current Grade Date"
DataFormatString="{0:dd-MMM-yyyy}" />

<asp:BoundField DataField="dateOfcontractappointment"
HeaderText="Contract Appointment"
DataFormatString="{0:dd-MMM-yyyy}" />


<asp:BoundField DataField="dateofRegularappointment"
HeaderText="Regular Appointment"
DataFormatString="{0:dd-MMM-yyyy}" />


<asp:BoundField DataField="Description"
HeaderText="Designation" />

<asp:BoundField DataField="BPS"
HeaderText="BPS" />

</Columns>

</asp:GridView>

</div>

</div>

</div>

</asp:Content>
