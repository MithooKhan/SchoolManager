<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="DigitalSchoolManager.WebForm17" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="dashboard-page">
        <section class="dashboard-hero" aria-labelledby="dashboard-title">
            <div class="dashboard-hero-main">
                <div class="dashboard-hero-emblem"><img src="images/SchoolLogo.png" alt="School logo" /></div>
                <div class="dashboard-hero-copy">
                    <span class="dashboard-eyebrow">Administrative command centre</span>
                    <h1 id="dashboard-title">School Operations Dashboard</h1>
                    <p>A focused, live view of enrolment, attendance, academics, finance and institutional services.</p>
                    <div class="dashboard-hero-tags"><span>Government Higher Secondary School Maankot</span><span>Tehsil Kabirwala, District Khanewal</span></div>
                </div>
            </div>
            <aside class="dashboard-date-card">
                <span>Operating date</span>
                <strong><asp:Label ID="lblCurrentDate" runat="server" /></strong>
                <small><i aria-hidden="true"></i> School database connected</small>
                <a href="PublicDashboard.aspx">Open public school page</a>
            </aside>
        </section>

        <asp:Panel ID="pnlDashboardStatus" runat="server" CssClass="dashboard-status dashboard-status-success" role="status">
            <span class="dashboard-status-dot" aria-hidden="true"></span>
            <asp:Label ID="lblDashboardStatus" runat="server" />
        </asp:Panel>

        <nav class="dashboard-command-strip" aria-label="Frequently used school actions">
            <a href="StudentRegistrationForm.aspx"><span>AD</span><strong>New Admission</strong><small>Register a student</small></a>
            <a href="StudentAttendance.aspx"><span>AT</span><strong>Mark Attendance</strong><small>Open class register</small></a>
            <a href="FeeCollection.aspx"><span>FC</span><strong>Collect Fee</strong><small>Issue fee voucher</small></a>
            <a href="StudentResultEntry.aspx"><span>RS</span><strong>Enter Results</strong><small>Record examination marks</small></a>
            <a href="PublicContentManagement.aspx"><span>NW</span><strong>Publish Update</strong><small>News, audio or video</small></a>
            <a href="DataBackup.aspx"><span>DB</span><strong>Data Backup</strong><small>Protect school records</small></a>
        </nav>

        <section class="dashboard-overview-panel" aria-labelledby="overview-heading">
            <div class="dashboard-section-heading">
                <div>
                    <span class="dashboard-section-kicker">Executive indicators</span>
                    <h2 id="overview-heading">Institutional Overview</h2>
                </div>
                <span class="dashboard-section-note">Values are loaded from SchoolDB</span>
            </div>

            <div class="dashboard-metric-grid">
                <a class="dashboard-metric dashboard-metric-green" href="StudentPrintData.aspx">
                    <span class="dashboard-metric-icon" aria-hidden="true">ST</span>
                    <span class="dashboard-metric-value"><asp:Label ID="lblTotalStudents" runat="server" Text="-" /></span>
                    <span class="dashboard-metric-label">Total Students</span>
                    <span class="dashboard-metric-link">View student records</span>
                </a>

                <a class="dashboard-metric dashboard-metric-blue" href="PrintAllTeacherList.aspx">
                    <span class="dashboard-metric-icon" aria-hidden="true">TS</span>
                    <span class="dashboard-metric-value"><asp:Label ID="lblTotalTeachers" runat="server" Text="-" /></span>
                    <span class="dashboard-metric-label">Teaching Staff</span>
                    <span class="dashboard-metric-link">Open teacher directory</span>
                </a>

                <a class="dashboard-metric dashboard-metric-gold" href="Manage_Non_Teaching_Records.aspx">
                    <span class="dashboard-metric-icon" aria-hidden="true">NS</span>
                    <span class="dashboard-metric-value"><asp:Label ID="lblNonTeachingStaff" runat="server" Text="-" /></span>
                    <span class="dashboard-metric-label">Non-Teaching Staff</span>
                    <span class="dashboard-metric-link">Manage staff records</span>
                </a>

                <a class="dashboard-metric dashboard-metric-purple" href="ClassManagement.aspx">
                    <span class="dashboard-metric-icon" aria-hidden="true">CL</span>
                    <span class="dashboard-metric-value"><asp:Label ID="lblTotalClasses" runat="server" Text="-" /></span>
                    <span class="dashboard-metric-label">Active Classes</span>
                    <span class="dashboard-metric-link">Manage classes</span>
                </a>
            </div>
        </section>

        <section class="dashboard-snapshot" aria-labelledby="snapshot-heading">
            <div class="dashboard-section-heading dashboard-section-heading-compact">
                <div>
                    <span class="dashboard-section-kicker">Daily operating pulse</span>
                    <h2 id="snapshot-heading">Today at a Glance</h2>
                </div>
            </div>

            <div class="dashboard-snapshot-grid">
                <div class="dashboard-snapshot-card">
                    <span class="dashboard-snapshot-symbol" aria-hidden="true">+</span>
                    <div><strong><asp:Label ID="lblTodayAdmissions" runat="server" Text="-" /></strong><span>Admissions today</span></div>
                </div>
                <div class="dashboard-snapshot-card">
                    <span class="dashboard-snapshot-symbol" aria-hidden="true">AT</span>
                    <div><strong><asp:Label ID="lblStaffPresent" runat="server" Text="-" /></strong><span>Staff marked present</span></div>
                </div>
                <div class="dashboard-snapshot-card">
                    <span class="dashboard-snapshot-symbol" aria-hidden="true">EX</span>
                    <div><strong><asp:Label ID="lblUpcomingExams" runat="server" Text="-" /></strong><span>Current / upcoming exams</span></div>
                </div>
                <div class="dashboard-snapshot-card">
                    <span class="dashboard-snapshot-symbol" aria-hidden="true">VP</span>
                    <div><strong><asp:Label ID="lblVacantPosts" runat="server" Text="-" /></strong><span>Total vacant posts</span></div>
                </div>
            </div>
        </section>

        <section class="dashboard-fund-monitor" aria-labelledby="dashboard-fund-heading">
            <div class="dashboard-section-heading">
                <div>
                    <span class="dashboard-section-kicker">Funds and accounts</span>
                    <h2 id="dashboard-fund-heading">NSB and FTF Financial Position</h2>
                </div>
                <span class="dashboard-section-note">Live balances from the school accounting ledgers</span>
            </div>

            <asp:Panel ID="pnlFundSummaryLogin" runat="server" Visible="false" CssClass="dashboard-fee-login">
                <div><strong>Administrator login required</strong><p>Sign in to view protected school fund balances and collection performance.</p></div>
                <a href="Login.aspx?ReturnUrl=%2fAdminDashboard.aspx" class="dashboard-fee-login-button">Login to view funds</a>
            </asp:Panel>

            <asp:Panel ID="pnlFundSummaryAdmin" runat="server" Visible="false" CssClass="dashboard-fund-card">
                <asp:Panel ID="pnlFundSummaryUnavailable" runat="server" Visible="false" CssClass="dashboard-reminder-message reminder-warning">
                    <asp:Label ID="lblFundSummaryUnavailable" runat="server" />
                </asp:Panel>
                <div class="dashboard-fund-grid">
                    <a href="NSBFundManagement.aspx" class="dashboard-fund-metric fund-nsb">
                        <span>NSB Current Balance</span><strong><asp:Label ID="lblDashboardNsbBalance" runat="server" Text="Rs. 0.00" /></strong><small>Receipts less utilization</small>
                    </a>
                    <a href="FTFFundManagement.aspx" class="dashboard-fund-metric fund-wallet">
                        <span>FTF Cash in Hand</span><strong><asp:Label ID="lblDashboardFtfWallet" runat="server" Text="Rs. 0.00" /></strong><small>Collected but not deposited</small>
                    </a>
                    <a href="FTFFundManagement.aspx" class="dashboard-fund-metric fund-ftf">
                        <span>FTF Account Balance</span><strong><asp:Label ID="lblDashboardFtfBalance" runat="server" Text="Rs. 0.00" /></strong><small>Deposits less utilization</small>
                    </a>
                    <a href="FTFFundManagement.aspx" class="dashboard-fund-metric fund-performance">
                        <span>FTF Collected This Month</span><strong><asp:Label ID="lblDashboardFtfMonth" runat="server" Text="Rs. 0.00" /></strong><small><asp:Label ID="lblDashboardFtfTarget" runat="server" Text="0.00% of target" /></small>
                    </a>
                    <a href="BudgetReports.aspx" class="dashboard-fund-metric fund-budget">
                        <span>Latest Annual Budget</span><strong><asp:Label ID="lblDashboardBudgetEstimate" runat="server" Text="Not created" /></strong><small><asp:Label ID="lblDashboardBudgetName" runat="server" Text="Open the annual budget workspace" /></small>
                    </a>
                    <a href="MonthlyExpenditureReports.aspx" class="dashboard-fund-metric fund-expenditure">
                        <span>Latest Monthly Expenditure</span><strong><asp:Label ID="lblDashboardMonthlyExpenditure" runat="server" Text="Not created" /></strong><small><asp:Label ID="lblDashboardMonthlyExpenditureName" runat="server" Text="Open the monthly statement workspace" /></small>
                    </a>
                </div>
                <div class="dashboard-fund-actions"><a href="NSBFundManagement.aspx">Manage NSB and print statement</a><a href="FTFFundManagement.aspx">Reconcile FTF collections and account</a><a href="AnnualBudget.aspx">Develop annual budget</a><a href="MonthlyExpenditure.aspx">Prepare monthly expenditure</a></div>
            </asp:Panel>
        </section>

        <section class="dashboard-student-attendance-monitor" aria-labelledby="student-attendance-monitor-heading">
            <div class="dashboard-section-heading">
                <div>
                    <span class="dashboard-section-kicker">Student attendance</span>
                    <h2 id="student-attendance-monitor-heading">Class Attendance and Intervention</h2>
                </div>
                <a class="dashboard-text-link" href="StudentAttendanceAnalytics.aspx">Open full analysis</a>
            </div>

            <asp:Panel ID="pnlStudentAttendanceLogin" runat="server" Visible="false" CssClass="dashboard-fee-login">
                <div>
                    <strong>Administrator login required</strong>
                    <p>Sign in to view class attendance coverage and students who require attendance intervention.</p>
                </div>
                <a href="Login.aspx?ReturnUrl=%2fAdminDashboard.aspx" class="dashboard-fee-login-button">Login to view attendance</a>
            </asp:Panel>

            <asp:Panel ID="pnlStudentAttendanceAdmin" runat="server" Visible="false" CssClass="dashboard-attendance-monitor-card">
                <asp:Panel ID="pnlStudentAttendanceUnavailable" runat="server" Visible="false" CssClass="dashboard-reminder-message reminder-warning">
                    <asp:Label ID="lblStudentAttendanceUnavailable" runat="server" />
                </asp:Panel>

                <div class="dashboard-attendance-monitor-summary">
                    <div><span>Present Today</span><strong><asp:Label ID="lblStudentPresentToday" runat="server" Text="0 / 0" /></strong><small>Present / attendance marked</small></div>
                    <div><span>Short Attendance</span><strong><asp:Label ID="lblShortAttendanceCount" runat="server" Text="0" /></strong><small>Below 75% in the last 30 days</small></div>
                    <div class="dashboard-attendance-monitor-actions">
                        <a href="StudentAttendance.aspx">Mark today's attendance</a>
                        <a href="StudentAttendanceReport.aspx">Print attendance sheet</a>
                    </div>
                </div>

                <div class="dashboard-attendance-monitor-grid">
                    <div class="dashboard-attendance-class-list">
                        <h3>Class-wise Last 30 Days</h3>
                        <div class="dashboard-table-wrap">
                            <table class="dashboard-table">
                                <thead><tr><th>Class</th><th>Students</th><th>Today</th><th>30-day Rate</th></tr></thead>
                                <tbody>
                                    <asp:Repeater ID="rptDashboardClassAttendance" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td><strong><%#: Eval("ClassName") %></strong></td>
                                                <td><%#: Eval("StudentCount") %></td>
                                                <td><%#: Eval("PresentToday") %> / <%#: Eval("MarkedToday") %></td>
                                                <td><span class="dashboard-attendance-rate"><%#: Eval("AttendancePercentage", "{0:0.0}") %>%</span></td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                            <asp:Panel ID="pnlNoDashboardClassAttendance" runat="server" Visible="false" CssClass="dashboard-empty-state">
                                No class attendance records are available yet.
                            </asp:Panel>
                        </div>
                    </div>

                    <div class="dashboard-short-attendance-list">
                        <h3>Students Requiring Follow-up</h3>
                        <asp:Repeater ID="rptDashboardShortAttendance" runat="server">
                            <ItemTemplate>
                                <div class="dashboard-short-attendance-row">
                                    <span><strong><%#: Eval("StudentName") %></strong><small><%#: Eval("ClassName") %>, Roll <%#: Eval("StudentRollNo") %></small></span>
                                    <span><b><%#: Eval("AttendancePercentage", "{0:0.0}") %>%</b><small><%#: Eval("AbsentDays") %> absent</small></span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlNoDashboardShortAttendance" runat="server" Visible="false" CssClass="dashboard-empty-state">
                            No students are below the 75% attendance target in the last 30 recorded days.
                        </asp:Panel>
                    </div>
                </div>
            </asp:Panel>
        </section>

        <section class="dashboard-result-monitor" aria-labelledby="dashboard-result-monitor-heading">
            <div class="dashboard-section-heading">
                <div>
                    <span class="dashboard-section-kicker">Examination performance</span>
                    <h2 id="dashboard-result-monitor-heading">Latest Result Summary and Class Toppers</h2>
                </div>
                <a class="dashboard-text-link" href="StudentResultReports.aspx">Open result reports</a>
            </div>

            <asp:Panel ID="pnlResultSummaryLogin" runat="server" Visible="false" CssClass="dashboard-fee-login">
                <div>
                    <strong>Administrator login required</strong>
                    <p>Sign in to view examination appearance, pass percentage and class-wise top students.</p>
                </div>
                <a href="Login.aspx?ReturnUrl=%2fAdminDashboard.aspx" class="dashboard-fee-login-button">Login to view results</a>
            </asp:Panel>

            <asp:Panel ID="pnlResultSummaryAdmin" runat="server" Visible="false" CssClass="dashboard-result-card">
                <asp:Panel ID="pnlResultSummaryUnavailable" runat="server" Visible="false"
                    CssClass="dashboard-reminder-message reminder-warning">
                    <asp:Label ID="lblResultSummaryUnavailable" runat="server" />
                </asp:Panel>

                <div class="dashboard-result-heading">
                    <div>
                        <span>Latest examination with entered results</span>
                        <strong><asp:Label ID="lblDashboardResultExam" runat="server" Text="No examination results" /></strong>
                        <small><asp:Label ID="lblDashboardResultDate" runat="server" /></small>
                    </div>
                    <a href="StudentResultEntry.aspx">Enter or update results</a>
                </div>

                <div class="dashboard-result-summary-grid">
                    <div><span>Students Appeared</span><strong><asp:Label ID="lblExamAppearedStudents" runat="server" Text="0" /></strong><small>Students with recorded marks</small></div>
                    <div><span>Pass Percentage</span><strong><asp:Label ID="lblExamPassPercentage" runat="server" Text="0.00%" /></strong><small>Aggregate score of 40% or higher</small></div>
                    <div><span>Classes Reported</span><strong><asp:Label ID="lblExamClassesReported" runat="server" Text="0" /></strong><small>Classes with result entries</small></div>
                </div>

                <div class="dashboard-topper-heading">
                    <div><span>Academic distinction</span><h3>Top Three Students from Every Class</h3></div>
                    <small>Ranking is calculated from the selected examination's entered marks.</small>
                </div>

                <div class="dashboard-topper-grid">
                    <asp:Repeater ID="rptClassTopStudents" runat="server">
                        <ItemTemplate>
                            <article class="dashboard-topper-card">
                                <div class="dashboard-topper-rank">#<%#: Eval("ClassRank") %></div>
                                <img src='<%#: Eval("StudentPhotoUrl") %>' alt="Student photograph" />
                                <div class="dashboard-topper-copy">
                                    <span><%#: Eval("ClassName") %> | Roll <%#: Eval("StudentRollNo") %></span>
                                    <strong><%#: Eval("StudentName") %></strong>
                                    <small>Father: <%#: Eval("FatherName") %></small>
                                </div>
                                <div class="dashboard-topper-score">
                                    <strong><%#: Eval("Percentage", "{0:0.00}") %>%</strong>
                                    <small><%#: Eval("ObtainedMarks", "{0:0.##}") %> / <%#: Eval("TotalMarks", "{0:0.##}") %></small>
                                </div>
                            </article>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <asp:Panel ID="pnlNoClassTopStudents" runat="server" Visible="false" CssClass="dashboard-empty-state">
                    No student result entries are available for class-wise ranking yet.
                </asp:Panel>
            </asp:Panel>
        </section>

        <section class="dashboard-fee-monitor" aria-labelledby="fee-monitor-heading">
            <div class="dashboard-section-heading">
                <div>
                    <span class="dashboard-section-kicker">Fee compliance</span>
                    <h2 id="fee-monitor-heading">Outstanding Student Fees</h2>
                </div>
                <span class="dashboard-section-note">Protected parent-contact and reminder workspace</span>
            </div>

            <asp:Panel ID="pnlFeeDefaultersLogin" runat="server" CssClass="dashboard-fee-login" Visible="false">
                <div>
                    <strong>Administrator login required</strong>
                    <p>Sign in to view student fee defaulters and generate parent reminders. Personal student data is not shown to guests.</p>
                </div>
                <a href="Login.aspx?ReturnUrl=%2fAdminDashboard.aspx" class="dashboard-fee-login-button">Login to manage fees</a>
            </asp:Panel>

            <asp:Panel ID="pnlFeeDefaultersAdmin" runat="server" Visible="false" CssClass="dashboard-fee-card">
                <div class="dashboard-fee-toolbar">
                    <div class="dashboard-fee-period">
                        <label for="<%= ddlDefaulterMonth.ClientID %>">Fee month</label>
                        <asp:DropDownList ID="ddlDefaulterMonth" runat="server" CssClass="dashboard-fee-select" />
                        <label for="<%= ddlDefaulterYear.ClientID %>">Year</label>
                        <asp:DropDownList ID="ddlDefaulterYear" runat="server" CssClass="dashboard-fee-select dashboard-fee-year" />
                        <asp:Button ID="btnRefreshDefaulters" runat="server" Text="Load Status"
                            CssClass="dashboard-fee-button dashboard-fee-button-secondary"
                            CausesValidation="false" OnClick="btnRefreshDefaulters_Click" />
                    </div>
                    <div class="dashboard-fee-summary">
                        <span>Unpaid students</span>
                        <strong><asp:Label ID="lblFeeDefaulterCount" runat="server" Text="0" /></strong>
                        <asp:Button ID="btnSendAllReminders" runat="server" Text="Send All Reminders"
                            CssClass="dashboard-fee-button dashboard-fee-button-primary"
                            CausesValidation="false" OnClick="btnSendAllReminders_Click"
                            OnClientClick="return confirm('Generate and send or queue reminders for every unpaid student in this period?');" />
                    </div>
                </div>

                <asp:Panel ID="pnlFeeReminderMessage" runat="server" Visible="false" CssClass="dashboard-reminder-message" role="status">
                    <asp:Label ID="lblFeeReminderMessage" runat="server" />
                </asp:Panel>

                <div class="dashboard-table-wrap dashboard-fee-table-wrap">
                    <table class="dashboard-table dashboard-fee-table">
                        <thead>
                            <tr>
                                <th>Student</th>
                                <th>Registration</th>
                                <th>Class / Roll</th>
                                <th>Parent Contact</th>
                                <th>Reminder</th>
                                <th>Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptFeeDefaulters" runat="server" OnItemCommand="rptFeeDefaulters_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td><strong><%#: Eval("StudentName") %></strong><small>Father: <%#: Eval("FatherName") %></small></td>
                                        <td><%#: Eval("Regno") %></td>
                                        <td><span class="dashboard-table-badge"><%#: Eval("ClassName") %></span><small>Roll <%#: Eval("StudentRollNo") %></small></td>
                                        <td><%#: Eval("ContactNo") %></td>
                                        <td><span class='<%# "dashboard-reminder-status status-" + Eval("ReminderStatus").ToString().ToLowerInvariant().Replace(" ", "-") %>'><%#: Eval("ReminderStatus") %></span></td>
                                        <td>
                                            <asp:LinkButton ID="btnSendReminder" runat="server" Text="Send Reminder"
                                                CssClass="dashboard-row-action" CausesValidation="false"
                                                CommandName="SendReminder" CommandArgument='<%# Eval("StudentID") %>' />
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                    <asp:Panel ID="pnlNoFeeDefaulters" runat="server" CssClass="dashboard-empty-state dashboard-fee-clear" Visible="false">
                        All configured students have paid for the selected fee period.
                    </asp:Panel>
                </div>
                <p class="dashboard-panel-footnote">Reminder delivery uses the parent contact number saved in the Students table. Without an SMS gateway configuration, reminders are safely retained in the outbox as Pending.</p>
            </asp:Panel>
        </section>

        <section class="dashboard-main-grid">
            <article class="dashboard-panel dashboard-panel-wide" aria-labelledby="recent-students-heading">
                <div class="dashboard-panel-header">
                    <div>
                        <span class="dashboard-section-kicker">Latest records</span>
                        <h2 id="recent-students-heading">Recent Admissions</h2>
                    </div>
                    <a class="dashboard-text-link" href="StudentRegistrationForm.aspx">Open admissions</a>
                </div>
                <div class="dashboard-table-wrap">
                    <table class="dashboard-table">
                        <thead>
                            <tr>
                                <th>Student</th>
                                <th>Registration No.</th>
                                <th>Class</th>
                                <th>Roll No.</th>
                                <th>Admission Date</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptRecentStudents" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td><strong><%#: Eval("Name") %></strong></td>
                                        <td><%#: Eval("RegNo") %></td>
                                        <td><span class="dashboard-table-badge"><%#: Eval("ClassName") %></span></td>
                                        <td><%#: Eval("StudentRollNo") %></td>
                                        <td><%#: Eval("DateofAdmission", "{0:dd MMM yyyy}") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                    <asp:Panel ID="pnlNoRecentStudents" runat="server" CssClass="dashboard-empty-state" Visible="false">
                        No student admission records are available yet.
                    </asp:Panel>
                </div>
            </article>

            <aside class="dashboard-panel" aria-labelledby="attendance-heading">
                <div class="dashboard-panel-header">
                    <div>
                        <span class="dashboard-section-kicker">Attendance</span>
                        <h2 id="attendance-heading">Staff Today</h2>
                    </div>
                </div>
                <div class="dashboard-attendance-list">
                    <a href="TeachingStaffAttendance.aspx" class="dashboard-attendance-item">
                        <span class="dashboard-attendance-mark">T</span>
                        <span><strong>Teaching Staff</strong><small>Present / total staff</small></span>
                        <b><asp:Label ID="lblTeachingAttendance" runat="server" Text="-" /></b>
                    </a>
                    <a href="Non_TeachingStaffAttendance.aspx" class="dashboard-attendance-item">
                        <span class="dashboard-attendance-mark">N</span>
                        <span><strong>Non-Teaching Staff</strong><small>Present / total staff</small></span>
                        <b><asp:Label ID="lblNonTeachingAttendance" runat="server" Text="-" /></b>
                    </a>
                </div>
                <p class="dashboard-panel-footnote">Attendance figures update after today's records are saved.</p>
            </aside>
        </section>

        <section class="dashboard-lower-grid">
            <article class="dashboard-panel" aria-labelledby="quick-links-heading">
                <div class="dashboard-panel-header">
                    <div>
                        <span class="dashboard-section-kicker">Shortcuts</span>
                        <h2 id="quick-links-heading">Quick Navigation</h2>
                    </div>
                </div>
                <div class="dashboard-quick-grid">
                    <a href="StudentRegistrationForm.aspx"><span>01</span><strong>Register Student</strong><small>Create a new admission</small></a>
                    <a href="TeacherRegForm.aspx"><span>02</span><strong>Register Teacher</strong><small>Add teaching staff</small></a>
                    <a href="TeacherClassesTimeTable.aspx"><span>03</span><strong>Timetable</strong><small>Assign periods and classes</small></a>
                    <a href="SchoolFeeManagement.aspx"><span>04</span><strong>School Fee</strong><small>Manage fee structure</small></a>
                    <a href="StudentResultEntry.aspx"><span>05</span><strong>Enter Results</strong><small>Build student result cards</small></a>
                    <a href="FeeCollection.aspx"><span>06</span><strong>Collect Fee</strong><small>Create a student voucher</small></a>
                    <a href="StudentResultReports.aspx"><span>07</span><strong>Print Results</strong><small>Individual or class PDF cards</small></a>
                    <a href="NSBFundManagement.aspx"><span>08</span><strong>NSB Accounts</strong><small>Receipts, utilization, statement</small></a>
                    <a href="FTFFundManagement.aspx"><span>09</span><strong>FTF Accounts</strong><small>Wallet, bank, and collection target</small></a>
                </div>
            </article>

            <article class="dashboard-panel" aria-labelledby="upcoming-exams-heading">
                <div class="dashboard-panel-header">
                    <div>
                        <span class="dashboard-section-kicker">Academic calendar</span>
                        <h2 id="upcoming-exams-heading">Upcoming Examinations</h2>
                    </div>
                    <a class="dashboard-text-link" href="UpcommingExamDetails.aspx">Manage</a>
                </div>
                <div class="dashboard-exam-list">
                    <asp:Repeater ID="rptUpcomingExams" runat="server">
                        <ItemTemplate>
                            <div class="dashboard-exam-item">
                                <span class="dashboard-exam-date"><b><%#: Eval("StartDate", "{0:dd}") %></b><small><%#: Eval("StartDate", "{0:MMM}") %></small></span>
                                <span><strong><%#: Eval("ExamName") %></strong><small><%#: Eval("StartDate", "{0:dd MMM yyyy}") %> - <%#: Eval("EndDate", "{0:dd MMM yyyy}") %></small></span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoUpcomingExams" runat="server" CssClass="dashboard-empty-state" Visible="false">
                        No current or upcoming examinations are scheduled.
                    </asp:Panel>
                </div>
            </article>
        </section>
    </div>
</asp:Content>
