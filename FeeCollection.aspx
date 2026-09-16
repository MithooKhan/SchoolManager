<%@ Page Title="Fee Collection" Language="C#" MasterPageFile="~/DSM.Master" AutoEventWireup="true" CodeBehind="FeeCollection.aspx.cs" Inherits="DigitalSchoolManager.FeeCollection" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="fee-collection-page">
        <section class="fee-collection-hero" aria-labelledby="feeCollectionTitle">
            <div class="fee-collection-hero-copy">
                <span class="fee-module-label">Finance and Accounts</span>
                <h1 id="feeCollectionTitle">Fee Collection</h1>
                <p>Select a student, enter the received charges, and save a complete fee voucher.</p>
            </div>
            <a id="lnkFeeRates" runat="server" class="fee-rates-link" href="SchoolFeeManagement.aspx">
                <span>Fee Setup</span>
                <strong>Add Fee Rates</strong>
            </a>
        </section>

        <div class="fee-process-strip" aria-label="Fee collection process">
            <div class="fee-process-step is-current">
                <span>1</span>
                <div><strong>Find Student</strong><small>Class or direct search</small></div>
            </div>
            <div class="fee-process-step">
                <span>2</span>
                <div><strong>Enter Charges</strong><small>Review every fee item</small></div>
            </div>
            <div class="fee-process-step">
                <span>3</span>
                <div><strong>Save Voucher</strong><small>Record the collection</small></div>
            </div>
        </div>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="fee-message" role="status" aria-live="polite">
            <asp:Label ID="lblMessage" runat="server" />
        </asp:Panel>

        <section class="fee-selection-card" aria-labelledby="studentSelectionTitle">
            <div class="fee-section-heading">
                <div>
                    <span class="fee-section-kicker">Student lookup</span>
                    <h2 id="studentSelectionTitle">Select the student</h2>
                </div>
                <p>Choose a class to load its students, or search across all records.</p>
            </div>

            <div class="fee-student-filters">
                <div class="fee-field">
                    <asp:Label ID="lblFeeMonthPrompt" runat="server" AssociatedControlID="ddlFeeMonth"
                        Text="Fee month" CssClass="fee-field-label" />
                    <asp:DropDownList ID="ddlFeeMonth" runat="server" CssClass="fee-control"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlFeePeriod_SelectedIndexChanged" />
                    <small>The month this voucher covers.</small>
                </div>

                <div class="fee-field">
                    <asp:Label ID="lblFeeYearPrompt" runat="server" AssociatedControlID="ddlFeeYear"
                        Text="Fee year" CssClass="fee-field-label" />
                    <asp:DropDownList ID="ddlFeeYear" runat="server" CssClass="fee-control"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlFeePeriod_SelectedIndexChanged" />
                    <small>Duplicate collection is prevented per period.</small>
                </div>

                <div class="fee-field">
                    <asp:Label ID="lblClassPrompt" runat="server" AssociatedControlID="ddlClass" Text="Class" CssClass="fee-field-label" />
                    <asp:DropDownList ID="ddlClass" runat="server" CssClass="fee-control"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlClass_SelectedIndexChanged" />
                    <small>Selecting a class loads its student list.</small>
                </div>

                <div class="fee-field fee-field-wide">
                    <asp:Label ID="lblSearchPrompt" runat="server" AssociatedControlID="txtStudentSearch"
                        Text="Search student" CssClass="fee-field-label" />
                    <div class="fee-search-control">
                        <asp:TextBox ID="txtStudentSearch" runat="server" CssClass="fee-control"
                            MaxLength="100" placeholder="Name, Registration No, or Form-B No" />
                        <asp:Button ID="btnSearchStudent" runat="server" Text="Search"
                            CssClass="fee-button fee-button-secondary" CausesValidation="false"
                            OnClick="btnSearchStudent_Click" />
                    </div>
                    <small>The search can be used with or without a selected class.</small>
                </div>

                <div class="fee-field fee-field-student">
                    <asp:Label ID="lblStudentPrompt" runat="server" AssociatedControlID="ddlStudent"
                        Text="Student" CssClass="fee-field-label" />
                    <asp:DropDownList ID="ddlStudent" runat="server" CssClass="fee-control"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlStudent_SelectedIndexChanged" />
                    <small>Choose a student to open the fee voucher.</small>
                </div>

                <div class="fee-filter-action">
                    <asp:Button ID="btnClearSelection" runat="server" Text="Clear Selection"
                        CssClass="fee-button fee-button-quiet" CausesValidation="false"
                        OnClick="btnClearSelection_Click" />
                </div>
            </div>
        </section>

        <asp:Panel ID="pnlVoucher" runat="server" Visible="false" CssClass="fee-voucher-shell">
            <asp:HiddenField ID="hfStudentID" runat="server" />
            <asp:HiddenField ID="hfClassID" runat="server" />
            <asp:HiddenField ID="hfVoucherNo" runat="server" />
            <asp:HiddenField ID="hfCalculatedTotal" runat="server" ClientIDMode="Static" Value="0" />

            <article id="feeVoucherPrintArea" class="fee-voucher" aria-labelledby="voucherHeading">
                <header class="fee-voucher-header">
                    <div class="fee-voucher-government-mark">
                        <img src="<%= ResolveUrl("~/images/SchoolLogo.png") %>" alt="Government Higher Secondary School Maankot logo" />
                    </div>
                    <div class="fee-voucher-school">
                        <span>Government of the Punjab</span>
                        <h2 id="voucherHeading">Government Higher Secondary School Maankot</h2>
                        <p>Official Student Fee Collection Voucher</p>
                    </div>
                    <div class="fee-voucher-reference">
                        <small>Voucher No</small>
                        <asp:Label ID="lblVoucherNo" runat="server" />
                        <small>Fee period</small>
                        <asp:Label ID="lblFeePeriod" runat="server" />
                        <small>Collection date</small>
                        <asp:TextBox ID="txtDepositDate" runat="server" CssClass="fee-date-control" TextMode="Date" />
                    </div>
                </header>

                <section class="fee-student-summary" aria-label="Selected student details">
                    <div class="fee-student-avatar" aria-hidden="true">
                        <asp:Label ID="lblStudentInitials" runat="server" />
                    </div>
                    <div class="fee-student-primary">
                        <small>Student name</small>
                        <asp:Label ID="lblStudentName" runat="server" CssClass="fee-student-name" />
                        <span>Father: <asp:Label ID="lblFatherName" runat="server" /></span>
                    </div>
                    <dl class="fee-student-facts">
                        <div><dt>Registration No</dt><dd><asp:Label ID="lblRegistrationNo" runat="server" /></dd></div>
                        <div><dt>Form-B No</dt><dd><asp:Label ID="lblFormBNo" runat="server" /></dd></div>
                        <div><dt>Class</dt><dd><asp:Label ID="lblClassName" runat="server" /></dd></div>
                        <div><dt>Roll No</dt><dd><asp:Label ID="lblRollNo" runat="server" /></dd></div>
                    </dl>
                </section>

                <section class="fee-voucher-body">
                    <asp:Panel ID="pnlAlreadyPaid" runat="server" Visible="false"
                        CssClass="fee-already-paid" role="status">
                        <span class="fee-paid-stamp">PAID</span>
                        <div>
                            <strong>This student has already deposited the fee for <asp:Label ID="lblPaidPeriod" runat="server" />.</strong>
                            <small>The saved voucher is displayed below and can be printed again. A duplicate payment cannot be submitted.</small>
                        </div>
                    </asp:Panel>

                    <div class="fee-table-heading">
                        <div>
                            <span>Fee details</span>
                            <h3>Charges received</h3>
                        </div>
                        <p>Enter zero for any charge that is not being collected.</p>
                    </div>

                    <div class="fee-table-wrap">
                        <asp:GridView ID="gvFeeItems" runat="server" AutoGenerateColumns="false"
                            DataKeyNames="FeeID" CssClass="fee-collection-grid" GridLines="None"
                            UseAccessibleHeader="true" ShowHeaderWhenEmpty="true"
                            EmptyDataText="No fee rates are configured for this class.">
                            <Columns>
                                <asp:TemplateField HeaderText="No." ItemStyle-CssClass="fee-row-number">
                                    <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Description" HeaderText="Charge description" />
                                <asp:BoundField DataField="Comments" HeaderText="Notes" NullDisplayText="Not specified" />
                                <asp:BoundField DataField="Rate" HeaderText="Fee rate" DataFormatString="Rs. {0:N2}"
                                    HtmlEncode="false" ItemStyle-CssClass="fee-rate-cell" />
                                <asp:TemplateField HeaderText="Amount received" ItemStyle-CssClass="fee-amount-cell">
                                    <ItemTemplate>
                                        <span class="fee-currency-prefix">Rs.</span>
                                        <asp:TextBox ID="txtAmount" runat="server" Text='<%# Eval("Amount", "{0:0.00}") %>' TextMode="Number"
                                            Enabled='<%# Convert.ToBoolean(Eval("CanEdit")) %>'
                                            CssClass="fee-amount-input" min="0" step="0.01" inputmode="decimal"
                                            aria-label='<%# "Amount received for " + Eval("Description") %>'
                                            oninput="calculateVoucherTotal()" onfocus="this.select();" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>

                    <div class="fee-voucher-footer-grid">
                        <div class="fee-field fee-remarks-field">
                            <asp:Label ID="lblRemarksPrompt" runat="server" AssociatedControlID="txtRemarks"
                                Text="Voucher remarks" CssClass="fee-field-label" />
                            <asp:TextBox ID="txtRemarks" runat="server" CssClass="fee-control" TextMode="MultiLine"
                                Rows="3" MaxLength="250" placeholder="Optional notes for this collection" />
                        </div>

                        <div class="fee-total-panel">
                            <span>Total amount received</span>
                            <strong id="voucherTotalAmount">Rs. 0.00</strong>
                            <small>Calculated automatically from the entered charges.</small>
                        </div>
                    </div>

                    <asp:Panel ID="pnlSavedVoucher" runat="server" Visible="false" CssClass="fee-save-confirmation" role="status">
                        <strong>Fee payment recorded and voucher generated.</strong>
                        <span>Voucher <asp:Label ID="lblSavedVoucherNo" runat="server" /> was recorded for
                            <asp:Label ID="lblSavedTotal" runat="server" />.</span>
                    </asp:Panel>

                    <div class="fee-voucher-actions no-print">
                        <button type="button" class="fee-button fee-button-quiet" onclick="printFeeVoucher();">Print Voucher</button>
                        <asp:Button ID="btnSaveVoucher" runat="server" Text="Save Fee Voucher"
                            CssClass="fee-button fee-button-primary" OnClick="btnSaveVoucher_Click"
                            OnClientClick="this.value='Saving Voucher...';" />
                    </div>
                </section>

                <footer class="fee-voucher-footnote">
                    <span>Government Higher Secondary School Maankot</span>
                    <span>This computer-generated voucher is an official school fee record.</span>
                </footer>
            </article>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        function calculateVoucherTotal() {
            var total = 0;
            var inputs = document.querySelectorAll('#feeVoucherPrintArea .fee-amount-input');
            for (var index = 0; index < inputs.length; index++) {
                var amount = parseFloat(inputs[index].value);
                if (!isNaN(amount) && amount > 0) {
                    total += amount;
                }
            }

            var totalElement = document.getElementById('voucherTotalAmount');
            var hiddenTotal = document.getElementById('hfCalculatedTotal');
            if (totalElement) {
                totalElement.textContent = 'Rs. ' + total.toLocaleString('en-PK', {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                });
            }
            if (hiddenTotal) {
                hiddenTotal.value = total.toFixed(2);
            }
        }

        function printFeeVoucher() {
            calculateVoucherTotal();
            window.print();
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', calculateVoucherTotal);
        } else {
            calculateVoucherTotal();
        }
    </script>
</asp:Content>
