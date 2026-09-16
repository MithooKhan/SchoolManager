<%@ Page Title="Student Cards" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="PrintStudentCard.aspx.cs" Inherits="DigitalSchoolManager.WebForm26" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        function triggerPrint() {
            window.print();
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="student-card-page">
        <section class="student-card-toolbar no-print" aria-labelledby="studentCardPageTitle">
            <div class="student-card-toolbar-copy">
                <span class="student-card-eyebrow">Student Services</span>
                <h1 id="studentCardPageTitle">Student Identity Card Generator</h1>
                <p>Select a class and student to create clear, print-ready school identity cards.</p>
            </div>

            <asp:Label ID="lblMessage" runat="server" CssClass="student-card-message"
                EnableViewState="false" role="alert" />

            <div class="student-card-filter-row">
                <div class="student-card-filter">
                    <label class="student-card-label" for="<%= ddlClasses.ClientID %>">Class</label>
                    <asp:DropDownList ID="ddlClasses" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="ddlClasses_SelectedIndexChanged"
                        CssClass="student-card-select" />
                </div>

                <div class="student-card-filter student-card-filter-wide">
                    <label class="student-card-label" for="<%= ddlStudents.ClientID %>">Student</label>
                    <asp:DropDownList ID="ddlStudents" runat="server" AutoPostBack="true"
                        OnSelectedIndexChanged="ddlStudents_SelectedIndexChanged"
                        CssClass="student-card-select" />
                </div>

                <asp:Button ID="btnPrint" runat="server" Text="Print Student Card"
                    OnClientClick="return triggerPrint();" CssClass="student-card-print-button"
                    Enabled="false" />
            </div>
        </section>

        <div class="student-card-grid">
            <asp:Repeater ID="rptStudentCards" runat="server">
                <ItemTemplate>
                    <article class="student-id-card-v3">
                        <div class="student-card-watermark" aria-hidden="true">GHSS</div>

                        <header class="student-id-card-v3-header">
                            <div class="student-school-mark">
                                <img src="images/SchoolLogo.png"
                                    alt="Government Higher Secondary School Maankot logo" />
                            </div>
                            <div class="student-school-title">
                                <strong>Government Higher Secondary School Maankot</strong>
                                <span>Government of Punjab | Official Student Identity Card</span>
                            </div>
                            <span class="student-card-type">STUDENT</span>
                        </header>

                        <div class="student-id-card-v3-body">
                            <div class="student-card-identity">
                                <img class="student-card-photo"
                                    src='<%# GetStudentPhotoUrl(Eval("Image")) %>'
                                    alt="Student photograph" />
                                <span>Registration No</span>
                                <strong><%#: Eval("RegNo") %></strong>
                            </div>

                            <div class="student-card-profile">
                                <span class="student-card-profile-label">Student Name</span>
                                <h2><%#: Eval("Name") %></h2>
                                <dl class="student-card-details">
                                    <div>
                                        <dt>Father Name</dt>
                                        <dd><%#: Eval("FatherName") %></dd>
                                    </div>
                                    <div>
                                        <dt>Class</dt>
                                        <dd><%#: Eval("ClassName") %></dd>
                                    </div>
                                    <div>
                                        <dt>Roll No</dt>
                                        <dd><%#: Eval("StudentRollNo") %></dd>
                                    </div>
                                    <div>
                                        <dt>Contact</dt>
                                        <dd><%#: Eval("ContactNo") %></dd>
                                    </div>
                                </dl>
                            </div>
                        </div>

                        <footer class="student-id-card-v3-footer">
                            <div class="student-card-address">
                                <span>Address</span>
                                <strong><%#: Eval("Address") %></strong>
                            </div>
                            <div class="student-card-signatures">
                                <span>Student Signature</span>
                                <span>Principal Signature</span>
                            </div>
                        </footer>
                    </article>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </div>
</asp:Content>
