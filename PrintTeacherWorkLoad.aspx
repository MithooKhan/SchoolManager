<%@ Page Title="Teacher Workload" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="PrintTeacherWorkLoad.aspx.cs" Inherits="DigitalSchoolManager.WebForm23" %>
<asp:Content ID="mainContent" ContentPlaceHolderID="MainContent" runat="server">


<asp:ScriptManager ID="sm1" runat="server"/>
<asp:HiddenField ID="hfEditID" runat="server" Value="0"/>

<div class="wl-page">

<!-- ══════════════════════════════════════════════
     HERO
═══════════════════════════════════════════════ -->


<!-- ══════════════════════════════════════════════
     NOTIFICATION
═══════════════════════════════════════════════ -->
<asp:UpdatePanel ID="upMsg" runat="server" UpdateMode="Conditional">
<ContentTemplate>
  <asp:Panel ID="pnlMsg" runat="server" Visible="false">
    <div class="wl-alert" id="alertBox" runat="server">
      <span class="wl-alert-ico"><asp:Literal ID="litIco" runat="server"/></span>
      <asp:Literal ID="litMsg" runat="server"/>
    </div>
  </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>


<!-- ══════════════════════════════════════════════
     SUMMARY CARDS
═══════════════════════════════════════════════ -->
<div class="summary-cards no-print">
  <div class="scard scard-a">
    <div class="scard-icon"></div>
    <div class="scard-value"><asp:Label ID="lblCardTeachers" runat="server">0</asp:Label></div>
    <div class="scard-label">Teachers Assigned</div>
    <div class="scard-sub">Active this term</div>
  </div>
  <div class="scard scard-b">
    <div class="scard-icon"></div>
    <div class="scard-value"><asp:Label ID="lblCardPeriods" runat="server">0</asp:Label></div>
    <div class="scard-label">Total Periods</div>
    <div class="scard-sub">Across all classes</div>
  </div>
  <div class="scard scard-c">
    <div class="scard-icon"></div>
    <div class="scard-value"><asp:Label ID="lblCardAvg" runat="server">0.0</asp:Label></div>
    <div class="scard-label">Avg Periods</div>
    <div class="scard-sub">Per teacher per day</div>
  </div>
  <div class="scard scard-d">
    <div class="scard-icon"></div>
    <div class="scard-value"><asp:Label ID="lblCardMax" runat="server">0</asp:Label></div>
    <div class="scard-label">Highest Load</div>
    <div class="scard-sub"><asp:Label ID="lblCardMaxName" runat="server">-</asp:Label></div>
  </div>
  <div class="scard scard-e">
    <div class="scard-icon"></div>
    <div class="scard-value"><asp:Label ID="lblCardOverloaded" runat="server">0</asp:Label></div>
    <div class="scard-label">Overloaded</div>
    <div class="scard-sub">&gt; avg by 50%</div>
  </div>
</div>

<!-- ══════════════════════════════════════════════
     FILTER BAR
═══════════════════════════════════════════════ -->
<div class="ctrl-bar no-print">
  <div class="ctrl-group">
    <label class="ctrl-label">Filter Teacher</label>
    <asp:DropDownList ID="ddlFltTeacher" runat="server" CssClass="ctrl-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed"/>
  </div>
  <div class="ctrl-group">
    <label class="ctrl-label">Filter Designation</label>
    <asp:DropDownList ID="ddlFltDesig" runat="server" CssClass="ctrl-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed"/>
  </div>
  <div class="ctrl-group">
    <label class="ctrl-label">Filter Subject</label>
    <asp:DropDownList ID="ddlFltSubject" runat="server" CssClass="ctrl-select" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed"/>
  </div>
  <div class="ctrl-group">
    <label class="ctrl-label">Min Periods</label>
    <asp:TextBox ID="txtFltMinP" runat="server" CssClass="ctrl-input" placeholder="e.g. 3" TextMode="Number"/>
  </div>
  <div class="ctrl-actions">
    <asp:Button ID="btnApplyFilter" runat="server" Text="Apply" CssClass="btn btn-primary btn-sm" CausesValidation="false" OnClick="btnApplyFilter_Click"/>
    <asp:Button ID="btnResetFilter" runat="server" Text="Reset"  CssClass="btn btn-ghost btn-sm"   CausesValidation="false" OnClick="btnResetFilter_Click"/>
    <button type="button" class="btn btn-print btn-sm" onclick="doPrint()"> Print Report</button>
  </div>
</div>

<!-- ══════════════════════════════════════════════
     PRINT AREA - everything inside here prints
