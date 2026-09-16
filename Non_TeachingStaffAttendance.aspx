<%@ Page Title="Non-Teaching Staff Attendance" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true"
    CodeBehind="Non_TeachingStaffAttendance.aspx.cs"
    Inherits="SchoolManagement.NonTeachingStaffAttendance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="attendance-page attendance-shell-v4">

        <header class="attendance-hero-v4">
            <div>
                <span class="attendance-eyebrow-v4">Support Staff Operations</span>
                <h1>Non-Teaching Staff Attendance</h1>
                <p>Record daily attendance and review saved history for all support staff members.</p>
            </div>
            <div class="attendance-legend-v4" aria-label="Attendance color guide">
                <span class="legend-present">Present</span>
                <span class="legend-absent">Absent</span>
                <span class="legend-leave">Leave</span>
            </div>
        </header>

        <asp:Panel ID="pnlAlert" runat="server"
            Visible="false"
            CssClass="message-panel">
            <div class="alert-box alert-success">
                <asp:Label ID="lblAlert" runat="server" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlError" runat="server"
            Visible="false"
            CssClass="message-panel">
            <div class="alert-box alert-error">
                <asp:Label ID="lblError" runat="server" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlMarkMode" runat="server">
            <div class="mode-bar">
                <div>
                    <span class="mode-kicker-v4">Daily Attendance Sheet</span>
                    <div class="mode-title">Mark Non-Teaching Staff Attendance</div>
                </div>

                <div class="date-selector">
                    <label>Attendance Date</label>
                    <asp:TextBox ID="txtAttendanceDate" runat="server"
                        CssClass="date-input"
                        TextMode="Date"
                        AutoPostBack="true"
                        OnTextChanged="txtAttendanceDate_TextChanged" />
                </div>
            </div>

            <asp:Panel ID="pnlNonTeachingClosedDay" runat="server" Visible="false"
                CssClass="attendance-closed-day" role="alert">
                <strong>School is closed on this date</strong>
                <span><asp:Label ID="lblNonTeachingClosedDay" runat="server" /></span>
                <small>Non-teaching staff attendance cannot be marked on a Sunday or registered school holiday.</small>
            </asp:Panel>
        </asp:Panel>

        <section class="attendance-calendar-card no-print" aria-labelledby="nonTeachingHolidayHeading">
            <div class="attendance-calendar-heading">
                <div>
                    <span>Shared School Calendar</span>
                    <h2 id="nonTeachingHolidayHeading">Sundays and Holiday Periods</h2>
                    <p>Add one-day holidays or complete periods such as Eid, summer and winter vacations. The period applies to student, teaching and non-teaching attendance.</p>
                </div>
                <span class="attendance-calendar-badge">School-wide</span>
            </div>
            <div class="attendance-calendar-form">
                <div>
                    <label for="<%= txtNonTeachingHolidayStartDate.ClientID %>">Start date</label>
                    <asp:TextBox ID="txtNonTeachingHolidayStartDate" runat="server" TextMode="Date" />
                </div>
                <div>
                    <label for="<%= txtNonTeachingHolidayEndDate.ClientID %>">End date</label>
                    <asp:TextBox ID="txtNonTeachingHolidayEndDate" runat="server" TextMode="Date" />
                </div>
                <div class="attendance-calendar-name-field">
                    <label for="<%= txtNonTeachingHolidayName.ClientID %>">Holiday name</label>
                    <asp:TextBox ID="txtNonTeachingHolidayName" runat="server" MaxLength="150"
                        placeholder="For example: Summer Vacations, Eid-ul-Fitr" />
                </div>
                <asp:Button ID="btnSaveNonTeachingHoliday" runat="server" Text="Save School Holiday"
                    CssClass="attendance-calendar-save" CausesValidation="false"
                    OnClick="btnSaveNonTeachingHoliday_Click" />
            </div>
            <div class="attendance-calendar-table-wrap">
                <asp:GridView ID="gvNonTeachingHolidays" runat="server" AutoGenerateColumns="false"
                    CssClass="attendance-calendar-table" GridLines="None"
                    OnRowCommand="gvNonTeachingHolidays_RowCommand"
                    EmptyDataText="No custom school holiday periods have been added.">
                    <Columns>
                        <asp:BoundField DataField="HolidayName" HeaderText="Holiday" />
                        <asp:BoundField DataField="HolidayStartDate" HeaderText="Start date" DataFormatString="{0:ddd, dd MMM yyyy}" />
                        <asp:BoundField DataField="HolidayEndDate" HeaderText="End date" DataFormatString="{0:ddd, dd MMM yyyy}" />
                        <asp:BoundField DataField="DurationDays" HeaderText="Days" />
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnDeleteNonTeachingHoliday" runat="server" Text="Remove"
                                    CssClass="attendance-calendar-remove" CausesValidation="false"
                                    CommandName="DeleteHoliday" CommandArgument='<%# Eval("HolidayID") %>'
                                    OnClientClick="return confirm('Remove this complete school holiday period?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </section>

        <asp:Panel ID="pnlHistoryNote" runat="server"
            Visible="false"
            CssClass="history-note">
            Attendance search results are read-only. Click
            <strong>Back to Attendance Marking</strong>
            to return to the live attendance sheet.
        </asp:Panel>

        <div class="grid-wrap">
            <asp:GridView ID="gvAttendance" runat="server"
                AutoGenerateColumns="false"
                CssClass="attendance-grid"
                DataKeyNames="StaffID"
                EmptyDataText="No non-teaching staff members or attendance records found.">

                <Columns>

                    <asp:TemplateField HeaderText="Staff Image"
                        HeaderStyle-CssClass="col-image"
                        ItemStyle-CssClass="image-cell col-image">
                        <ItemTemplate>
                            <%# GetAvatarHtml(Eval("Image"), Eval("StaffName").ToString()) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Staff Name"
                        HeaderStyle-CssClass="col-name"
                        ItemStyle-CssClass="col-name">
                        <ItemTemplate>
                            <span class="staff-name"><%# Eval("StaffName") %></span>
                            <asp:HiddenField ID="hfStaffID" runat="server"
                                Value='<%# Eval("StaffID") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Contact No"
                        HeaderStyle-CssClass="col-contact"
                        ItemStyle-CssClass="col-contact">
                        <ItemTemplate>
                            <span class="contact-text"><%# Eval("ContactNo") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Attendance Status"
                        HeaderStyle-CssClass="col-status"
                        ItemStyle-CssClass="col-status">

                        <ItemTemplate>
                            <asp:Panel ID="pnlEditableStatus"
                                runat="server"
                                Visible='<%# !IsHistoryMode %>'
                                CssClass="status-editor">

                                <label class="status-option status-present">
                                    <asp:RadioButton ID="rbPresent"
                                        runat="server"
                                        GroupName="AttendanceStatus"
                                        Checked="true" />
                                    <span>Present</span>
                                </label>

                                <label class="status-option status-absent">
                                    <asp:RadioButton ID="rbAbsent"
                                        runat="server"
                                        GroupName="AttendanceStatus" />
                                    <span>Absent</span>
                                </label>

                                <label class="status-option status-casual">
                                    <asp:RadioButton ID="rbCasual"
                                        runat="server"
                                        GroupName="AttendanceStatus" />
                                    <span>Casual Leave</span>
                                </label>
                            </asp:Panel>

                            <asp:Literal ID="litReadStatus"
                                runat="server"
                                Visible='<%# IsHistoryMode %>'
                                Text='<%# GetReadOnlyStatusHtml(Eval("AttendanceStatus")) %>' />
                        </ItemTemplate>

                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Attendance Date"
                        HeaderStyle-CssClass="col-date"
                        ItemStyle-CssClass="col-date">
                        <ItemTemplate>
                            <span class="date-read">
                                <%# IsHistoryMode
                                    ? Convert.ToDateTime(Eval("AttendanceDate")).ToString("dd-MM-yyyy")
                                    : txtAttendanceDate.Text %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Total number of absents"
                        HeaderStyle-CssClass="col-absent"
                        ItemStyle-CssClass="col-absent">
                        <ItemTemplate>
                            <span class="count-text"><%# Eval("TotalAbsents") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Total Number of Casual Leaves"
                        HeaderStyle-CssClass="col-casual"
                        ItemStyle-CssClass="col-casual">
                        <ItemTemplate>
                            <span class="count-text"><%# Eval("TotalCasualLeaves") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>
            </asp:GridView>
        </div>

        <asp:Panel ID="pnlSubmit" runat="server"
            CssClass="submit-row">
            <asp:Button ID="btnSubmitAttendance"
                runat="server"
                Text="Submit Attendance"
                CssClass="submit-btn"
                CausesValidation="false"
                OnClick="btnSubmitAttendance_Click"
                OnClientClick="return confirm('Submit attendance for all staff members shown in the table?');" />
        </asp:Panel>

        <div class="bottom-controls">

            <div class="control-block">
                <div class="control-title">Search Staff by Name</div>

                <asp:TextBox ID="txtSearchName"
                    runat="server"
                    CssClass="search-input"
                    placeholder="Staff name" />

                <asp:Button ID="btnSearchStaff"
                    runat="server"
                    Text="Search"
                    CssClass="small-btn"
                    CausesValidation="false"
                    OnClick="btnSearchStaff_Click" />

                <asp:Button ID="btnShowAllStaff"
                    runat="server"
                    Text="Show All"
                    CssClass="small-btn secondary-btn"
                    CausesValidation="false"
                    OnClick="btnShowAllStaff_Click" />
            </div>

            <div class="control-block">
                <div class="control-title">Search Attendance by DATE</div>

                <div class="date-range-box">
                    <span>Select Date from</span>
                    <asp:TextBox ID="txtFromDate"
                        runat="server"
                        CssClass="range-date"
                        TextMode="Date" />

                    <span>Select Date To</span>
                    <asp:TextBox ID="txtToDate"
                        runat="server"
                        CssClass="range-date"
                        TextMode="Date" />
                </div>

                <div class="range-actions">
                    <asp:Button ID="btnSearchDates"
                        runat="server"
                        Text="Search"
                        CssClass="small-btn"
                        CausesValidation="false"
                        OnClick="btnSearchDates_Click" />

                    <asp:Button ID="btnBackToMarking"
                        runat="server"
                        Text="Back to Attendance Marking"
                        CssClass="small-btn secondary-btn"
                        CausesValidation="false"
                        OnClick="btnBackToMarking_Click" />
                </div>
            </div>

            <div class="control-block">
                <div class="control-title u-invisible">Print</div>

                <asp:Button ID="btnPrint"
                    runat="server"
                    Text="Print"
                    CssClass="large-btn"
                    CausesValidation="false"
                    OnClick="btnPrint_Click" />
            </div>

            <div class="control-block">
                <div class="control-title u-invisible">Excel</div>

                <asp:Button ID="btnExportExcel"
                    runat="server"
                    Text="Export as Excel"
                    CssClass="large-btn"
                    CausesValidation="false"
                    OnClick="btnExportExcel_Click" />
            </div>

        </div>

        <div class="record-count">
            <asp:Label ID="lblRecordCount" runat="server" />
        </div>

    </div>

    <script type="text/javascript">
        (function () {
            function syncGroup(group) {
                if (!group) return;
                var options = group.querySelectorAll('.status-option');
                for (var i = 0; i < options.length; i++) {
                    var radio = options[i].querySelector('input[type="radio"]');
                    if (radio && radio.checked) {
                        options[i].classList.add('is-selected');
                    } else {
                        options[i].classList.remove('is-selected');
                    }
                }
            }

            function syncAllAttendanceChoices() {
                var groups = document.querySelectorAll('.attendance-shell-v4 .status-editor');
                for (var i = 0; i < groups.length; i++) syncGroup(groups[i]);
            }

            document.addEventListener('change', function (event) {
                var target = event.target;
                if (!target || target.type !== 'radio') return;
                var group = target.closest ? target.closest('.status-editor') : target.parentNode;
                while (group && !((' ' + group.className + ' ').indexOf(' status-editor ') >= 0)) {
                    group = group.parentNode;
                }
                syncGroup(group);
            });

            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', syncAllAttendanceChoices);
            } else {
                syncAllAttendanceChoices();
            }

            if (window.Sys && Sys.Application) {
                Sys.Application.add_load(syncAllAttendanceChoices);
            }
        }());
    </script>

</asp:Content>
