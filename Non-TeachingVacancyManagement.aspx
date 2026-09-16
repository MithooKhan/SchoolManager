<%@ Page Title="Non-Teaching Vacancies" Language="C#" MasterPageFile="~/DSM.Master"
    AutoEventWireup="true" CodeBehind="Non-TeachingVacancyManagement.aspx.cs"
    Inherits="DigitalSchoolManager.WebForm8" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    

    <div class="vacancy-page">

        <div class="vacancy-header">
            <h2>Non-Teaching Vacancy Management</h2>
            <p>Add, update, review and remove non-teaching sanctioned positions.</p>
        </div>

        <asp:HiddenField ID="hfPostId" runat="server" />

        <div class="vacancy-card no-print">
            <div class="vacancy-card-title">
                <asp:Label ID="lblFormTitle" runat="server" Text="Add Vacancy Position"></asp:Label>
            </div>

            <div class="vacancy-card-body">

                <asp:Label ID="lblEditMode" runat="server"
                    CssClass="edit-mode-note"
                    Visible="false"
                    Text="Edit mode is active. Update the required fields and click Update Position.">
                </asp:Label>

                <asp:ValidationSummary ID="vsVacancy" runat="server"
                    ValidationGroup="Vacancy"
                    CssClass="validation-summary"
                    HeaderText="Please correct the following:" />

                <div class="form-grid">

                    <div>
                        <label class="field-label">Description</label>
                        <asp:TextBox ID="txtDescription" runat="server"
                            CssClass="vacancy-input" MaxLength="150" />
                        <asp:RequiredFieldValidator ID="rfvDescription" runat="server"
                            ControlToValidate="txtDescription"
                            ErrorMessage="Description is required."
                            ValidationGroup="Vacancy"
                            Display="None" />
                    </div>

                    <div>
                        <label class="field-label">BPS</label>
                        <asp:TextBox ID="txtBPS" runat="server"
                            TextMode="Number" CssClass="vacancy-input" />
                        <asp:RequiredFieldValidator ID="rfvBPS" runat="server"
                            ControlToValidate="txtBPS"
                            ErrorMessage="BPS is required."
                            ValidationGroup="Vacancy"
                            Display="None" />
                        <asp:RangeValidator ID="rvBPS" runat="server"
                            ControlToValidate="txtBPS"
                            Type="Integer"
                            MinimumValue="1"
                            MaximumValue="22"
                            ErrorMessage="BPS must be between 1 and 22."
                            ValidationGroup="Vacancy"
                            Display="None" />
                    </div>

                    <div>
                        <label class="field-label">Sanctioned Posts</label>
                        <asp:TextBox ID="txtSanctioned" runat="server"
                            TextMode="Number" CssClass="vacancy-input" />
                        <asp:RequiredFieldValidator ID="rfvSanctioned" runat="server"
                            ControlToValidate="txtSanctioned"
                            ErrorMessage="Sanctioned posts are required."
                            ValidationGroup="Vacancy"
                            Display="None" />
                        <asp:RangeValidator ID="rvSanctioned" runat="server"
                            ControlToValidate="txtSanctioned"
                            Type="Integer"
                            MinimumValue="0"
                            MaximumValue="999"
                            ErrorMessage="Sanctioned posts must be between 0 and 999."
                            ValidationGroup="Vacancy"
                            Display="None" />
                    </div>

                    <div>
                        <label class="field-label">Comments</label>
                        <asp:TextBox ID="txtComments" runat="server"
                            CssClass="vacancy-input" MaxLength="250" />
                    </div>

                </div>

                <div class="action-bar">
                    <asp:Button ID="btnSave" runat="server"
                        Text="Save Position"
                        CssClass="btn-vacancy btn-save"
                        ValidationGroup="Vacancy"
                        OnClick="btnSave_Click" />

                    <asp:Button ID="btnClear" runat="server"
                        Text="Clear / Cancel"
                        CssClass="btn-vacancy btn-clear"
                        CausesValidation="false"
                        OnClick="btnClear_Click" />
                </div>
            </div>
        </div>

        <div class="vacancy-card">
            <div class="vacancy-card-title no-print">Vacancy Position Status</div>

            <div class="vacancy-card-body" id="printArea">
                <h3 class="report-title">Non-Teaching Vacancy Position Report</h3>

                <div class="grid-wrap">
                    <asp:GridView ID="gvVacancies" runat="server"
                        AutoGenerateColumns="False"
                        CssClass="vacancy-grid"
                        GridLines="None"
                        EmptyDataText="No vacancy positions are available."
                        OnRowCommand="gvVacancies_RowCommand">

                        <Columns>
                            <asp:BoundField DataField="Description" HeaderText="Description" />
                            <asp:BoundField DataField="BPS" HeaderText="BPS" />
                            <asp:BoundField DataField="Sactioned" HeaderText="Sanctioned" />
                            <asp:BoundField DataField="Working" HeaderText="Working" />
                            <asp:BoundField DataField="Vacant" HeaderText="Vacant" />
                            <asp:BoundField DataField="Comments" HeaderText="Comments" />

                            <asp:TemplateField HeaderText="Actions"
                                HeaderStyle-CssClass="action-column"
                                ItemStyle-CssClass="action-column">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkEdit" runat="server"
                                        Text="Edit"
                                        CssClass="grid-action grid-edit"
                                        CommandName="EditVacancy"
                                        CommandArgument='<%# Eval("postid") %>'
                                        CausesValidation="false" />

                                    <asp:LinkButton ID="lnkDelete" runat="server"
                                        Text="Delete"
                                        CssClass="grid-action grid-delete"
                                        CommandName="DeleteVacancy"
                                        CommandArgument='<%# Eval("postid") %>'
                                        CausesValidation="false"
                                        OnClientClick="return confirm('Are you sure you want to delete this vacancy position?');" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                    </asp:GridView>
                </div>
            </div>

            <div class="table-footer no-print">
                <asp:Button ID="btnPrint" runat="server"
                    Text="Print Vacancy Report"
                    CssClass="btn-vacancy btn-print"
                    CausesValidation="false"
                    OnClientClick="window.print(); return false;" />
            </div>
        </div>
    </div>

</asp:Content>