═══════════════════════════════════════════════ -->
<div id="printArea">

  <!-- Print header (hidden on screen) -->
  <div class="print-header">
    <img class="print-school-logo" src="<%= ResolveUrl("~/images/SchoolLogo.png") %>"
      alt="Government Higher Secondary School Maankot logo" />
    <span class="ph-school">GOVERNMENT HIGHER SECONDARY SCHOOL MAANKOT</span>
    <span class="ph-location">TEHSIL KABIRWALA, DISTRICT KHANEWAL</span>
    <span class="ph-report">Teacher Workload Report</span>
    <span class="ph-date">Generated: <asp:Literal ID="litPrintDate" runat="server"/></span>
    <hr/>
  </div>

  <!-- ── WORKLOAD TABLE ───────────────────────────────────────────── -->
  <div class="wl-table-wrap">
    <div class="wl-table-topbar">
      <span class="wl-table-topbar-title"> Teacher Workload - Periods Per Day</span>
      <span class="wl-table-topbar-count" id="rowCount">
        <asp:Label ID="lblRowCount" runat="server">0 teachers</asp:Label>
      </span>
    </div>
    <div class="wl-scroll">
      <asp:GridView ID="gvWorkload" runat="server"
        AutoGenerateColumns="false" CssClass="wlt" GridLines="None"
        OnRowCommand="gvWorkload_RowCommand"
        OnRowDataBound="gvWorkload_RowDataBound">
        <EmptyDataTemplate>
          <div class="wlt-empty">
            <span class="ei"></span>
            <p>No workload data found. Assign timetable entries to see results here.</p>
          </div>
        </EmptyDataTemplate>
        <Columns>
          <asp:TemplateField HeaderText="#" ItemStyle-CssClass="td-sno">
            <ItemTemplate><span class="td-sno-v"><%# Container.DataItemIndex + 1 %></span></ItemTemplate>
          </asp:TemplateField>

          <asp:TemplateField HeaderText="Teacher">
            <ItemTemplate>
              <div class="u-flex-center-gap-10">
                <div class="teacher-avatar" id="av_<%# Container.DataItemIndex %>">
                  <%# GetInitials(Eval("TeacherName").ToString()) %>
                </div>
                <div>
                  <div class="td-name"><%# Eval("TeacherName") %></div>
                  <div class="workload-subtitle"><%# Eval("Designation") %></div>
                </div>
              </div>
            </ItemTemplate>
          </asp:TemplateField>

          <asp:TemplateField HeaderText="BPS">
            <ItemTemplate>
              <span class="badge <%# GetBpsBadgeClass(Eval("BPS").ToString()) %>">
                <%# string.IsNullOrEmpty(Eval("BPS").ToString()) ? "-" : "BPS-" + Eval("BPS") %>
              </span>
            </ItemTemplate>
          </asp:TemplateField>

          <asp:TemplateField HeaderText="Subjects">
            <ItemTemplate>
              <div class="period-pills"><%# Eval("SubjectList") %></div>
            </ItemTemplate>
          </asp:TemplateField>

          <asp:TemplateField HeaderText="Classes">
            <ItemTemplate>
              <span class="badge badge-teal"><%# Eval("ClassCount") %> class(es)</span>
            </ItemTemplate>
          </asp:TemplateField>

          <asp:TemplateField HeaderText="Workload (Periods/Day)" ItemStyle-CssClass="td-bar">
            <ItemTemplate>
              <div class="bar-wrap">
                <div class="bar-track">
                  <div class="bar-fill <%# GetBarClass(Eval("PeriodCount")) %>"
                       data-pct="<%# Eval("BarPct") %>"></div>
                </div>
                <div class="bar-count"><%# Eval("PeriodCount") %></div>
              </div>
            </ItemTemplate>
          </asp:TemplateField>

          <asp:TemplateField HeaderText="Status" ItemStyle-CssClass="td-center">
            <ItemTemplate>
              <asp:Literal ID="litStatus" runat="server"/>
            </ItemTemplate>
          </asp:TemplateField>    
        </Columns>
      </asp:GridView>
    </div>
  </div>

  <!-- ── CHARTS ROW ────────────────────────────────────────────────── -->
  <div class="chart-grid no-print">
    <!-- Subject Distribution -->
    <div class="chart-card">
      <div class="chart-title"> Subject Distribution</div>
      <div class="chart-sub">Periods assigned per subject</div>
      <div class="subj-chart" id="subjChart">
        <asp:Literal ID="litSubjChart" runat="server"/>
      </div>
    </div>
    <!-- Period Utilisation -->
    <div class="chart-card">
      <div class="chart-title"> Period Utilisation</div>
      <div class="chart-sub">How many teachers occupy each period slot</div>
      <div class="period-util" id="puChart">
        <asp:Literal ID="litPeriodChart" runat="server"/>
      </div>
    </div>
  </div>

  <!-- ── SCHEDULE HEATMAP ──────────────────────────────────────────── -->
  <div class="sec-head no-print u-mt-4">
    <div class="sec-head-left">
      <div class="sec-badge"></div>
      <div>
        <div class="sec-title">Schedule Heatmap</div>
        <div class="sec-sub">Teacher x Period occupancy at a glance</div>
      </div>
    </div>
  </div>
  <div class="timeline-card no-print">
    <asp:Literal ID="litHeatmap" runat="server"/>
  </div>

