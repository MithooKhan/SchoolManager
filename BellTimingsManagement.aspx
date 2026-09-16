<%@ Page Title="Bell Timings" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="BellTimingsManagement.aspx.cs" Inherits="DigitalSchoolManager.WebForm20" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    

<asp:ScriptManager ID="ScriptManager1" runat="server" />

<div class="page-wrapper">

    <!-- ══ HERO ══════════════════════════════════════════════════════════════ -->
    <div class="hero">
        <div class="hero-accent"></div>
        <div class="hero-badge">School Administration System</div>
        <h1>Bell <span>Timetable</span> Manager</h1>
        <p>Add, update, and manage school bell periods with ease.</p>
    </div>

    <!-- ══ NOTIFICATION ══════════════════════════════════════════════════════ -->
    <asp:UpdatePanel ID="upNotify" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="alert u-mb-18">
                <span class="alert-icon"><asp:Literal ID="litMsgIcon" runat="server" /></span>
                <asp:Literal ID="litMsg" runat="server" />
            </asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- ══ ENTRY FORM CARD ════════════════════════════════════════════════════ -->
    <div class="card">
        <div class="card-header">
            <div class="card-header-icon"></div>
            <div class="card-header-text">
                <h2>Period Entry Form</h2>
                <p>Fill all fields - period names must be unique.</p>
            </div>
        </div>
        <div class="card-body">

            <!-- Hidden BellID for edit mode -->
            <asp:HiddenField ID="hfBellID" runat="server" Value="0" />

            <div class="form-grid">

                <!-- Period Name -->
                <div class="form-group">
                    <label for="txtPeriodName"><span class="req">*</span> Period Name</label>
                    <div class="input-wrap">
                        <span class="input-icon"></span>
                        <asp:TextBox ID="txtPeriodName" runat="server" CssClass="form-control" MaxLength="50"
                            placeholder="e.g. Period 1 / Assembly" />
                    </div>
                    <asp:RequiredFieldValidator ID="rfvPeriodName" runat="server"
                        ControlToValidate="txtPeriodName" ValidationGroup="vgForm"
                        Display="Dynamic" CssClass="validator-msg"
                        ErrorMessage="Period name is required." />
                    <asp:RegularExpressionValidator ID="revPeriodName" runat="server"
                        ControlToValidate="txtPeriodName" ValidationGroup="vgForm"
                        Display="Dynamic" CssClass="validator-msg"
                        ValidationExpression="^[a-zA-Z0-9 \-/]{2,50}$"
                        ErrorMessage="Use 2-50 letters, numbers, spaces, - or /." />
                </div>

                <!-- Start Time -->
                <div class="form-group">
                    <label for="txtStartTime"><span class="req">*</span> Start Time</label>
                    <div class="time-row">
                        <div class="input-wrap">
                            <span class="input-icon"></span>
                            <asp:TextBox ID="txtStartTime" runat="server" CssClass="form-control"
                                MaxLength="8" placeholder="hh:mm" />
                        </div>
                        <asp:DropDownList ID="ddlStartAMPM" runat="server" CssClass="form-control u-pl-8">
                            <asp:ListItem Value="AM">AM</asp:ListItem>
                            <asp:ListItem Value="PM">PM</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <asp:RequiredFieldValidator ID="rfvStartTime" runat="server"
                        ControlToValidate="txtStartTime" ValidationGroup="vgForm"
                        Display="Dynamic" CssClass="validator-msg"
                        ErrorMessage="Start time is required." />
                    <asp:RegularExpressionValidator ID="revStartTime" runat="server"
                        ControlToValidate="txtStartTime" ValidationGroup="vgForm"
                        Display="Dynamic" CssClass="validator-msg"
                        ValidationExpression="^(0?[1-9]|1[0-2]):[0-5][0-9]$"
                        ErrorMessage="Format must be hh:mm (e.g. 08:00 or 8:30)." />
                </div>

                <!-- End Time -->
                <div class="form-group">
                    <label for="txtEndTime"><span class="req">*</span> End Time</label>
                    <div class="time-row">
                        <div class="input-wrap">
                            <span class="input-icon"></span>
                            <asp:TextBox ID="txtEndTime" runat="server" CssClass="form-control"
                                MaxLength="8" placeholder="hh:mm" />
                        </div>
                        <asp:DropDownList ID="ddlEndAMPM" runat="server" CssClass="form-control u-pl-8">
                            <asp:ListItem Value="AM">AM</asp:ListItem>
                            <asp:ListItem Value="PM">PM</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <asp:RequiredFieldValidator ID="rfvEndTime" runat="server"
                        ControlToValidate="txtEndTime" ValidationGroup="vgForm"
                        Display="Dynamic" CssClass="validator-msg"
                        ErrorMessage="End time is required." />
                    <asp:RegularExpressionValidator ID="revEndTime" runat="server"
                        ControlToValidate="txtEndTime" ValidationGroup="vgForm"
                        Display="Dynamic" CssClass="validator-msg"
                        ValidationExpression="^(0?[1-9]|1[0-2]):[0-5][0-9]$"
                        ErrorMessage="Format must be hh:mm (e.g. 02:45 or 1:00)." />
                </div>

            </div><!-- /form-grid -->

            <!-- Action Buttons -->
            <div class="btn-row u-mt-24">
                <asp:Button ID="btnSave"   runat="server" Text="Save Period"
                    CssClass="btn btn-primary" ValidationGroup="vgForm"
                    OnClick="btnSave_Click" />
                <asp:Button ID="btnUpdate" runat="server" Text="Update Period"
                    CssClass="btn btn-primary" ValidationGroup="vgForm"
                    OnClick="btnUpdate_Click" Visible="false" />
                <asp:Button ID="btnClear"  runat="server" Text="Clear Form"
                    CssClass="btn btn-secondary" CausesValidation="false"
                    OnClick="btnClear_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel Edit"
                    CssClass="btn btn-secondary" CausesValidation="false"
                    OnClick="btnClear_Click" Visible="false" />
            </div>

        </div><!-- /card-body -->
    </div><!-- /card -->

    <!-- ══ TIMETABLE GRID CARD ════════════════════════════════════════════════ -->
    <div class="card">
        <div class="card-header">
            <div class="card-header-icon"></div>
            <div class="card-header-text">
                <h2>Bell Timetable</h2>
                <p>All scheduled periods - click Edit or Delete to manage entries.</p>
            </div>
        </div>
        <div class="card-body">

            <!-- Stats Bar -->
            <div class="stats-bar">
                <div class="stat-chip">
                    <div class="val"><asp:Label ID="lblTotalPeriods" runat="server">0</asp:Label></div>
                    <div class="lbl">Total Periods</div>
                </div>
                <div class="stat-chip">
                    <div class="val"><asp:Label ID="lblTotalMins" runat="server">0</asp:Label></div>
                    <div class="lbl">Total Minutes</div>
                </div>
                <div class="stat-chip">
                    <div class="val"><asp:Label ID="lblFirstBell" runat="server">-</asp:Label></div>
                    <div class="lbl">First Bell</div>
                </div>
                <div class="stat-chip">
                    <div class="val"><asp:Label ID="lblLastBell" runat="server">-</asp:Label></div>
                    <div class="lbl">Last Bell</div>
                </div>
            </div>

            <div class="divider">
                <div class="divider-line"></div>
                <div class="divider-text">Period Listing</div>
                <div class="divider-line"></div>
            </div>

            <!-- Printable Area -->
            <div id="printArea">
                <!-- Print-only header (hidden on screen) -->
                <div class="grid-print-header">
                    <img class="print-school-logo" src="<%= ResolveUrl("~/images/SchoolLogo.png") %>"
                        alt="Government Higher Secondary School Maankot logo" />
                    <h2>Government Higher Secondary School Maankot</h2>
                    <h3>School Bell Timetable</h3>
                    <p>Printed on: <%= DateTime.Today.ToString("dd MMMM yyyy") %></p>
                </div>

                <asp:GridView ID="gvBellTimetable" runat="server"
                    AutoGenerateColumns="false"
                    CssClass="gv"
                    GridLines="None"
                    OnRowCommand="gvBellTimetable_RowCommand"
                    EmptyDataText="">

                    <EmptyDataTemplate>
                        <div class="empty-state">
                            <div class="icon"></div>
                            <p>No periods added yet. Use the form above to create your first bell entry.</p>
                        </div>
                    </EmptyDataTemplate>

                    <Columns>
                        <asp:TemplateField HeaderText="#" ItemStyle-CssClass="col-num" HeaderStyle-CssClass="col-num">
                            <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Period Name">
                            <ItemTemplate>
                                <span class="period-name"><%# Eval("PeriodName") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="PeriodStartTime" HeaderText="Start Time" />
                        <asp:BoundField DataField="PeriodEndTime"   HeaderText="End Time"   />

                        <asp:TemplateField HeaderText="Duration">
                            <ItemTemplate>
                                <span class="duration-badge">
                                     <%# Eval("Duration") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="150px" HeaderStyle-CssClass="bell-action-column" ItemStyle-CssClass="bell-action-column">
                            <ItemTemplate>
                                <asp:LinkButton ID="lbEdit" runat="server"
                                    CommandName="EditRow"
                                    CommandArgument='<%# Eval("BellID") %>'
                                    CssClass="gv-btn gv-btn-edit">Edit</asp:LinkButton>
                                &nbsp;
                                <asp:LinkButton ID="lbDelete" runat="server"
                                    CommandName="DeleteRow"
                                    CommandArgument='<%# Eval("BellID") %>'
                                    CssClass="gv-btn gv-btn-del"
                                    OnClientClick="return confirm('Delete this period? This cannot be undone.');">Delete</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div><!-- /printArea -->

            <!-- Print Button -->
            <div class="btn-row u-mt-24-end">
                <button type="button" class="btn btn-print" id="btnPrint" onclick="return printBellTimetable();">
                    <span class="icon"></span> Print Timetable
                </button>
            </div>

        </div><!-- /card-body -->
    </div><!-- /card -->

