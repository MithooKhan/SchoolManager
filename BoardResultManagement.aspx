<%@ Page Title="Board Result Performance" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="BoardResultManagement.aspx.cs" Inherits="DigitalSchoolManager.BoardResultManagement" %>

<asp:Content ID="BoardResultHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .board-results-page{--br-deep:#063f2a;--br-green:#087347;--br-light:#eaf7ef;--br-gold:#d5a329;--br-border:#b9d7c6;--br-ink:#163b2b;max-width:1480px;margin:0 auto;padding:30px 28px 56px;color:var(--br-ink)}
        .br-hero{position:relative;overflow:hidden;display:flex;justify-content:space-between;gap:28px;padding:30px 34px;border-radius:22px;color:#fff;background:linear-gradient(125deg,#043a27,#076b42 68%,#0a8752);box-shadow:0 18px 38px rgba(3,58,38,.18)}
        .br-hero:after{content:"";position:absolute;right:-65px;bottom:-145px;width:330px;height:330px;border:48px solid rgba(255,255,255,.07);border-radius:50%}
        .br-hero>*{position:relative;z-index:1}.br-kicker{display:block;margin-bottom:7px;color:#f5cf68;font-size:.74rem;font-weight:900;letter-spacing:.13em;text-transform:uppercase}
        .br-hero h1{margin:0 0 9px;color:#fff;font-size:clamp(1.7rem,3vw,2.45rem);line-height:1.08}.br-hero p{max-width:830px;margin:0;color:#e0f3e8;line-height:1.55}
        .br-hero-note{flex:0 0 285px;align-self:center;padding:16px 18px;border:1px solid rgba(255,255,255,.25);border-radius:15px;background:rgba(255,255,255,.1)}
        .br-hero-note strong,.br-hero-note span{display:block}.br-hero-note span{margin-top:5px;color:#d7ede1;font-size:.78rem;line-height:1.45}
        .br-message{margin:18px 0 0;padding:14px 17px;border:1px solid;border-radius:12px;font-weight:700;line-height:1.45}.br-message.success{border-color:#7bcda0;color:#075a34;background:#e8f8ef}.br-message.error{border-color:#eca5a5;color:#842020;background:#fff0f0}
        .br-stats{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:14px;margin:19px 0}.br-stat{padding:18px 20px;border:1px solid var(--br-border);border-radius:15px;background:#fff;box-shadow:0 8px 21px rgba(5,71,45,.07)}
        .br-stat span,.br-stat strong,.br-stat small{display:block}.br-stat span{color:#5c7468;font-size:.7rem;font-weight:900;letter-spacing:.08em;text-transform:uppercase}.br-stat strong{margin:5px 0 2px;color:var(--br-green);font-size:1.75rem}.br-stat small{color:#6b7d74;font-size:.74rem}
        .br-card{margin-top:19px;overflow:hidden;border:1px solid var(--br-border);border-radius:18px;background:#fff;box-shadow:0 10px 27px rgba(4,63,40,.08)}.br-card-head{display:flex;align-items:center;justify-content:space-between;gap:18px;padding:18px 22px;border-bottom:1px solid #cce1d4;background:linear-gradient(90deg,#edf8f1,#f9fcfa)}
        .br-card-head h2,.br-card-head h3{margin:3px 0 0;color:var(--br-deep);font-size:1.22rem}.br-card-head p{margin:4px 0 0;color:#64786e;font-size:.78rem}.br-section-label{color:#9b7110;font-size:.69rem;font-weight:900;letter-spacing:.11em;text-transform:uppercase}
        .br-form{padding:21px}.br-grid{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:15px}.br-grid.three{grid-template-columns:repeat(3,minmax(0,1fr))}.br-field{display:grid;align-content:start;gap:6px}.br-field.wide{grid-column:span 2}.br-field.full{grid-column:1/-1}
        .br-field label,.br-label{color:#224737;font-size:.76rem;font-weight:900;letter-spacing:.025em}.br-control{width:100%;min-height:43px;box-sizing:border-box;padding:9px 11px;border:1px solid #9fc5af;border-radius:9px;color:#153c2b;background:#fff;font:inherit;outline:none}.br-control:focus{border-color:#07834e;box-shadow:0 0 0 3px rgba(8,131,78,.13)}textarea.br-control{min-height:84px;resize:vertical}
        .br-field small{color:#6a7d73;font-size:.7rem;line-height:1.4}.br-subsection{grid-column:1/-1;margin-top:4px;padding:12px 14px;border-left:4px solid var(--br-gold);border-radius:8px;background:#fff9e8}.br-subsection strong{display:block;color:#674a08}.br-subsection span{color:#796b47;font-size:.76rem}
        .br-live{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:10px;padding:13px;border:1px solid #b7d9c5;border-radius:12px;background:#edf8f1}.br-live div{padding:9px 11px;border-radius:9px;background:#fff}.br-live span,.br-live strong{display:block}.br-live span{color:#61766b;font-size:.66rem;font-weight:800;text-transform:uppercase}.br-live strong{margin-top:3px;color:#075d38;font-size:1.08rem}
        .br-actions{display:flex;flex-wrap:wrap;gap:10px;margin-top:18px}.br-button,.br-link-button{display:inline-flex;align-items:center;justify-content:center;min-height:42px;padding:9px 16px;border:1px solid transparent;border-radius:9px;font-weight:900;text-decoration:none;cursor:pointer}.br-primary{color:#fff;background:#087347;box-shadow:0 6px 13px rgba(8,115,71,.18)}.br-secondary{color:#123d2b;border-color:#93bca5;background:#edf7f1}.br-gold{color:#2c240c;background:#ecc85e}.br-button:hover,.br-link-button:hover{filter:brightness(.97)}
        .br-workspace-ribbon{display:flex;justify-content:space-between;align-items:center;gap:18px;padding:19px 22px;color:#fff;background:linear-gradient(105deg,#053f2a,#087347)}.br-workspace-ribbon strong,.br-workspace-ribbon span{display:block}.br-workspace-ribbon span{margin-top:4px;color:#d6ede1;font-size:.78rem}.br-workspace-links{display:flex;flex-wrap:wrap;gap:8px}.br-workspace-links a{padding:9px 12px;border:1px solid rgba(255,255,255,.3);border-radius:8px;color:#fff;background:rgba(255,255,255,.1);font-size:.77rem;font-weight:900;text-decoration:none}
        .br-two-column{display:grid;grid-template-columns:1.35fr .65fr;gap:18px;padding:20px;background:#f5fbf7}.br-inner{overflow:hidden;border:1px solid var(--br-border);border-radius:14px;background:#fff}.br-inner-head{padding:14px 17px;border-bottom:1px solid #d5e7dc;background:#ecf7f0}.br-inner-head h3{margin:2px 0;color:#084a30;font-size:1.02rem}.br-inner-head p{margin:3px 0 0;color:#667b70;font-size:.72rem}.br-inner .br-form{padding:17px}.br-inner .br-grid{grid-template-columns:repeat(3,minmax(0,1fr));gap:12px}.br-inner.position .br-grid{grid-template-columns:repeat(2,minmax(0,1fr))}
        .br-table-wrap{overflow:auto}.br-table{width:100%;border-collapse:collapse;min-width:980px}.br-table th{padding:11px 10px;color:#fff;background:#0a6741;font-size:.7rem;letter-spacing:.035em;text-align:left;text-transform:uppercase}.br-table td{padding:11px 10px;border-bottom:1px solid #d8e7de;color:#203f31;font-size:.77rem;vertical-align:middle}.br-table tr:nth-child(even) td{background:#f1f9f4}.br-table .number{text-align:right;font-variant-numeric:tabular-nums}.br-row-action{display:inline-flex;padding:6px 10px;border:1px solid #8fc3a6;border-radius:7px;color:#064c2f;background:#daf2e4;font-size:.71rem;font-weight:900;text-decoration:none}
        .br-status{display:inline-flex;padding:5px 8px;border-radius:999px;font-size:.68rem;font-weight:900;white-space:nowrap}.br-status.above{color:#075d35;background:#dff6e9}.br-status.below{color:#8a2b22;background:#ffebe8}.br-status.equal{color:#73560b;background:#fff3c9}
        .br-history-tools{display:grid;grid-template-columns:1fr 150px 1.3fr auto;gap:10px;align-items:end;padding:17px 20px;border-bottom:1px solid #d6e6dd}.br-empty{padding:25px;text-align:center;color:#687b71}
        @media(max-width:1050px){.br-grid{grid-template-columns:repeat(2,minmax(0,1fr))}.br-two-column{grid-template-columns:1fr}.br-stats{grid-template-columns:repeat(2,minmax(0,1fr))}.br-history-tools{grid-template-columns:1fr 1fr}.br-hero-note{flex-basis:230px}}
        @media(max-width:680px){.board-results-page{padding:18px 12px 40px}.br-hero{display:block;padding:24px 20px}.br-hero-note{margin-top:18px}.br-grid,.br-grid.three,.br-inner .br-grid,.br-inner.position .br-grid,.br-stats,.br-history-tools{grid-template-columns:1fr}.br-field.wide{grid-column:auto}.br-card-head,.br-workspace-ribbon{align-items:flex-start;flex-direction:column}.br-two-column{padding:12px}.br-form{padding:16px}}
    </style>
</asp:Content>

<asp:Content ID="BoardResultContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="board-results-page">
        <section class="br-hero" aria-labelledby="boardResultTitle">
            <div>
                <span class="br-kicker">Board examination performance</span>
                <h1 id="boardResultTitle">Teacher and Subject-Wise Results</h1>
                <p>Maintain permanent annual examination records for Classes 9th to 12th, compare school performance with the board, and prepare the official head-wise and teacher-wise statements.</p>
            </div>
            <aside class="br-hero-note">
                <strong>Official result record</strong>
                <span>Classes 9-12, subject teachers, academic groups, position holders and print-ready reports.</span>
            </aside>
        </section>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="br-message" role="status">
            <asp:Label ID="lblMessage" runat="server" />
        </asp:Panel>

        <section class="br-stats" aria-label="Board result record summary">
            <article class="br-stat"><span>Result files</span><strong><asp:Label ID="lblSessionCount" runat="server" Text="0" /></strong><small>Saved annual statements</small></article>
            <article class="br-stat"><span>Subject records</span><strong><asp:Label ID="lblSubjectRecordCount" runat="server" Text="0" /></strong><small>Teacher-subject entries</small></article>
            <article class="br-stat"><span>Above board</span><strong><asp:Label ID="lblAboveBoardCount" runat="server" Text="0" /></strong><small>Subject results above board rate</small></article>
            <article class="br-stat"><span>Latest year</span><strong><asp:Label ID="lblLatestYear" runat="server" Text="-" /></strong><small>Most recent result file</small></article>
        </section>

        <section class="br-card" aria-labelledby="sessionHeading">
            <header class="br-card-head">
                <div><span class="br-section-label">Step 1 - Result file</span><h2 id="sessionHeading"><asp:Label ID="lblSessionEditorTitle" runat="server" Text="Create annual board result file" /></h2><p>Enter the head-wise figures once. The school and failed percentages are calculated automatically.</p></div>
                <asp:Button ID="btnNewSession" runat="server" Text="New Result File" CausesValidation="false" CssClass="br-button br-secondary" OnClick="btnNewSession_Click" />
            </header>
            <div class="br-form">
                <asp:HiddenField ID="hfSessionID" runat="server" />
                <div class="br-grid">
                    <div class="br-field"><label for="<%= ddlClass.ClientID %>">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="br-control" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_SelectedIndexChanged" /><small><asp:Label ID="lblClassGuidance" runat="server" Text="Only Classes 9th to 12th are listed." /></small></div>
                    <div class="br-field"><label for="<%= txtStudyGroup.ClientID %>">Field / study group</label><asp:TextBox ID="txtStudyGroup" runat="server" CssClass="br-control" MaxLength="100" list="boardStudyGroups" placeholder="General, Humanities, Pre-Medical..." /><datalist id="boardStudyGroups"><option value="General"></option><option value="Humanities"></option><option value="Pre-Medical"></option><option value="Pre-Engineering"></option><option value="Computer Science"></option><option value="Commerce"></option></datalist><small>Required for 11th and 12th; use General where no stream applies.</small></div>
                    <div class="br-field"><label for="<%= txtExamTitle.ClientID %>">Examination title</label><asp:TextBox ID="txtExamTitle" runat="server" CssClass="br-control" MaxLength="160" placeholder="Annual Examination" /></div>
                    <div class="br-field"><label for="<%= txtExamYear.ClientID %>">Result year</label><asp:TextBox ID="txtExamYear" runat="server" CssClass="br-control" TextMode="Number" min="2000" max="2100" /></div>
                    <div class="br-field"><label for="<%= txtResultDate.ClientID %>">Result date</label><asp:TextBox ID="txtResultDate" runat="server" CssClass="br-control" TextMode="Date" /></div>
                    <div class="br-field"><label for="<%= txtBoardName.ClientID %>">Board name</label><asp:TextBox ID="txtBoardName" runat="server" CssClass="br-control" MaxLength="160" /></div>
                    <div class="br-field"><label for="<%= txtEmisCode.ClientID %>">EMIS code</label><asp:TextBox ID="txtEmisCode" runat="server" CssClass="br-control" MaxLength="30" /></div>
                    <div class="br-field"><label for="<%= txtBiseCode.ClientID %>">BISE code</label><asp:TextBox ID="txtBiseCode" runat="server" CssClass="br-control" MaxLength="30" /></div>

                    <div class="br-subsection"><strong>Responsible Head Teacher / Incharge</strong><span>The selected teacher's name, designation, scale and mobile number are permanently copied into this result file.</span></div>
                    <div class="br-field wide"><label for="<%= ddlHeadTeacher.ClientID %>">Head Teacher / Incharge</label><asp:DropDownList ID="ddlHeadTeacher" runat="server" CssClass="br-control" /></div>
                    <div class="br-field"><label for="<%= txtHeadPeriodFrom.ClientID %>">Responsible from</label><asp:TextBox ID="txtHeadPeriodFrom" runat="server" CssClass="br-control" TextMode="Date" /></div>
                    <div class="br-field"><label for="<%= txtHeadPeriodTo.ClientID %>">Responsible to</label><asp:TextBox ID="txtHeadPeriodTo" runat="server" CssClass="br-control" TextMode="Date" /></div>

                    <div class="br-subsection"><strong>Head-wise result figures</strong><span>Enter appeared and passed students. Failed students and school pass percentage are calculated from these values.</span></div>
                    <div class="br-field"><label for="<%= txtRegisteredCount.ClientID %>">Registered students</label><asp:TextBox ID="txtRegisteredCount" runat="server" CssClass="br-control br-session-number" TextMode="Number" min="0" Text="0" /></div>
                    <div class="br-field"><label for="<%= txtRegisteredYear.ClientID %>">Registration year</label><asp:TextBox ID="txtRegisteredYear" runat="server" CssClass="br-control" TextMode="Number" min="2000" max="2100" /></div>
                    <asp:Panel ID="pnlPreviousCohort" runat="server" CssClass="br-field"><label for="<%= txtPreviousAppearedCount.ClientID %>"><asp:Label ID="lblPreviousAppearedCaption" runat="server" Text="Previous class appeared" /></label><asp:TextBox ID="txtPreviousAppearedCount" runat="server" CssClass="br-control" TextMode="Number" min="0" /></asp:Panel>
                    <asp:Panel ID="pnlPreviousYear" runat="server" CssClass="br-field"><label for="<%= txtPreviousAppearedYear.ClientID %>">Previous result year</label><asp:TextBox ID="txtPreviousAppearedYear" runat="server" CssClass="br-control" TextMode="Number" min="2000" max="2100" /></asp:Panel>
                    <div class="br-field"><label for="<%= txtAppearedCount.ClientID %>">Students appeared</label><asp:TextBox ID="txtAppearedCount" runat="server" CssClass="br-control br-session-number" TextMode="Number" min="0" Text="0" /></div>
                    <div class="br-field"><label for="<%= txtPassedCount.ClientID %>">Students passed</label><asp:TextBox ID="txtPassedCount" runat="server" CssClass="br-control br-session-number" TextMode="Number" min="0" Text="0" /></div>
                    <div class="br-field"><label for="<%= txtBoardPassPercentage.ClientID %>">Board pass percentage</label><asp:TextBox ID="txtBoardPassPercentage" runat="server" CssClass="br-control" TextMode="Number" min="0" max="100" step="0.01" Text="0" /></div>
                    <div class="br-field"><label>Calculated result</label><div class="br-live"><div><span>Failed</span><strong id="brLiveFailed">0</strong></div><div><span>School %</span><strong id="brLiveSchool">0.00%</strong></div><div><span>Grade total</span><strong id="brLiveGrades">0</strong></div></div></div>

                    <div class="br-subsection"><strong>Grade-wise passed students</strong><span>The grade total cannot exceed the number of passed students.</span></div>
                    <div class="br-field"><label for="<%= txtGradeAPlus.ClientID %>">A+ grade</label><asp:TextBox ID="txtGradeAPlus" runat="server" CssClass="br-control br-grade-number" TextMode="Number" min="0" Text="0" /></div>
                    <div class="br-field"><label for="<%= txtGradeA.ClientID %>">A grade</label><asp:TextBox ID="txtGradeA" runat="server" CssClass="br-control br-grade-number" TextMode="Number" min="0" Text="0" /></div>
                    <div class="br-field"><label for="<%= txtGradeB.ClientID %>">B grade</label><asp:TextBox ID="txtGradeB" runat="server" CssClass="br-control br-grade-number" TextMode="Number" min="0" Text="0" /></div>
                    <div class="br-field"><label for="<%= txtGradeC.ClientID %>">C grade</label><asp:TextBox ID="txtGradeC" runat="server" CssClass="br-control br-grade-number" TextMode="Number" min="0" Text="0" /></div>
                    <div class="br-field"><label for="<%= txtGradeD.ClientID %>">D grade</label><asp:TextBox ID="txtGradeD" runat="server" CssClass="br-control br-grade-number" TextMode="Number" min="0" Text="0" /></div>
                    <div class="br-field"><label for="<%= txtGradeE.ClientID %>">E grade</label><asp:TextBox ID="txtGradeE" runat="server" CssClass="br-control br-grade-number" TextMode="Number" min="0" Text="0" /></div>
                    <div class="br-field wide"><label for="<%= txtSessionRemarks.ClientID %>">Remarks</label><asp:TextBox ID="txtSessionRemarks" runat="server" CssClass="br-control" TextMode="MultiLine" MaxLength="500" /></div>
                </div>
                <div class="br-actions">
                    <asp:Button ID="btnSaveSession" runat="server" Text="Save Result File" CssClass="br-button br-primary" OnClick="btnSaveSession_Click" />
                    <asp:Button ID="btnClearSession" runat="server" Text="Clear Form" CausesValidation="false" CssClass="br-button br-secondary" OnClick="btnClearSession_Click" />
                </div>
            </div>
        </section>

        <asp:Panel ID="pnlSessionWorkspace" runat="server" Visible="false" CssClass="br-card">
            <div class="br-workspace-ribbon">
                <div><strong><asp:Label ID="lblWorkspaceTitle" runat="server" /></strong><span><asp:Label ID="lblWorkspaceMeta" runat="server" /></span></div>
                <div class="br-workspace-links"><asp:HyperLink ID="lnkHeadReport" runat="server" Target="_blank" Text="Head-wise Report" /><asp:HyperLink ID="lnkTeacherReport" runat="server" Target="_blank" Text="Teacher-wise Report" /><asp:HyperLink ID="lnkCompleteReport" runat="server" Target="_blank" Text="Complete Print / PDF" /></div>
            </div>

            <div class="br-two-column">
                <section class="br-inner">
                    <header class="br-inner-head"><span class="br-section-label">Step 2</span><h3><asp:Label ID="lblSubjectEditorTitle" runat="server" Text="Add teacher and subject result" /></h3><p>Teacher details are loaded from the staff record and retained as a result-year snapshot.</p></header>
                    <div class="br-form">
                        <asp:HiddenField ID="hfSubjectResultID" runat="server" />
                        <div class="br-grid">
                            <div class="br-field wide"><label for="<%= ddlSubjectTeacher.ClientID %>">Teacher</label><asp:DropDownList ID="ddlSubjectTeacher" runat="server" CssClass="br-control" /></div>
                            <div class="br-field"><label for="<%= ddlSubject.ClientID %>">Subject</label><asp:DropDownList ID="ddlSubject" runat="server" CssClass="br-control" /></div>
                            <div class="br-field"><label for="<%= txtResponsibilityFrom.ClientID %>">Responsible from</label><asp:TextBox ID="txtResponsibilityFrom" runat="server" CssClass="br-control" TextMode="Date" /></div>
                            <div class="br-field"><label for="<%= txtResponsibilityTo.ClientID %>">Responsible to</label><asp:TextBox ID="txtResponsibilityTo" runat="server" CssClass="br-control" TextMode="Date" /></div>
                            <div class="br-field"><label for="<%= txtSubjectRegistered.ClientID %>">Registered</label><asp:TextBox ID="txtSubjectRegistered" runat="server" CssClass="br-control" TextMode="Number" min="0" Text="0" /></div>
                            <asp:Panel ID="pnlSubjectPrevious" runat="server" CssClass="br-field"><label for="<%= txtSubjectPreviousAppeared.ClientID %>">Previous class appeared</label><asp:TextBox ID="txtSubjectPreviousAppeared" runat="server" CssClass="br-control" TextMode="Number" min="0" /></asp:Panel>
                            <div class="br-field"><label for="<%= txtSubjectAppeared.ClientID %>">Appeared</label><asp:TextBox ID="txtSubjectAppeared" runat="server" CssClass="br-control br-subject-number" TextMode="Number" min="0" Text="0" /></div>
                            <div class="br-field"><label for="<%= txtSubjectPassed.ClientID %>">Passed</label><asp:TextBox ID="txtSubjectPassed" runat="server" CssClass="br-control br-subject-number" TextMode="Number" min="0" Text="0" /></div>
                            <div class="br-field"><label for="<%= txtSubjectBoardPercentage.ClientID %>">Board percentage</label><asp:TextBox ID="txtSubjectBoardPercentage" runat="server" CssClass="br-control br-subject-number" TextMode="Number" min="0" max="100" step="0.01" Text="0" /></div>
                            <div class="br-field full"><div class="br-live"><div><span>Failed</span><strong id="brSubjectFailed">0</strong></div><div><span>School %</span><strong id="brSubjectSchool">0.00%</strong></div><div><span>Comparison</span><strong id="brSubjectStatus">Equal</strong></div></div></div>
                            <div class="br-field full"><label for="<%= txtSubjectNotes.ClientID %>">Notes</label><asp:TextBox ID="txtSubjectNotes" runat="server" CssClass="br-control" TextMode="MultiLine" MaxLength="500" /></div>
                        </div>
                        <div class="br-actions"><asp:Button ID="btnSaveSubjectResult" runat="server" Text="Save Subject Result" CssClass="br-button br-primary" OnClick="btnSaveSubjectResult_Click" /><asp:Button ID="btnCancelSubjectEdit" runat="server" Text="Cancel Edit" Visible="false" CausesValidation="false" CssClass="br-button br-secondary" OnClick="btnCancelSubjectEdit_Click" /></div>
                    </div>
                </section>

                <section class="br-inner position">
                    <header class="br-inner-head"><span class="br-section-label">Step 3</span><h3>Position holders</h3><p>Save the first, second and third position for the official head-wise statement.</p></header>
                    <div class="br-form">
                        <div class="br-grid">
                            <div class="br-field full"><label for="<%= ddlPositionStudent.ClientID %>">Student</label><asp:DropDownList ID="ddlPositionStudent" runat="server" CssClass="br-control" /></div>
                            <div class="br-field"><label for="<%= ddlPositionNumber.ClientID %>">Position</label><asp:DropDownList ID="ddlPositionNumber" runat="server" CssClass="br-control"><asp:ListItem Value="1">1st</asp:ListItem><asp:ListItem Value="2">2nd</asp:ListItem><asp:ListItem Value="3">3rd</asp:ListItem></asp:DropDownList></div>
                            <div class="br-field"><label for="<%= txtPositionObtained.ClientID %>">Obtained marks</label><asp:TextBox ID="txtPositionObtained" runat="server" CssClass="br-control" TextMode="Number" min="0" step="0.01" /></div>
                            <div class="br-field"><label for="<%= txtPositionTotal.ClientID %>">Total marks</label><asp:TextBox ID="txtPositionTotal" runat="server" CssClass="br-control" TextMode="Number" min="1" step="0.01" /></div>
                            <div class="br-field full"><label for="<%= txtPositionNotes.ClientID %>">Notes</label><asp:TextBox ID="txtPositionNotes" runat="server" CssClass="br-control" MaxLength="300" /></div>
                        </div>
                        <div class="br-actions"><asp:Button ID="btnSavePosition" runat="server" Text="Save Position Holder" CssClass="br-button br-gold" OnClick="btnSavePosition_Click" /></div>
                    </div>
                </section>
            </div>

            <section class="br-inner" style="margin:0 20px 20px">
                <header class="br-inner-head"><h3>Teacher and subject-wise entries</h3><p>Failed students, school pass percentage and performance status are calculated by the system.</p></header>
                <div class="br-table-wrap"><asp:GridView ID="gvSubjectResults" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="br-table" EmptyDataText="No teacher-subject result has been entered for this file." OnRowCommand="gvSubjectResults_RowCommand"><Columns>
                    <asp:BoundField DataField="SubjectNameSnapshot" HeaderText="Subject" />
                    <asp:BoundField DataField="TeacherNameSnapshot" HeaderText="Teacher" />
                    <asp:BoundField DataField="DesignationSnapshot" HeaderText="Designation" />
                    <asp:BoundField DataField="AppearedCount" HeaderText="Appeared" ItemStyle-CssClass="number" />
                    <asp:BoundField DataField="PassedCount" HeaderText="Passed" ItemStyle-CssClass="number" />
                    <asp:BoundField DataField="FailedCount" HeaderText="Failed" ItemStyle-CssClass="number" />
                    <asp:BoundField DataField="SchoolPassPercentage" HeaderText="School %" DataFormatString="{0:N2}" ItemStyle-CssClass="number" />
                    <asp:BoundField DataField="BoardPassPercentage" HeaderText="Board %" DataFormatString="{0:N2}" ItemStyle-CssClass="number" />
                    <asp:TemplateField HeaderText="Status"><ItemTemplate><span class='br-status <%# Eval("PerformanceCss") %>'><%#: Eval("PerformanceStatus") %></span></ItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Action"><ItemTemplate><asp:LinkButton ID="btnEditSubjectResult" runat="server" Text="Edit" CssClass="br-row-action" CausesValidation="false" CommandName="EditResult" CommandArgument='<%# Eval("TeacherSubjectResultID") %>' /></ItemTemplate></asp:TemplateField>
                </Columns></asp:GridView></div>
            </section>

            <section class="br-inner" style="margin:0 20px 20px">
                <header class="br-inner-head"><h3>Saved position holders</h3></header>
                <div class="br-table-wrap"><asp:GridView ID="gvPositionHolders" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="br-table" EmptyDataText="No position holder has been saved." OnRowCommand="gvPositionHolders_RowCommand"><Columns>
                    <asp:BoundField DataField="PositionLabel" HeaderText="Position" />
                    <asp:BoundField DataField="StudentNameSnapshot" HeaderText="Student" />
                    <asp:BoundField DataField="FatherNameSnapshot" HeaderText="Father Name" />
                    <asp:BoundField DataField="ObtainedMarks" HeaderText="Obtained" DataFormatString="{0:N2}" ItemStyle-CssClass="number" />
                    <asp:BoundField DataField="TotalMarks" HeaderText="Total" DataFormatString="{0:N2}" ItemStyle-CssClass="number" />
                    <asp:BoundField DataField="Percentage" HeaderText="Percentage" DataFormatString="{0:N2}%" ItemStyle-CssClass="number" />
                    <asp:TemplateField HeaderText="Action"><ItemTemplate><asp:LinkButton ID="btnEditPosition" runat="server" Text="Edit" CssClass="br-row-action" CausesValidation="false" CommandName="EditPosition" CommandArgument='<%# Eval("PositionHolderID") %>' /></ItemTemplate></asp:TemplateField>
                </Columns></asp:GridView></div>
            </section>
        </asp:Panel>

        <section class="br-card" aria-labelledby="historyHeading">
            <header class="br-card-head"><div><span class="br-section-label">Permanent record</span><h2 id="historyHeading">Saved board result files</h2><p>Search by class, year, group, examination, teacher or subject and reopen any statement for editing or printing.</p></div></header>
            <div class="br-history-tools">
                <div class="br-field"><label for="<%= ddlHistoryClass.ClientID %>">Class</label><asp:DropDownList ID="ddlHistoryClass" runat="server" CssClass="br-control" /></div>
                <div class="br-field"><label for="<%= txtHistoryYear.ClientID %>">Year</label><asp:TextBox ID="txtHistoryYear" runat="server" CssClass="br-control" TextMode="Number" min="2000" max="2100" /></div>
                <div class="br-field"><label for="<%= txtHistorySearch.ClientID %>">Search</label><asp:TextBox ID="txtHistorySearch" runat="server" CssClass="br-control" MaxLength="100" placeholder="Exam, group, teacher or subject" /></div>
                <asp:Button ID="btnSearchHistory" runat="server" Text="Search Records" CssClass="br-button br-primary" CausesValidation="false" OnClick="btnSearchHistory_Click" />
            </div>
            <div class="br-table-wrap"><asp:GridView ID="gvSessions" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="br-table" EmptyDataText="No board result files match the selected filters." OnRowCommand="gvSessions_RowCommand"><Columns>
                <asp:BoundField DataField="ExamYear" HeaderText="Year" />
                <asp:BoundField DataField="ClassNameSnapshot" HeaderText="Class" />
                <asp:BoundField DataField="StudyGroup" HeaderText="Field / Group" />
                <asp:BoundField DataField="ExamTitle" HeaderText="Examination" />
                <asp:BoundField DataField="HeadNameSnapshot" HeaderText="Head / Incharge" />
                <asp:BoundField DataField="SubjectCount" HeaderText="Subjects" ItemStyle-CssClass="number" />
                <asp:BoundField DataField="AppearedCount" HeaderText="Appeared" ItemStyle-CssClass="number" />
                <asp:BoundField DataField="SchoolPassPercentage" HeaderText="School %" DataFormatString="{0:N2}" ItemStyle-CssClass="number" />
                <asp:TemplateField HeaderText="Actions"><ItemTemplate><asp:LinkButton ID="btnOpenSession" runat="server" Text="Open / Edit" CssClass="br-row-action" CausesValidation="false" CommandName="OpenSession" CommandArgument='<%# Eval("BoardResultSessionID") %>' /> <asp:HyperLink ID="lnkHistoryReport" runat="server" Text="Print" CssClass="br-row-action" Target="_blank" NavigateUrl='<%# GetBoardReportUrl(Eval("BoardResultSessionID")) %>' /></ItemTemplate></asp:TemplateField>
            </Columns></asp:GridView></div>
        </section>
    </div>

    <script type="text/javascript">
        (function () {
            function numberValue(id) { var el=document.getElementById(id); var value=el?parseFloat(el.value):0; return isNaN(value)?0:value; }
            function bind(selector, action) { var nodes=document.querySelectorAll(selector); for(var i=0;i<nodes.length;i++) nodes[i].addEventListener("input",action); }
            function sessionTotals(){
                var appeared=numberValue("<%= txtAppearedCount.ClientID %>"), passed=numberValue("<%= txtPassedCount.ClientID %>"), grades=0;
                var gradeNodes=document.querySelectorAll(".br-grade-number"); for(var i=0;i<gradeNodes.length;i++){var n=parseInt(gradeNodes[i].value,10);grades+=isNaN(n)?0:n;}
                document.getElementById("brLiveFailed").textContent=Math.max(0,appeared-passed);
                document.getElementById("brLiveSchool").textContent=(appeared>0?(passed*100/appeared):0).toFixed(2)+"%";
                document.getElementById("brLiveGrades").textContent=grades;
            }
            function subjectTotals(){
                var appeared=numberValue("<%= txtSubjectAppeared.ClientID %>"), passed=numberValue("<%= txtSubjectPassed.ClientID %>"), board=numberValue("<%= txtSubjectBoardPercentage.ClientID %>");
                var school=appeared>0?(passed*100/appeared):0;
                document.getElementById("brSubjectFailed").textContent=Math.max(0,appeared-passed);
                document.getElementById("brSubjectSchool").textContent=school.toFixed(2)+"%";
                document.getElementById("brSubjectStatus").textContent=school>board?"Above Board":school<board?"Below Board":"Equal";
            }
            bind(".br-session-number,.br-grade-number",sessionTotals); bind(".br-subject-number",subjectTotals); sessionTotals(); subjectTotals();
        }());
    </script>
</asp:Content>