</div><%-- /printArea --%>

</div><%-- /wl-page --%>

<!-- ══════════════════════════════════════════════
     DETAIL MODAL
═══════════════════════════════════════════════ -->
<div id="detailModal" class="modal-overlay">
  <div class="modal-box">
    <div class="modal-hdr">
      <div class="modal-title" id="mdTitle">Teacher Detail</div>
      <button class="modal-close" onclick="closeModal()"></button>
    </div>
    <div id="mdBody">Loading...</div>
  </div>
</div>

<!-- Hidden JSON payload for JS modal -->
<asp:HiddenField ID="hfTeacherJson" runat="server" Value="[]"/>


<script>
/* ═══════════════════════════════════════════════════
   BAR ANIMATION on load
═══════════════════════════════════════════════════ */
function animateBars(){
  document.querySelectorAll('.bar-fill[data-pct]').forEach(function(el){
    var pct = parseFloat(el.getAttribute('data-pct'))||0;
    setTimeout(function(){ el.style.width = pct + '%'; }, 120);
  });
  document.querySelectorAll('.subj-bar[data-pct]').forEach(function(el){
    var pct = parseFloat(el.getAttribute('data-pct'))||0;
    setTimeout(function(){ el.style.width = pct + '%'; }, 200);
  });
  document.querySelectorAll('.pu-fill[data-pct]').forEach(function(el){
    var pct = parseFloat(el.getAttribute('data-pct'))||0;
    setTimeout(function(){ el.style.width = pct + '%'; }, 200);
  });
}
window.addEventListener('load', animateBars);
// also after UpdatePanel refresh
if(typeof Sys !== 'undefined'){
  Sys.WebForms.PageRequestManager.getInstance().add_endRequest(animateBars);
}

/* ═══════════════════════════════════════════════════
   DETAIL MODAL
═══════════════════════════════════════════════════ */
function openDetail(teacherID){
  var raw = document.getElementById('<%= hfTeacherJson.ClientID %>').value;
        var all = [];
        try { all = JSON.parse(raw); } catch (e) { }
        var t = null;
        for (var i = 0; i < all.length; i++) { if (String(all[i].id) === String(teacherID)) { t = all[i]; break; } }
        if (!t) { alert('Details not available.'); return; }

        document.getElementById('mdTitle').textContent = t.name;
        var html = '';
        html += mdRow('Designation', t.desig || '-');
        html += mdRow('BPS Grade', t.bps ? 'BPS-' + t.bps : '-');
        html += mdRow('Total Periods', '<strong class="workload-total-emphasis">' + t.periods + '</strong>');
        html += mdRow('Classes Taught', t.classes || '-');
        html += mdRow('Subjects', t.subjects || '-');
        html += mdRow('Period Slots', t.slots || '-');
        html += mdRow('Load Status', t.status || '-');
        document.getElementById('mdBody').innerHTML = html;
        document.getElementById('detailModal').classList.add('open');
    }
    function mdRow(label, val) {
        return '<div class="modal-detail-row"><span class="modal-dl">' + label + '</span><span class="modal-dv">' + val + '</span></div>';
    }
    function closeModal() {
        document.getElementById('detailModal').classList.remove('open');
    }
    document.getElementById('detailModal').addEventListener('click', function (e) {
        if (e.target === this) closeModal();
    });

    /* ═══════════════════════════════════════════════════
       REQ #5: single-copy print guard
    ═══════════════════════════════════════════════════ */
    var _printing = false;
    function doPrint() {
        if (_printing) return;
        _printing = true;
        window.print();
        setTimeout(function () { _printing = false; }, 3000);
    }
    window.addEventListener('beforeprint', function () { _printing = true; });
    window.addEventListener('afterprint', function () { _printing = false; });
</script>

</asp:Content>
