<%@ Page Title="Board Result Report" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="BoardResultReport.aspx.cs" Inherits="DigitalSchoolManager.BoardResultReport" %>

<asp:Content ID="BoardReportHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .board-report-page{max-width:1500px;margin:0 auto;padding:28px 22px 50px;color:#111}.board-report-toolbar{display:flex;align-items:center;justify-content:space-between;gap:18px;margin-bottom:18px;padding:17px 20px;border:1px solid #b6d5c3;border-radius:14px;background:#fff;box-shadow:0 8px 22px rgba(4,61,39,.08)}
        .board-report-toolbar span,.board-report-toolbar strong,.board-report-toolbar small{display:block}.board-report-toolbar span{color:#0b6a41;font-size:.7rem;font-weight:900;letter-spacing:.1em;text-transform:uppercase}.board-report-toolbar strong{margin:3px 0;color:#073f29;font-size:1.15rem}.board-report-toolbar small{color:#63776c}.board-report-actions{display:flex;gap:9px}.board-report-actions a,.board-report-actions button{display:inline-flex;align-items:center;min-height:40px;padding:8px 14px;border:1px solid #8ebda3;border-radius:8px;font:inherit;font-size:.78rem;font-weight:900;text-decoration:none;cursor:pointer}.board-report-actions a{color:#0a4e31;background:#edf8f2}.board-report-actions button{color:#fff;background:#087347}
        .board-report-error{padding:16px;border:1px solid #e49b9b;border-radius:12px;color:#842020;background:#fff0f0;font-weight:800}.official-sheet{box-sizing:border-box;width:100%;min-height:190mm;margin:0 auto 18px;padding:8mm;border:1px solid #9faaa4;background:#fff;box-shadow:0 12px 32px rgba(0,0,0,.1);font-family:"Times New Roman",Times,serif}.official-sheet.page-break{page-break-after:always}
        .official-header{display:grid;grid-template-columns:62px 1fr 62px;align-items:center;gap:10px;margin-bottom:8px;text-align:center}.official-header img{width:56px;height:56px;object-fit:contain}.official-header h1{margin:0;color:#000;font-size:17pt;font-weight:800;text-transform:uppercase}.official-header p{margin:4px 0 0;color:#000;font-size:12pt;font-weight:800;text-transform:uppercase}.official-file-mark{width:54px;height:54px;display:grid;place-items:center;border:2px solid #bd8c19;border-radius:50%;color:#06432c;font-family:Arial,sans-serif;font-size:8pt;font-weight:900}
        .official-meta{display:flex;justify-content:center;gap:10px;margin:-1px 0 9px;color:#222;font:700 8pt Arial,sans-serif}.official-meta span{padding:3px 8px;border:1px solid #8e9d95;border-radius:999px}.official-grid{width:100%;border-collapse:collapse;table-layout:fixed}.official-grid th,.official-grid td{border:1px solid #111;padding:4px 3px;text-align:center;vertical-align:middle;overflow-wrap:anywhere}.official-grid th{font-size:6.5pt;font-weight:800;line-height:1.05;text-transform:uppercase}.official-grid td{font-size:7.5pt;line-height:1.12}.official-grid.head-grid td{font-size:7pt}.official-grid.teacher-grid th{font-size:6.2pt}.official-grid.teacher-grid td{font-size:7.2pt}.official-grid.position-grid{width:74%;margin-top:4px}.official-grid.position-grid th{font-size:7pt}.official-grid.position-grid td{text-align:left;font-size:8pt}.official-grid.position-grid td.num{text-align:center}
        .official-subheading{display:flex;justify-content:space-between;align-items:end;gap:15px;margin:12px 0 5px}.official-subheading h2{margin:0;font-size:10pt;text-transform:uppercase}.official-subheading span{font-size:8pt;font-weight:700}.official-signature{display:flex;justify-content:flex-end;margin-top:18px;padding-right:55px;font-size:9pt;font-weight:800}.official-status-above{font-weight:800;color:#07592f}.official-status-below{font-weight:800;color:#7c1c18}.official-status-equal{font-weight:800;color:#6a500d}.official-note{margin:8px 0 0;font-size:7.5pt;color:#333}
        @media print{@page{size:A4 landscape;margin:7mm}html,body{background:#fff!important}.site-header,.site-footer,.board-report-toolbar,.no-print{display:none!important}.site-main,.main-content,#main-content{padding:0!important;margin:0!important;max-width:none!important}.board-report-page{max-width:none;padding:0}.official-sheet{width:100%;min-height:0;margin:0;padding:0;border:0;box-shadow:none}.official-sheet:last-child{page-break-after:auto}}
        @media(max-width:760px){.board-report-toolbar{align-items:flex-start;flex-direction:column}.official-sheet{overflow:auto}.board-report-actions{flex-wrap:wrap}}
    </style>
</asp:Content>

<asp:Content ID="BoardReportContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="board-report-page">
        <asp:Panel ID="pnlReportError" runat="server" Visible="false" CssClass="board-report-error">
            <asp:Label ID="lblReportError" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlReport" runat="server" Visible="false">
            <div class="board-report-toolbar no-print">
                <div><span>Official board result statement</span><strong><asp:Label ID="lblReportTitle" runat="server" /></strong><small><asp:Label ID="lblReportMeta" runat="server" /></small></div>
                <div class="board-report-actions"><asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/BoardResultManagement.aspx" Text="Back to Records" /><button type="button" onclick="window.print();">Print / Save PDF</button></div>
            </div>
            <asp:Literal ID="litReport" runat="server" />
        </asp:Panel>
    </div>
</asp:Content>
