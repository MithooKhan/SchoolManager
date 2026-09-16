<%@ Page Title="Daak Diary" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="DaakDiary.aspx.cs" Inherits="DigitalSchoolManager.DaakDiary" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="office-daak-page office-daak-diary-page">
        <section class="office-daak-hero" aria-labelledby="daakDiaryTitle">
            <div class="office-daak-hero-copy">
                <span class="office-daak-kicker">Principal Office Correspondence</span>
                <h1 id="daakDiaryTitle">Incoming Daak Diary</h1>
                <p>Register every letter received from another office, assign its permanent diary number, record its movement, and preserve the scanned original.</p>
                <div class="office-daak-workflow" aria-label="Incoming Daak workflow">
                    <span><strong>01</strong> Receive</span>
                    <span><strong>02</strong> Stamp</span>
                    <span><strong>03</strong> Assign</span>
                    <span><strong>04</strong> File</span>
                </div>
            </div>
            <div class="office-daak-hero-stamp" aria-label="Diary stamp preview">
                <span>Government Higher Secondary School</span>
                <strong>DAAK DIARY</strong>
                <small>Maankot, Kabirwala</small>
                <div><b>Diary No.</b><em>Assigned on save</em></div>
                <div><b>Received</b><em><%= DateTime.Today.ToString("dd MMM yyyy") %></em></div>
            </div>
        </section>

        <nav class="office-daak-command-bar" aria-label="Office Daak workspace">
            <div class="office-daak-command-current">
                <small>Current workspace</small>
                <strong>Incoming Diary</strong>
                <span>Receive, stamp, assign, and monitor office correspondence.</span>
            </div>
            <a class="office-daak-command-switch" href="<%= ResolveUrl("~/DaakDispatch.aspx") %>">
                <small>Switch register</small>
                <strong>Open Daak Dispatch</strong>
                <span>Register a new outgoing letter.</span>
            </a>
            <div class="office-daak-database-status">
                <span class="office-daak-database-dot" aria-hidden="true"></span>
                <div><small>Document storage</small><strong>SchoolDatabase</strong><span>Scanned copies are stored directly in SQL.</span></div>
            </div>
        </nav>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="office-daak-message" role="status" aria-live="polite">
            <asp:Label ID="lblMessage" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlSavedStamp" runat="server" Visible="false" CssClass="office-daak-saved-stamp" role="status">
            <div>
                <span>Diary entry saved</span>
                <strong><asp:Label ID="lblSavedDiaryNo" runat="server" /></strong>
                <small>This number is permanent and should be written on the received letter's diary stamp.</small>
            </div>
            <div class="office-daak-stamp-date">
                <span>Received date</span>
                <strong><asp:Label ID="lblSavedReceivedDate" runat="server" /></strong>
            </div>
        </asp:Panel>

        <section class="office-daak-stat-grid" aria-label="Incoming Daak summary">
            <div><span>Total diary entries</span><strong><asp:Label ID="lblTotalRecords" runat="server" Text="0" /></strong><small>Complete incoming register</small></div>
            <div><span>Received today</span><strong><asp:Label ID="lblTodayRecords" runat="server" Text="0" /></strong><small>Current working date</small></div>
            <div><span>Awaiting action</span><strong><asp:Label ID="lblActiveRecords" runat="server" Text="0" /></strong><small>Received, review, or forwarded</small></div>
            <div><span>Scanned copies</span><strong><asp:Label ID="lblFiledCopies" runat="server" Text="0" /></strong><small>Protected database documents</small></div>
        </section>

        <section class="office-daak-entry-card" aria-labelledby="incomingEntryTitle">
            <asp:HiddenField ID="hfDiaryID" runat="server" Value="0" />
            <div class="office-daak-section-heading">
                <div>
                    <span>New receipt</span>
                    <h2 id="incomingEntryTitle">Register received Daak</h2>
                    <p>Required information is marked. The diary number is generated only after a successful save.</p>
                </div>
                <div class="office-daak-number-format"><small>Number format</small><strong>DY-YYYY-00001</strong></div>
            </div>

            <asp:ValidationSummary ID="vsDiary" runat="server" ValidationGroup="DiaryEntry"
                CssClass="office-daak-validation" HeaderText="Please correct these details:" />

            <div class="office-daak-form-section">
                <div class="office-daak-form-section-title"><span>1</span><div><h3>Letter identity</h3><p>Record the receiving and issuing details.</p></div></div>
                <div class="office-daak-form-grid">
                    <div class="office-daak-field">
                        <asp:Label ID="lblReceivedDatePrompt" runat="server" AssociatedControlID="txtReceivedDate" Text="Date received *" />
                        <asp:TextBox ID="txtReceivedDate" runat="server" TextMode="Date" CssClass="office-daak-control" />
                        <asp:RequiredFieldValidator ID="rfvReceivedDate" runat="server" ControlToValidate="txtReceivedDate"
                            ValidationGroup="DiaryEntry" CssClass="office-daak-field-error" ErrorMessage="Date received is required." Text="Date received is required." />
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblLetterDatePrompt" runat="server" AssociatedControlID="txtLetterDate" Text="Date on letter" />
                        <asp:TextBox ID="txtLetterDate" runat="server" TextMode="Date" CssClass="office-daak-control" />
                    </div>
                    <div class="office-daak-field office-daak-field-wide">
                        <asp:Label ID="lblSenderOfficePrompt" runat="server" AssociatedControlID="txtSenderOffice" Text="Sending office / authority *" />
                        <asp:TextBox ID="txtSenderOffice" runat="server" CssClass="office-daak-control" MaxLength="200" placeholder="Example: District Education Authority, Khanewal" />
                        <asp:RequiredFieldValidator ID="rfvSenderOffice" runat="server" ControlToValidate="txtSenderOffice"
                            ValidationGroup="DiaryEntry" CssClass="office-daak-field-error" ErrorMessage="Sending office is required." Text="Sending office is required." />
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblSenderReferencePrompt" runat="server" AssociatedControlID="txtSenderReferenceNo" Text="Sender reference / letter No." />
                        <asp:TextBox ID="txtSenderReferenceNo" runat="server" CssClass="office-daak-control" MaxLength="100" placeholder="Reference written on the letter" />
                    </div>
                    <div class="office-daak-field office-daak-field-wide">
                        <asp:Label ID="lblSubjectPrompt" runat="server" AssociatedControlID="txtSubject" Text="Subject *" />
                        <asp:TextBox ID="txtSubject" runat="server" CssClass="office-daak-control" MaxLength="300" placeholder="Official subject of the received correspondence" />
                        <asp:RequiredFieldValidator ID="rfvSubject" runat="server" ControlToValidate="txtSubject"
                            ValidationGroup="DiaryEntry" CssClass="office-daak-field-error" ErrorMessage="Letter subject is required." Text="Letter subject is required." />
                    </div>
                </div>
            </div>

            <div class="office-daak-form-section">
                <div class="office-daak-form-section-title"><span>2</span><div><h3>Classification and movement</h3><p>Set handling priority and responsibility.</p></div></div>
                <div class="office-daak-form-grid">
                    <div class="office-daak-field">
                        <asp:Label ID="lblCategoryPrompt" runat="server" AssociatedControlID="ddlCategory" Text="Category" />
                        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="office-daak-control">
                            <asp:ListItem Text="General" Value="General" />
                            <asp:ListItem Text="Academic" Value="Academic" />
                            <asp:ListItem Text="Administrative" Value="Administrative" />
                            <asp:ListItem Text="Finance" Value="Finance" />
                            <asp:ListItem Text="Examination" Value="Examination" />
                            <asp:ListItem Text="Staff" Value="Staff" />
                            <asp:ListItem Text="Student" Value="Student" />
                            <asp:ListItem Text="Legal" Value="Legal" />
                        </asp:DropDownList>
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblPriorityPrompt" runat="server" AssociatedControlID="ddlPriority" Text="Priority" />
                        <asp:DropDownList ID="ddlPriority" runat="server" CssClass="office-daak-control">
                            <asp:ListItem Text="Normal" Value="Normal" />
                            <asp:ListItem Text="Important" Value="Important" />
                            <asp:ListItem Text="Urgent" Value="Urgent" />
                        </asp:DropDownList>
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblDeliveryModePrompt" runat="server" AssociatedControlID="ddlDeliveryMode" Text="Received through" />
                        <asp:DropDownList ID="ddlDeliveryMode" runat="server" CssClass="office-daak-control">
                            <asp:ListItem Text="By Hand" Value="By Hand" />
                            <asp:ListItem Text="Post / Courier" Value="Post / Courier" />
                            <asp:ListItem Text="Official Messenger" Value="Official Messenger" />
                            <asp:ListItem Text="Email Print" Value="Email Print" />
                            <asp:ListItem Text="Other" Value="Other" />
                        </asp:DropDownList>
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblStatusPrompt" runat="server" AssociatedControlID="ddlStatus" Text="Current status" />
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="office-daak-control">
                            <asp:ListItem Text="Received" Value="Received" />
                            <asp:ListItem Text="Under Review" Value="Under Review" />
                            <asp:ListItem Text="Forwarded" Value="Forwarded" />
                            <asp:ListItem Text="Action Completed" Value="Action Completed" />
                            <asp:ListItem Text="Filed" Value="Filed" />
                        </asp:DropDownList>
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblAssignedToPrompt" runat="server" AssociatedControlID="txtAssignedTo" Text="Marked / assigned to" />
                        <asp:TextBox ID="txtAssignedTo" runat="server" CssClass="office-daak-control" MaxLength="150" placeholder="Name, post, or office section" />
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblActionDueDatePrompt" runat="server" AssociatedControlID="txtActionDueDate" Text="Action due date" />
                        <asp:TextBox ID="txtActionDueDate" runat="server" TextMode="Date" CssClass="office-daak-control" />
                    </div>
                </div>
            </div>

            <div class="office-daak-form-section">
                <div class="office-daak-form-section-title"><span>3</span><div><h3>Details and database archive</h3><p>Preserve instructions and the scanned original inside SchoolDatabase.</p></div></div>
                <div class="office-daak-form-grid">
                    <div class="office-daak-field office-daak-field-full">
                        <asp:Label ID="lblDescriptionPrompt" runat="server" AssociatedControlID="txtDescription" Text="Description / action required" />
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4" CssClass="office-daak-control office-daak-textarea" MaxLength="2000" placeholder="Summarize the letter, instructions received, and action required." />
                    </div>
                    <div class="office-daak-field office-daak-field-wide">
                        <asp:Label ID="lblRemarksPrompt" runat="server" AssociatedControlID="txtRemarks" Text="Office remarks" />
                        <asp:TextBox ID="txtRemarks" runat="server" TextMode="MultiLine" Rows="3" CssClass="office-daak-control office-daak-textarea" MaxLength="500" placeholder="Optional internal remarks" />
                    </div>
                    <div class="office-daak-field office-daak-upload-field">
                        <asp:Label ID="lblDocumentPrompt" runat="server" AssociatedControlID="fuDaakDocument" Text="Scanned letter / office copy" />
                        <div class="office-daak-upload-box">
                            <asp:FileUpload ID="fuDaakDocument" runat="server" AllowMultiple="true" CssClass="office-daak-file-control" accept=".jpg,.jpeg,.png,.gif,.bmp,.pdf" />
                            <strong>Upload one or several scanned pages</strong>
                            <small>Choose multiple images or PDFs together. Images are optimized below 1 MB and each PDF may be up to 5 MB. All pages are stored in SQL Server.</small>
                        </div>
                    </div>
                </div>
            </div>

            <div class="office-daak-action-row">
                <asp:Button ID="btnSaveDiary" runat="server" Text="Save and Assign Diary Number"
                    CssClass="office-daak-button office-daak-button-primary" ValidationGroup="DiaryEntry" OnClick="btnSaveDiary_Click" />
                <asp:Button ID="btnClearDiary" runat="server" Text="Clear Entry"
                    CssClass="office-daak-button office-daak-button-secondary" CausesValidation="false" OnClick="btnClearDiary_Click" />
            </div>
        </section>

        <section class="office-daak-register-card" aria-labelledby="diaryRegisterTitle">
            <div class="office-daak-section-heading office-daak-register-heading">
                <div>
                    <span>Permanent register</span>
                    <h2 id="diaryRegisterTitle">Incoming Daak records</h2>
                    <p>Search by diary number, sender, reference number, or subject.</p>
                </div>
            </div>

            <div class="office-daak-filter-grid">
                <div class="office-daak-field office-daak-filter-search">
                    <asp:Label ID="lblSearchPrompt" runat="server" AssociatedControlID="txtSearch" Text="Search register" />
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="office-daak-control" placeholder="Diary No., sender, reference, or subject" />
                </div>
                <div class="office-daak-field">
                    <asp:Label ID="lblFilterStatusPrompt" runat="server" AssociatedControlID="ddlFilterStatus" Text="Status" />
                    <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="office-daak-control">
                        <asp:ListItem Text="All statuses" Value="" />
                        <asp:ListItem Text="Received" Value="Received" />
                        <asp:ListItem Text="Under Review" Value="Under Review" />
                        <asp:ListItem Text="Forwarded" Value="Forwarded" />
                        <asp:ListItem Text="Action Completed" Value="Action Completed" />
                        <asp:ListItem Text="Filed" Value="Filed" />
                    </asp:DropDownList>
                </div>
                <div class="office-daak-field">
                    <asp:Label ID="lblFromDatePrompt" runat="server" AssociatedControlID="txtFromDate" Text="From date" />
                    <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date" CssClass="office-daak-control" />
                </div>
                <div class="office-daak-field">
                    <asp:Label ID="lblToDatePrompt" runat="server" AssociatedControlID="txtToDate" Text="To date" />
                    <asp:TextBox ID="txtToDate" runat="server" TextMode="Date" CssClass="office-daak-control" />
                </div>
                <div class="office-daak-filter-actions">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="office-daak-button office-daak-button-primary" CausesValidation="false" OnClick="btnSearch_Click" />
                    <asp:Button ID="btnResetSearch" runat="server" Text="Reset" CssClass="office-daak-button office-daak-button-secondary" CausesValidation="false" OnClick="btnResetSearch_Click" />
                </div>
            </div>

            <div class="office-daak-grid-wrap">
                <asp:GridView ID="gvDiary" runat="server" AutoGenerateColumns="false" DataKeyNames="DiaryID"
                    CssClass="office-daak-grid" GridLines="None" AllowPaging="true" PageSize="10"
                    EmptyDataText="No incoming Daak records match the selected filters."
                    OnPageIndexChanging="gvDiary_PageIndexChanging" OnRowCommand="gvDiary_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="No."><ItemTemplate><%# Container.DataItemIndex + 1 + (gvDiary.PageIndex * gvDiary.PageSize) %></ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="DiaryNo" HeaderText="Diary number" />
                        <asp:BoundField DataField="ReceivedDate" HeaderText="Received" DataFormatString="{0:dd MMM yyyy}" />
                        <asp:BoundField DataField="SenderOffice" HeaderText="Sending office" />
                        <asp:BoundField DataField="Subject" HeaderText="Subject" />
                        <asp:BoundField DataField="Priority" HeaderText="Priority" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                        <asp:TemplateField HeaderText="Record actions">
                            <ItemTemplate>
                                <div class="office-daak-row-actions"><asp:LinkButton ID="btnEditRecord" runat="server" Text="Edit" CssClass="office-daak-record-edit" CommandName="EditRecord" CommandArgument='<%# Eval("DiaryID") %>' CausesValidation="false" />
                                <a class="office-daak-document-button" href='<%# "DaakDocuments.aspx?type=incoming&id="+Eval("DiaryID") %>'>Pages (<%# Eval("DocumentCount") %>)</a></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="office-daak-pager" />
                </asp:GridView>
            </div>
        </section>
    </div>
</asp:Content>
