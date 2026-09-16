<%@ Page Title="Daak Dispatch" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="DaakDispatch.aspx.cs" Inherits="DigitalSchoolManager.DaakDispatch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="office-daak-page office-daak-dispatch-page">
        <section class="office-daak-hero office-daak-dispatch-hero" aria-labelledby="daakDispatchTitle">
            <div class="office-daak-hero-copy">
                <span class="office-daak-kicker">Principal Office Correspondence</span>
                <h1 id="daakDispatchTitle">Outgoing Daak Dispatch</h1>
                <p>Assign a controlled dispatch number to every letter issued by the Principal's office, record delivery details, and preserve the signed office copy.</p>
                <div class="office-daak-workflow" aria-label="Outgoing Daak workflow">
                    <span><strong>01</strong> Prepare</span>
                    <span><strong>02</strong> Approve</span>
                    <span><strong>03</strong> Dispatch</span>
                    <span><strong>04</strong> Archive</span>
                </div>
            </div>
            <div class="office-daak-hero-stamp office-daak-dispatch-stamp" aria-label="Dispatch register preview">
                <span>Government Higher Secondary School</span>
                <strong>DAAK DISPATCH</strong>
                <small>Principal Office, Maankot</small>
                <div><b>Dispatch No.</b><em>Assigned on save</em></div>
                <div><b>Date</b><em><%= DateTime.Today.ToString("dd MMM yyyy") %></em></div>
            </div>
        </section>

        <nav class="office-daak-command-bar" aria-label="Office Daak workspace">
            <div class="office-daak-command-current office-daak-command-current-dispatch">
                <small>Current workspace</small>
                <strong>Outgoing Dispatch</strong>
                <span>Prepare, approve, dispatch, and track official letters.</span>
            </div>
            <a class="office-daak-command-switch" href="<%= ResolveUrl("~/DaakDiary.aspx") %>">
                <small>Switch register</small>
                <strong>Open Daak Diary</strong>
                <span>Register newly received correspondence.</span>
            </a>
            <div class="office-daak-database-status">
                <span class="office-daak-database-dot" aria-hidden="true"></span>
                <div><small>Document storage</small><strong>SchoolDatabase</strong><span>Signed office copies are stored directly in SQL.</span></div>
            </div>
        </nav>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="office-daak-message" role="status" aria-live="polite">
            <asp:Label ID="lblMessage" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlSavedReceipt" runat="server" Visible="false" CssClass="office-daak-saved-stamp office-daak-saved-dispatch" role="status">
            <div>
                <span>Dispatch entry saved</span>
                <strong><asp:Label ID="lblSavedDispatchNo" runat="server" /></strong>
                <small>This permanent number should appear on the outgoing letter and office copy.</small>
            </div>
            <div class="office-daak-stamp-date">
                <span>Dispatch date</span>
                <strong><asp:Label ID="lblSavedDispatchDate" runat="server" /></strong>
            </div>
        </asp:Panel>

        <section class="office-daak-stat-grid" aria-label="Outgoing Daak summary">
            <div><span>Total dispatches</span><strong><asp:Label ID="lblTotalRecords" runat="server" Text="0" /></strong><small>Complete outgoing register</small></div>
            <div><span>Dispatched today</span><strong><asp:Label ID="lblTodayRecords" runat="server" Text="0" /></strong><small>Current working date</small></div>
            <div><span>In dispatch process</span><strong><asp:Label ID="lblActiveRecords" runat="server" Text="0" /></strong><small>Prepared or dispatched</small></div>
            <div><span>Office copies</span><strong><asp:Label ID="lblFiledCopies" runat="server" Text="0" /></strong><small>Protected database documents</small></div>
        </section>

        <section class="office-daak-entry-card" aria-labelledby="dispatchEntryTitle">
            <asp:HiddenField ID="hfDispatchID" runat="server" Value="0" />
            <div class="office-daak-section-heading">
                <div>
                    <span>New dispatch</span>
                    <h2 id="dispatchEntryTitle">Register outgoing letter</h2>
                    <p>Record the receiving office and delivery trail. The dispatch number is assigned after a successful save.</p>
                </div>
                <div class="office-daak-number-format"><small>Number format</small><strong>DSP-YYYY-00001</strong></div>
            </div>

            <asp:ValidationSummary ID="vsDispatch" runat="server" ValidationGroup="DispatchEntry"
                CssClass="office-daak-validation" HeaderText="Please correct these details:" />

            <div class="office-daak-form-section">
                <div class="office-daak-form-section-title"><span>1</span><div><h3>Letter and recipient</h3><p>Identify the document and destination office.</p></div></div>
                <div class="office-daak-form-grid">
                    <div class="office-daak-field">
                        <asp:Label ID="lblDispatchDatePrompt" runat="server" AssociatedControlID="txtDispatchDate" Text="Date dispatched *" />
                        <asp:TextBox ID="txtDispatchDate" runat="server" TextMode="Date" CssClass="office-daak-control" />
                        <asp:RequiredFieldValidator ID="rfvDispatchDate" runat="server" ControlToValidate="txtDispatchDate"
                            ValidationGroup="DispatchEntry" CssClass="office-daak-field-error" ErrorMessage="Dispatch date is required." Text="Dispatch date is required." />
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblLetterDatePrompt" runat="server" AssociatedControlID="txtLetterDate" Text="Date on letter" />
                        <asp:TextBox ID="txtLetterDate" runat="server" TextMode="Date" CssClass="office-daak-control" />
                    </div>
                    <div class="office-daak-field office-daak-field-wide">
                        <asp:Label ID="lblRecipientOfficePrompt" runat="server" AssociatedControlID="txtRecipientOffice" Text="Receiving office / authority *" />
                        <asp:TextBox ID="txtRecipientOffice" runat="server" CssClass="office-daak-control" MaxLength="200" placeholder="Office or authority receiving the letter" />
                        <asp:RequiredFieldValidator ID="rfvRecipientOffice" runat="server" ControlToValidate="txtRecipientOffice"
                            ValidationGroup="DispatchEntry" CssClass="office-daak-field-error" ErrorMessage="Receiving office is required." Text="Receiving office is required." />
                    </div>
                    <div class="office-daak-field office-daak-field-wide">
                        <asp:Label ID="lblRecipientAddressPrompt" runat="server" AssociatedControlID="txtRecipientAddress" Text="Postal / office address" />
                        <asp:TextBox ID="txtRecipientAddress" runat="server" CssClass="office-daak-control" MaxLength="350" placeholder="Complete dispatch address" />
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblRecipientContactPrompt" runat="server" AssociatedControlID="txtRecipientContact" Text="Recipient contact" />
                        <asp:TextBox ID="txtRecipientContact" runat="server" CssClass="office-daak-control" MaxLength="100" placeholder="Phone or official email" />
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblReferenceNoPrompt" runat="server" AssociatedControlID="txtReferenceNo" Text="Related reference No." />
                        <asp:TextBox ID="txtReferenceNo" runat="server" CssClass="office-daak-control" MaxLength="100" placeholder="Earlier letter or case reference" />
                    </div>
                    <div class="office-daak-field office-daak-field-full">
                        <asp:Label ID="lblSubjectPrompt" runat="server" AssociatedControlID="txtSubject" Text="Subject *" />
                        <asp:TextBox ID="txtSubject" runat="server" CssClass="office-daak-control" MaxLength="300" placeholder="Official subject of the outgoing correspondence" />
                        <asp:RequiredFieldValidator ID="rfvSubject" runat="server" ControlToValidate="txtSubject"
                            ValidationGroup="DispatchEntry" CssClass="office-daak-field-error" ErrorMessage="Letter subject is required." Text="Letter subject is required." />
                    </div>
                </div>
            </div>

            <div class="office-daak-form-section">
                <div class="office-daak-form-section-title"><span>2</span><div><h3>Approval and delivery trail</h3><p>Capture responsibility and traceable delivery details.</p></div></div>
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
                        <asp:Label ID="lblDispatchModePrompt" runat="server" AssociatedControlID="ddlDispatchMode" Text="Dispatch method" />
                        <asp:DropDownList ID="ddlDispatchMode" runat="server" CssClass="office-daak-control">
                            <asp:ListItem Text="By Hand" Value="By Hand" />
                            <asp:ListItem Text="Post / Courier" Value="Post / Courier" />
                            <asp:ListItem Text="Official Messenger" Value="Official Messenger" />
                            <asp:ListItem Text="Email" Value="Email" />
                            <asp:ListItem Text="Other" Value="Other" />
                        </asp:DropDownList>
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblTrackingNoPrompt" runat="server" AssociatedControlID="txtTrackingNo" Text="Courier / tracking / receipt No." />
                        <asp:TextBox ID="txtTrackingNo" runat="server" CssClass="office-daak-control" MaxLength="100" placeholder="Optional delivery proof number" />
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblPreparedByPrompt" runat="server" AssociatedControlID="txtPreparedBy" Text="Prepared by" />
                        <asp:TextBox ID="txtPreparedBy" runat="server" CssClass="office-daak-control" MaxLength="150" placeholder="Name and designation" />
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblSignedByPrompt" runat="server" AssociatedControlID="txtSignedBy" Text="Approved / signed by" />
                        <asp:TextBox ID="txtSignedBy" runat="server" CssClass="office-daak-control" MaxLength="150" placeholder="Principal or approving officer" />
                    </div>
                    <div class="office-daak-field">
                        <asp:Label ID="lblStatusPrompt" runat="server" AssociatedControlID="ddlStatus" Text="Current status" />
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="office-daak-control">
                            <asp:ListItem Text="Prepared" Value="Prepared" />
                            <asp:ListItem Text="Dispatched" Value="Dispatched" Selected="True" />
                            <asp:ListItem Text="Acknowledged" Value="Acknowledged" />
                            <asp:ListItem Text="Returned" Value="Returned" />
                            <asp:ListItem Text="Closed" Value="Closed" />
                        </asp:DropDownList>
                    </div>
                </div>
            </div>

            <div class="office-daak-form-section">
                <div class="office-daak-form-section-title"><span>3</span><div><h3>Contents and database archive</h3><p>Preserve the dispatch purpose and signed office copy inside SchoolDatabase.</p></div></div>
                <div class="office-daak-form-grid">
                    <div class="office-daak-field office-daak-field-full">
                        <asp:Label ID="lblDescriptionPrompt" runat="server" AssociatedControlID="txtDescription" Text="Description / purpose" />
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4" CssClass="office-daak-control office-daak-textarea" MaxLength="2000" placeholder="Summarize the outgoing letter, enclosures, and required response." />
                    </div>
                    <div class="office-daak-field office-daak-field-wide">
                        <asp:Label ID="lblRemarksPrompt" runat="server" AssociatedControlID="txtRemarks" Text="Dispatch remarks" />
                        <asp:TextBox ID="txtRemarks" runat="server" TextMode="MultiLine" Rows="3" CssClass="office-daak-control office-daak-textarea" MaxLength="500" placeholder="Optional internal notes or acknowledgement details" />
                    </div>
                    <div class="office-daak-field office-daak-upload-field">
                        <asp:Label ID="lblDocumentPrompt" runat="server" AssociatedControlID="fuDispatchDocument" Text="Signed original / office copy" />
                        <div class="office-daak-upload-box">
                            <asp:FileUpload ID="fuDispatchDocument" runat="server" AllowMultiple="true" CssClass="office-daak-file-control" accept=".jpg,.jpeg,.png,.gif,.bmp,.pdf" />
                            <strong>Archive one or several signed pages</strong>
                            <small>Choose multiple images or PDFs together. Images are optimized below 1 MB and each PDF may be up to 5 MB. All pages are stored in SQL Server.</small>
                        </div>
                    </div>
                </div>
            </div>

            <div class="office-daak-action-row">
                <asp:Button ID="btnSaveDispatch" runat="server" Text="Save and Assign Dispatch Number"
                    CssClass="office-daak-button office-daak-button-primary office-daak-dispatch-button" ValidationGroup="DispatchEntry" OnClick="btnSaveDispatch_Click" />
                <asp:Button ID="btnClearDispatch" runat="server" Text="Clear Entry"
                    CssClass="office-daak-button office-daak-button-secondary" CausesValidation="false" OnClick="btnClearDispatch_Click" />
            </div>
        </section>

        <section class="office-daak-register-card" aria-labelledby="dispatchRegisterTitle">
            <div class="office-daak-section-heading office-daak-register-heading">
                <div>
                    <span>Permanent register</span>
                    <h2 id="dispatchRegisterTitle">Outgoing dispatch records</h2>
                    <p>Search by dispatch number, receiving office, reference, tracking number, or subject.</p>
                </div>
            </div>

            <div class="office-daak-filter-grid">
                <div class="office-daak-field office-daak-filter-search">
                    <asp:Label ID="lblSearchPrompt" runat="server" AssociatedControlID="txtSearch" Text="Search register" />
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="office-daak-control" placeholder="Dispatch No., office, reference, tracking, or subject" />
                </div>
                <div class="office-daak-field">
                    <asp:Label ID="lblFilterStatusPrompt" runat="server" AssociatedControlID="ddlFilterStatus" Text="Status" />
                    <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="office-daak-control">
                        <asp:ListItem Text="All statuses" Value="" />
                        <asp:ListItem Text="Prepared" Value="Prepared" />
                        <asp:ListItem Text="Dispatched" Value="Dispatched" />
                        <asp:ListItem Text="Acknowledged" Value="Acknowledged" />
                        <asp:ListItem Text="Returned" Value="Returned" />
                        <asp:ListItem Text="Closed" Value="Closed" />
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
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="office-daak-button office-daak-button-primary office-daak-dispatch-button" CausesValidation="false" OnClick="btnSearch_Click" />
                    <asp:Button ID="btnResetSearch" runat="server" Text="Reset" CssClass="office-daak-button office-daak-button-secondary" CausesValidation="false" OnClick="btnResetSearch_Click" />
                </div>
            </div>

            <div class="office-daak-grid-wrap">
                <asp:GridView ID="gvDispatch" runat="server" AutoGenerateColumns="false" DataKeyNames="DispatchID"
                    CssClass="office-daak-grid office-daak-dispatch-grid" GridLines="None" AllowPaging="true" PageSize="10"
                    EmptyDataText="No outgoing Daak records match the selected filters."
                    OnPageIndexChanging="gvDispatch_PageIndexChanging" OnRowCommand="gvDispatch_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="No."><ItemTemplate><%# Container.DataItemIndex + 1 + (gvDispatch.PageIndex * gvDispatch.PageSize) %></ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="DispatchNo" HeaderText="Dispatch number" />
                        <asp:BoundField DataField="DispatchDate" HeaderText="Date" DataFormatString="{0:dd MMM yyyy}" />
                        <asp:BoundField DataField="RecipientOffice" HeaderText="Receiving office" />
                        <asp:BoundField DataField="Subject" HeaderText="Subject" />
                        <asp:BoundField DataField="DispatchMode" HeaderText="Method" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                        <asp:TemplateField HeaderText="Record actions">
                            <ItemTemplate>
                                <div class="office-daak-row-actions"><asp:LinkButton ID="btnEditRecord" runat="server" Text="Edit" CssClass="office-daak-record-edit" CommandName="EditRecord" CommandArgument='<%# Eval("DispatchID") %>' CausesValidation="false" />
                                <a class="office-daak-document-button office-daak-dispatch-document" href='<%# "DaakDocuments.aspx?type=outgoing&id="+Eval("DispatchID") %>'>Pages (<%# Eval("DocumentCount") %>)</a></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="office-daak-pager" />
                </asp:GridView>
            </div>
        </section>
    </div>
</asp:Content>