</div><!-- /page-wrapper -->

<div class="page-footer">
    Copyright <%= DateTime.Now.Year %> School Administration System | Bell Timetable Module
</div>

<script type="text/javascript">
function printBellTimetable() {
    var source = document.getElementById('printArea');
    if (!source) return false;
    var printWindow = window.open('', '_blank', 'width=980,height=760');
    if (!printWindow) { alert('Allow pop-ups to print the bell timetable.'); return false; }
    var css = '<style>@page{size:A4 portrait;margin:12mm}*{box-sizing:border-box}body{font-family:"Times New Roman",serif;color:#000;margin:0}'+
        '.grid-print-header{text-align:center;margin-bottom:14px}.print-school-logo{width:78px;height:78px;object-fit:contain}.grid-print-header h2{margin:4px 0;font-size:22px}.grid-print-header h3{margin:2px 0 4px;font-size:18px}.grid-print-header p{margin:0;font-size:12px}'+
        'table{width:100%;border-collapse:collapse;font-size:12pt}th,td{border:1px solid #000;padding:8px;text-align:left;color:#000!important;background:#fff!important}th{text-align:center;font-weight:bold}.col-num{text-align:center;width:45px}.duration-badge{color:#000!important}.bell-action-column{display:none!important}tr{break-inside:avoid}.empty-state{text-align:center;padding:20px}</style>';
    printWindow.document.open();
    printWindow.document.write('<!doctype html><html><head><meta charset="utf-8"><base href="' + document.baseURI + '"><title>School Bell Timetable</title>' + css + '</head><body>' + source.innerHTML + '</body></html>');
    printWindow.document.close();
    printWindow.focus();
    window.setTimeout(function(){ printWindow.print(); printWindow.close(); }, 350);
    return false;
}
</script>

</asp:Content>
