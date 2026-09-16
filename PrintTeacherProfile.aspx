<%@ Page Title="Teacher Profile" Language="C#" MasterPageFile="~/DSM.Master"
    AutoEventWireup="true" CodeBehind="PrintTeacherProfile.aspx.cs"
    Inherits="DigitalSchoolManager.WebForm4" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">


<div class="teacher-profile-page">

    <div class="search-card no-print">
        <div class="search-title">Search Teacher Bio Data</div>

        <div class="search-grid">

            <div>
                <label class="field-label">Teacher Name</label>
                <asp:TextBox ID="txtNameSearch" runat="server"
                    CssClass="input-control"
                    placeholder="Enter teacher name" />
            </div>

            <div>
                <label class="field-label">CNIC</label>
                <asp:TextBox ID="txtCNICSearch" runat="server"
                    CssClass="input-control"
                    placeholder="Enter CNIC" />
            </div>

            <div>
                <asp:Button ID="btnSearch" runat="server"
                    Text="Search & Load"
                    CssClass="btn-ui btn-search"
                    CausesValidation="false"
                    OnClick="btnSearch_Click" />
            </div>

            <div>
                <asp:Button ID="btnClear" runat="server"
                    Text="Clear"
                    CssClass="btn-ui btn-clear"
                    CausesValidation="false"
                    OnClick="btnClear_Click" />
            </div>
        </div>

        <asp:Panel ID="pnlSearchResults" runat="server"
            Visible="false" CssClass="u-mt-14">

            <label class="field-label">Matching Teachers</label>

            <asp:DropDownList ID="ddlTeachers" runat="server"
                CssClass="input-control"
                AutoPostBack="true"
                OnSelectedIndexChanged="ddlTeachers_SelectedIndexChanged">
            </asp:DropDownList>

        </asp:Panel>

        <asp:Panel ID="pnlMessage" runat="server"
            Visible="false"
            CssClass="message-box">
            <asp:Label ID="lblMessage" runat="server" />
        </asp:Panel>
    </div>

    <asp:Panel ID="pnlProfile" runat="server" Visible="false">

        <div id="printProfile" class="bio-wrapper">

            <div class="bio-header">
                <img class="print-school-logo" src="<%= ResolveUrl("~/images/SchoolLogo.png") %>"
                    alt="Government Higher Secondary School Maankot logo" />
                <strong class="bio-school-name">Government Higher Secondary School Maankot</strong>
                <h1>Teacher Bio Data Form</h1>
                <div class="sub">Official School Record</div>
            </div>

            <div class="bio-body">

                <div class="top-section">
                    <div class="photo-box">
                        <asp:Image ID="imgTeacher" runat="server"
                            AlternateText="Teacher Image" />
                    </div>

                    <div class="name-block">
                        <div class="teacher-name">
                            <asp:Label ID="lblName" runat="server" />
                        </div>

                        <div class="designation">
                            <asp:Label ID="lblDesignation" runat="server" />
                            <asp:Label ID="lblBPS" runat="server" />
                        </div>

                        <div class="cnic-line">
                            CNIC:
                            <asp:Label ID="lblCNIC" runat="server" />
                        </div>
                    </div>
                </div>

                <div class="section-heading">Personal Information</div>

                <div class="bio-grid">

                    <div class="bio-item">
                        <div class="bio-label">Father Name</div>
                        <div class="bio-value">
                            <asp:Label ID="lblFatherName" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Gender</div>
                        <div class="bio-value">
                            <asp:Label ID="lblGender" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Date of Birth</div>
                        <div class="bio-value">
                            <asp:Label ID="lblDOB" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">CNIC No.</div>
                        <div class="bio-value">
                            <asp:Label ID="lblCNIC2" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Contact No.</div>
                        <div class="bio-value">
                            <asp:Label ID="lblContact" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Email</div>
                        <div class="bio-value">
                            <asp:Label ID="lblEmail" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item full-width">
                        <div class="bio-label">Address</div>
                        <div class="bio-value">
                            <asp:Label ID="lblAddress" runat="server" />
                        </div>
                    </div>

                </div>

                <div class="section-heading">Professional Information</div>

                <div class="bio-grid">

                    <div class="bio-item">
                        <div class="bio-label">Qualification</div>
                        <div class="bio-value">
                            <asp:Label ID="lblQualification" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Professional Qualification</div>
                        <div class="bio-value">
                            <asp:Label ID="lblProfessionalQualification" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Personal No.</div>
                        <div class="bio-value">
                            <asp:Label ID="lblPersonalNo" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Designation</div>
                        <div class="bio-value">
                            <asp:Label ID="lblDesignation2" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">BPS</div>
                        <div class="bio-value">
                            <asp:Label ID="lblBPS2" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Joining Govt. Service</div>
                        <div class="bio-value">
                            <asp:Label ID="lblDateOfJoining" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Joining This School</div>
                        <div class="bio-value">
                            <asp:Label ID="lblJoiningSchool" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Regular Appointment</div>
                        <div class="bio-value">
                            <asp:Label ID="lblRegularAppointment" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Contract Appointment</div>
                        <div class="bio-value">
                            <asp:Label ID="lblContractAppointment" runat="server" />
                        </div>
                    </div>

                    <div class="bio-item">
                        <div class="bio-label">Award of Current Grade</div>
                        <div class="bio-value">
                            <asp:Label ID="lblCurrentGradeDate" runat="server" />
                        </div>
                    </div>

                </div>

                <div class="signature-row">
                    <div class="signature-box">
                        <div class="signature-line"></div>
                        Teacher Signature
                    </div>

                    <div class="signature-box">
                        <div class="signature-line"></div>
                        Principal Signature
                    </div>
                </div>

            </div>
        </div>

        <div class="print-actions no-print">
            <asp:Button ID="btnPrint" runat="server"
                Text="Print Bio Data"
                CssClass="btn-ui btn-print"
                CausesValidation="false"
                OnClientClick="window.print(); return false;" />
        </div>

    </asp:Panel>

</div>

</asp:Content>
