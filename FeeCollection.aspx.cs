using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalSchoolManager
{
    public partial class FeeCollection : Page
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString;

        private bool IsCollectionSchemaReady
        {
            get { return ViewState["FeeCollectionSchemaReady"] as bool? ?? false; }
            set { ViewState["FeeCollectionSchemaReady"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            txtDepositDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            lnkFeeRates.Visible = !PortalAuthorizationService.IsTeacher(Context);
            pnlVoucher.Visible = false;
            pnlSavedVoucher.Visible = false;

            try
            {
                EnsureFundsCollectionSchema();
                IsCollectionSchemaReady = true;
            }
            catch (Exception ex)
            {
                IsCollectionSchemaReady = false;
                ShowMessage(
                    "The fee collection database upgrade could not be completed automatically. " +
                    "Run FeeCollection_Database_Update.sql once in SQL Server Management Studio. Details: " + ex.Message,
                    "error");
            }

            try
            {
                LoadFeePeriods();
                LoadClasses();
                ResetStudentList("Select a class or search for a student");
                if (PortalAuthorizationService.IsTeacher(Context) && GetSelectedClassId() > 0)
                    LoadStudents(GetSelectedClassId(), string.Empty);
            }
            catch (Exception ex)
            {
                ShowMessage("The student selection lists could not be loaded. " + ex.Message, "error");
            }
        }

        private void EnsureFundsCollectionSchema()
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.FundsCollection', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FundsCollection
    (
        CollectionID INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_FundsCollection PRIMARY KEY,
        StudentID INT NOT NULL,
        FeeID INT NOT NULL,
        DateOfDeposit DATE NOT NULL
    );
END;

IF COL_LENGTH('dbo.FundsCollection', 'PaidAmount') IS NULL
BEGIN
    ALTER TABLE dbo.FundsCollection
        ADD PaidAmount DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_FundsCollection_PaidAmount DEFAULT (0) WITH VALUES;
END;

IF COL_LENGTH('dbo.FundsCollection', 'VoucherNo') IS NULL
BEGIN
    ALTER TABLE dbo.FundsCollection ADD VoucherNo NVARCHAR(50) NULL;
END;

IF COL_LENGTH('dbo.FundsCollection', 'Remarks') IS NULL
BEGIN
    ALTER TABLE dbo.FundsCollection ADD Remarks NVARCHAR(250) NULL;
END;

IF COL_LENGTH('dbo.FundsCollection', 'CollectedAt') IS NULL
BEGIN
    ALTER TABLE dbo.FundsCollection
        ADD CollectedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_FundsCollection_CollectedAt DEFAULT (SYSDATETIME()) WITH VALUES;
END;

IF COL_LENGTH('dbo.FundsCollection', 'FeeMonth') IS NULL
BEGIN
    ALTER TABLE dbo.FundsCollection ADD FeeMonth TINYINT NULL;
END;

IF COL_LENGTH('dbo.FundsCollection', 'FeeYear') IS NULL
BEGIN
    ALTER TABLE dbo.FundsCollection ADD FeeYear SMALLINT NULL;
END;

IF COL_LENGTH('dbo.FundsCollection', 'CollectedByTeacherID') IS NULL
BEGIN
    ALTER TABLE dbo.FundsCollection ADD CollectedByTeacherID INT NULL;
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FundsCollection')
      AND name = N'IX_FundsCollection_VoucherNo'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_FundsCollection_VoucherNo
        ON dbo.FundsCollection (VoucherNo)
        INCLUDE (StudentID, DateOfDeposit, PaidAmount);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FundsCollection')
      AND name = N'IX_FundsCollection_StudentPeriod'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_FundsCollection_StudentPeriod
        ON dbo.FundsCollection (StudentID, FeeYear, FeeMonth)
        INCLUDE (VoucherNo, FeeID, DateOfDeposit, PaidAmount);
END;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void LoadFeePeriods()
        {
            ddlFeeMonth.Items.Clear();
            for (int month = 1; month <= 12; month++)
            {
                ddlFeeMonth.Items.Add(new ListItem(
                    CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month),
                    month.ToString(CultureInfo.InvariantCulture)));
            }

            int currentYear = DateTime.Today.Year;
            ddlFeeYear.Items.Clear();
            for (int year = currentYear - 2; year <= currentYear + 1; year++)
            {
                ddlFeeYear.Items.Add(new ListItem(
                    year.ToString(CultureInfo.InvariantCulture),
                    year.ToString(CultureInfo.InvariantCulture)));
            }

            ddlFeeMonth.SelectedValue = DateTime.Today.Month.ToString(CultureInfo.InvariantCulture);
            ddlFeeYear.SelectedValue = currentYear.ToString(CultureInfo.InvariantCulture);
        }

        private void LoadClasses()
        {
            const string sql = @"SELECT c.ClassID, c.ClassName
                                 FROM dbo.Classes c
                                 WHERE (@TeacherID=0 OR EXISTS
                                   (SELECT 1 FROM dbo.TeachersClasses tc WHERE tc.ClassID=c.ClassID AND tc.InchargeID=@TeacherID))
                                 ORDER BY ClassName;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@TeacherID", SqlDbType.Int).Value =
                    PortalAuthorizationService.IsTeacher(Context) ? PortalAuthorizationService.TeacherId(Context) : 0;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    ddlClass.Items.Clear();
                    ddlClass.Items.Add(new ListItem(
                        PortalAuthorizationService.IsTeacher(Context) ? "Select assigned class" : "All classes", "0"));
                    while (reader.Read())
                    {
                        ddlClass.Items.Add(new ListItem(
                            Convert.ToString(reader["ClassName"]),
                            Convert.ToString(reader["ClassID"])));
                    }
                    if (PortalAuthorizationService.IsTeacher(Context) && ddlClass.Items.Count == 2)
                        ddlClass.SelectedIndex = 1;
                }
            }
        }

        private void ResetStudentList(string firstItemText)
        {
            ddlStudent.Items.Clear();
            ddlStudent.Items.Add(new ListItem(firstItemText, "0"));
        }

        private int LoadStudents(int classId, string keyword)
        {
            const string sql = @"
SELECT DISTINCT
       s.StudentID,
       ISNULL(s.Name, '') +
       CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(s.Regno, ''))), '') IS NULL
            THEN '' ELSE ' | Reg: ' + s.Regno END +
       CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(s.FormBNo, ''))), '') IS NULL
            THEN '' ELSE ' | Form-B: ' + s.FormBNo END AS StudentDisplay
FROM dbo.Students s
INNER JOIN dbo.StudentClass sc ON sc.StudentID = s.StudentID
WHERE (@ClassID = 0 OR sc.ClassID = @ClassID)
  AND (@TeacherID=0 OR EXISTS(SELECT 1 FROM dbo.TeachersClasses tc WHERE tc.ClassID=sc.ClassID AND tc.InchargeID=@TeacherID))
  AND (@Keyword = ''
       OR s.Name LIKE @SearchPattern
       OR s.Regno LIKE @SearchPattern
       OR s.FormBNo LIKE @SearchPattern)
ORDER BY StudentDisplay;";

            DataTable students = new DataTable();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@ClassID", SqlDbType.Int).Value = classId;
                command.Parameters.Add("@TeacherID", SqlDbType.Int).Value =
                    PortalAuthorizationService.IsTeacher(Context) ? PortalAuthorizationService.TeacherId(Context) : 0;
                command.Parameters.Add("@Keyword", SqlDbType.NVarChar, 100).Value = keyword ?? string.Empty;
                command.Parameters.Add("@SearchPattern", SqlDbType.NVarChar, 104).Value =
                    "%" + (keyword ?? string.Empty) + "%";
                adapter.Fill(students);
            }

            ResetStudentList(students.Rows.Count == 0
                ? "No matching students found"
                : "Select a student");

            foreach (DataRow row in students.Rows)
            {
                ddlStudent.Items.Add(new ListItem(
                    Convert.ToString(row["StudentDisplay"]),
                    Convert.ToString(row["StudentID"])));
            }

            return students.Rows.Count;
        }

        protected void ddlClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearVoucher();
            txtStudentSearch.Text = string.Empty;

            int classId = GetSelectedClassId();
            try { PortalAuthorizationService.RequireTeacherClass(Context, classId); }
            catch (UnauthorizedAccessException ex) { ShowMessage(ex.Message, "error"); return; }
            if (classId <= 0)
            {
                ResetStudentList("Select a class or search for a student");
                HideMessage();
                return;
            }

            try
            {
                int studentCount = LoadStudents(classId, string.Empty);
                if (studentCount == 0)
                {
                    ShowMessage("No students were found in the selected class.", "info");
                }
                else
                {
                    ShowMessage(studentCount + " student record(s) loaded. Select a student to continue.", "info");
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Students could not be loaded. " + ex.Message, "error");
            }
        }

        protected void btnSearchStudent_Click(object sender, EventArgs e)
        {
            ClearVoucher();
            string keyword = (txtStudentSearch.Text ?? string.Empty).Trim();
            int classId = GetSelectedClassId();
            if (PortalAuthorizationService.IsTeacher(Context) && classId <= 0)
            {
                ShowMessage("Select your assigned class before searching for a student.", "info");
                return;
            }
            try { PortalAuthorizationService.RequireTeacherClass(Context, classId); }
            catch (UnauthorizedAccessException ex) { ShowMessage(ex.Message, "error"); return; }

            if (classId == 0 && keyword.Length == 0)
            {
                ShowMessage("Select a class or enter a student name, Registration No, or Form-B No.", "info");
                ResetStudentList("Select a class or search for a student");
                return;
            }

            try
            {
                int studentCount = LoadStudents(classId, keyword);
                if (studentCount == 0)
                {
                    ShowMessage("No student matched the selected class or search details.", "info");
                    return;
                }

                if (studentCount == 1)
                {
                    ddlStudent.SelectedIndex = 1;
                    LoadVoucherForStudent(Convert.ToInt32(ddlStudent.SelectedValue));
                    return;
                }

                ShowMessage(studentCount + " matching students found. Select the correct student from the list.", "info");
            }
            catch (Exception ex)
            {
                ShowMessage("The student search could not be completed. " + ex.Message, "error");
            }
        }

        protected void ddlStudent_SelectedIndexChanged(object sender, EventArgs e)
        {
            int studentId;
            if (!int.TryParse(ddlStudent.SelectedValue, out studentId) || studentId <= 0)
            {
                ClearVoucher();
                return;
            }

            try
            {
                LoadVoucherForStudent(studentId);
            }
            catch (Exception ex)
            {
                ClearVoucher();
                ShowMessage("The fee voucher could not be opened. " + ex.Message, "error");
            }
        }

        protected void ddlFeePeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            int studentId;
            if (int.TryParse(ddlStudent.SelectedValue, out studentId) && studentId > 0)
            {
                try
                {
                    LoadVoucherForStudent(studentId);
                }
                catch (Exception ex)
                {
                    ClearVoucher();
                    ShowMessage("The selected fee period could not be loaded. " + ex.Message, "error");
                }
            }
            else
            {
                ClearVoucher();
            }
        }

        protected void btnClearSelection_Click(object sender, EventArgs e)
        {
            if (ddlClass.Items.FindByValue("0") != null)
            {
                ddlClass.SelectedValue = "0";
            }

            txtStudentSearch.Text = string.Empty;
            ResetStudentList("Select a class or search for a student");
            ClearVoucher();
            HideMessage();
        }

        private void LoadVoucherForStudent(int studentId)
        {
            PortalAuthorizationService.RequireTeacherClass(Context, GetSelectedClassId());
            const string sql = @"
SELECT TOP (1)
       s.StudentID,
       ISNULL(s.Name, '') AS StudentName,
       ISNULL(s.FatherName, '') AS FatherName,
       ISNULL(s.Regno, '') AS Regno,
       ISNULL(s.FormBNo, '') AS FormBNo,
       sc.ClassID,
       ISNULL(CONVERT(NVARCHAR(30), sc.StudentRollNo), '') AS StudentRollNo,
       ISNULL(c.ClassName, '') AS ClassName
FROM dbo.Students s
INNER JOIN dbo.StudentClass sc ON sc.StudentID = s.StudentID
INNER JOIN dbo.Classes c ON c.ClassID = sc.ClassID
WHERE s.StudentID = @StudentID
  AND (@SelectedClassID = 0 OR sc.ClassID = @SelectedClassID)
ORDER BY sc.ClassID DESC;";

            int classId;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;
                command.Parameters.Add("@SelectedClassID", SqlDbType.Int).Value = GetSelectedClassId();
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("The selected student record was not found.");
                    }

                    classId = Convert.ToInt32(reader["ClassID"]);
                    string studentName = Convert.ToString(reader["StudentName"]);

                    hfStudentID.Value = studentId.ToString(CultureInfo.InvariantCulture);
                    hfClassID.Value = classId.ToString(CultureInfo.InvariantCulture);
                    lblStudentName.Text = ValueOrNotAvailable(studentName);
                    lblFatherName.Text = ValueOrNotAvailable(Convert.ToString(reader["FatherName"]));
                    lblRegistrationNo.Text = ValueOrNotAvailable(Convert.ToString(reader["Regno"]));
                    lblFormBNo.Text = ValueOrNotAvailable(Convert.ToString(reader["FormBNo"]));
                    lblClassName.Text = ValueOrNotAvailable(Convert.ToString(reader["ClassName"]));
                    lblRollNo.Text = ValueOrNotAvailable(Convert.ToString(reader["StudentRollNo"]));
                    lblStudentInitials.Text = GetInitials(studentName);
                }
            }

            int feeMonth = GetSelectedFeeMonth();
            int feeYear = GetSelectedFeeYear();
            string periodName = FormatFeePeriod(feeMonth, feeYear);
            lblFeePeriod.Text = periodName;
            lblPaidPeriod.Text = periodName;

            ExistingVoucher existingVoucher = FindExistingVoucher(studentId, feeMonth, feeYear);
            int feeItemCount;
            if (existingVoucher != null)
            {
                feeItemCount = LoadFeeItems(classId, studentId, feeMonth, feeYear, false);
                hfVoucherNo.Value = existingVoucher.VoucherNo;
                lblVoucherNo.Text = existingVoucher.VoucherNo;
                txtDepositDate.Text = existingVoucher.DepositDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                txtDepositDate.Enabled = false;
                txtRemarks.Text = existingVoucher.Remarks;
                txtRemarks.Enabled = false;
                hfCalculatedTotal.Value = existingVoucher.Total.ToString("0.00", CultureInfo.InvariantCulture);
                lblSavedVoucherNo.Text = existingVoucher.VoucherNo;
                lblSavedTotal.Text = "Rs. " + existingVoucher.Total.ToString("N2", CultureInfo.InvariantCulture);
                pnlSavedVoucher.Visible = true;
                pnlAlreadyPaid.Visible = true;
                pnlVoucher.Visible = true;
                btnSaveVoucher.Enabled = false;
                ShowMessage(
                    lblStudentName.Text + " has already deposited the fee for " + periodName +
                    ". The original voucher has been loaded for printing.",
                    "info");
                RegisterTotalCalculationScript();
                return;
            }

            feeItemCount = LoadFeeItems(classId, studentId, feeMonth, feeYear, true);
            string voucherNo = GenerateVoucherNumber(studentId, feeMonth, feeYear);
            hfVoucherNo.Value = voucherNo;
            lblVoucherNo.Text = voucherNo;
            txtDepositDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            txtDepositDate.Enabled = true;
            txtRemarks.Text = string.Empty;
            txtRemarks.Enabled = true;
            hfCalculatedTotal.Value = "0";
            pnlSavedVoucher.Visible = false;
            pnlAlreadyPaid.Visible = false;
            pnlVoucher.Visible = true;
            btnSaveVoucher.Enabled = IsCollectionSchemaReady && feeItemCount > 0;

            if (feeItemCount == 0)
            {
                ShowMessage(
                    "No fee rates are configured for " + lblClassName.Text +
                    ". Use Add Fee Rates before collecting a fee.",
                    "info");
            }
            else if (!IsCollectionSchemaReady)
            {
                ShowMessage(
                    "The voucher is ready, but saving is disabled until FeeCollection_Database_Update.sql has been run.",
                    "error");
            }
            else
            {
                HideMessage();
            }

            RegisterTotalCalculationScript();
        }

        private int LoadFeeItems(int classId, int studentId, int feeMonth, int feeYear, bool canEdit)
        {
            const string sql = @"
SELECT FeeID,
       ISNULL(Description, '') AS Description,
       ISNULL(Comments, '') AS Comments,
       ISNULL(Rate, 0) AS Rate,
       CASE WHEN @CanEdit = 1 THEN CAST(0 AS DECIMAL(18,2))
            ELSE ISNULL((SELECT SUM(fc.PaidAmount)
                         FROM dbo.FundsCollection fc
                         WHERE fc.StudentID = @StudentID
                           AND fc.FeeID = f.FeeID
                           AND fc.FeeMonth = @FeeMonth
                           AND fc.FeeYear = @FeeYear), 0) END AS Amount,
       CAST(@CanEdit AS BIT) AS CanEdit
FROM dbo.FeeSchoolCharges f
WHERE ClassID = @ClassID
ORDER BY Description, FeeID;";

            DataTable charges = new DataTable();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@ClassID", SqlDbType.Int).Value = classId;
                command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;
                command.Parameters.Add("@FeeMonth", SqlDbType.TinyInt).Value = feeMonth;
                command.Parameters.Add("@FeeYear", SqlDbType.SmallInt).Value = feeYear;
                command.Parameters.Add("@CanEdit", SqlDbType.Bit).Value = canEdit;
                adapter.Fill(charges);
            }

            gvFeeItems.DataSource = charges;
            gvFeeItems.DataBind();
            return charges.Rows.Count;
        }

        protected void btnSaveVoucher_Click(object sender, EventArgs e)
        {
            int restrictedClassId;
            int.TryParse(hfClassID.Value, out restrictedClassId);
            try { PortalAuthorizationService.RequireTeacherClass(Context, restrictedClassId); }
            catch (UnauthorizedAccessException ex) { ShowMessage(ex.Message, "error"); return; }
            if (PortalAuthorizationService.IsTeacher(Context))
            {
                int restrictedStudentId;
                int.TryParse(hfStudentID.Value, out restrictedStudentId);
                using (SqlConnection accessConnection = new SqlConnection(_connectionString))
                using (SqlCommand accessCommand = new SqlCommand(
                    "SELECT COUNT(*) FROM dbo.StudentClass WHERE StudentID=@StudentID AND ClassID=@ClassID",
                    accessConnection))
                {
                    accessCommand.Parameters.Add("@StudentID", SqlDbType.Int).Value = restrictedStudentId;
                    accessCommand.Parameters.Add("@ClassID", SqlDbType.Int).Value = restrictedClassId;
                    accessConnection.Open();
                    if (Convert.ToInt32(accessCommand.ExecuteScalar()) != 1)
                    {
                        ShowMessage("The selected student does not belong to your assigned class.", "error");
                        return;
                    }
                }
            }
            int studentId;
            if (!int.TryParse(hfStudentID.Value, out studentId) || studentId <= 0)
            {
                ShowMessage("Select a valid student before saving the fee voucher.", "error");
                return;
            }

            DateTime depositDate;
            if (!DateTime.TryParseExact(
                txtDepositDate.Text,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out depositDate))
            {
                ShowMessage("Enter a valid collection date.", "error");
                return;
            }

            int feeMonth = GetSelectedFeeMonth();
            int feeYear = GetSelectedFeeYear();

            List<FeePayment> payments;
            string validationError;
            if (!TryReadPayments(out payments, out validationError))
            {
                ShowMessage(validationError, "error");
                RegisterTotalCalculationScript();
                return;
            }

            if (payments.Count == 0)
            {
                ShowMessage("Enter an amount greater than zero for at least one fee charge.", "error");
                RegisterTotalCalculationScript();
                return;
            }

            try
            {
                if (!IsCollectionSchemaReady)
                {
                    EnsureFundsCollectionSchema();
                    IsCollectionSchemaReady = true;
                }

                string voucherNo = NormalizeVoucherNumber(hfVoucherNo.Value, studentId, feeMonth, feeYear);
                string remarks = (txtRemarks.Text ?? string.Empty).Trim();
                if (remarks.Length > 250)
                {
                    remarks = remarks.Substring(0, 250);
                }

                SavePayments(studentId, feeMonth, feeYear, depositDate, voucherNo, remarks, payments);

                int classId;
                if (int.TryParse(hfClassID.Value, out classId) && classId > 0)
                {
                    LoadFeeItems(classId, studentId, feeMonth, feeYear, false);
                }

                decimal total = 0m;
                foreach (FeePayment payment in payments)
                {
                    total += payment.Amount;
                }

                hfVoucherNo.Value = voucherNo;
                lblVoucherNo.Text = voucherNo;
                lblSavedVoucherNo.Text = voucherNo;
                lblSavedTotal.Text = "Rs. " + total.ToString("N2", CultureInfo.InvariantCulture);
                hfCalculatedTotal.Value = total.ToString("0.00", CultureInfo.InvariantCulture);
                pnlSavedVoucher.Visible = true;
                pnlAlreadyPaid.Visible = true;
                pnlVoucher.Visible = true;
                btnSaveVoucher.Enabled = false;
                txtDepositDate.Enabled = false;
                txtRemarks.Enabled = false;
                ShowMessage(
                    "Fee collection saved successfully for " + FormatFeePeriod(feeMonth, feeYear) +
                    ". The student voucher has been generated and is ready to print.",
                    "success");
                RegisterTotalCalculationScript();
            }
            catch (DuplicateNameException)
            {
                pnlVoucher.Visible = true;
                btnSaveVoucher.Enabled = false;
                ShowMessage(
                    "This student has already deposited the fee for " + FormatFeePeriod(feeMonth, feeYear) +
                    ". Reload the student to print the saved voucher.",
                    "info");
                RegisterTotalCalculationScript();
            }
            catch (Exception ex)
            {
                pnlVoucher.Visible = true;
                ShowMessage("The fee voucher could not be saved. " + ex.Message, "error");
                RegisterTotalCalculationScript();
            }
        }

        private bool TryReadPayments(out List<FeePayment> payments, out string error)
        {
            payments = new List<FeePayment>();
            error = string.Empty;

            foreach (GridViewRow row in gvFeeItems.Rows)
            {
                if (row.RowType != DataControlRowType.DataRow)
                {
                    continue;
                }

                int feeId = Convert.ToInt32(gvFeeItems.DataKeys[row.RowIndex].Value);
                TextBox amountTextBox = row.FindControl("txtAmount") as TextBox;
                string rawAmount = amountTextBox == null ? "0" : amountTextBox.Text.Trim();

                decimal amount;
                if (!TryParseAmount(rawAmount, out amount))
                {
                    error = "Enter a valid numeric amount for fee row " + (row.RowIndex + 1) + ".";
                    return false;
                }

                if (amount < 0m)
                {
                    error = "Fee amounts cannot be negative.";
                    return false;
                }

                if (amount > 99999999.99m)
                {
                    error = "A fee amount cannot be greater than Rs. 99,999,999.99.";
                    return false;
                }

                if (amount > 0m)
                {
                    payments.Add(new FeePayment { FeeId = feeId, Amount = decimal.Round(amount, 2) });
                }
            }

            return true;
        }

        private static bool TryParseAmount(string text, out decimal amount)
        {
            return decimal.TryParse(
                       text,
                       NumberStyles.Number,
                       CultureInfo.InvariantCulture,
                       out amount)
                   || decimal.TryParse(
                       text,
                       NumberStyles.Number,
                       CultureInfo.CurrentCulture,
                       out amount);
        }

        private void SavePayments(
            int studentId,
            int feeMonth,
            int feeYear,
            DateTime depositDate,
            string voucherNo,
            string remarks,
            IEnumerable<FeePayment> payments)
        {
            const string duplicateSql = @"
SELECT COUNT(1)
FROM dbo.FundsCollection WITH (UPDLOCK, HOLDLOCK)
WHERE VoucherNo = @VoucherNo
   OR (StudentID = @StudentID AND FeeMonth = @FeeMonth AND FeeYear = @FeeYear);";

            const string insertSql = @"
INSERT INTO dbo.FundsCollection
       (StudentID, FeeID, DateOfDeposit, PaidAmount, VoucherNo, Remarks,
        CollectedAt, FeeMonth, FeeYear, CollectedByTeacherID)
VALUES (@StudentID, @FeeID, @DateOfDeposit, @PaidAmount, @VoucherNo, @Remarks,
        SYSDATETIME(), @FeeMonth, @FeeYear, @CollectedByTeacherID);";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (SqlCommand duplicateCommand = new SqlCommand(duplicateSql, connection, transaction))
                        {
                            duplicateCommand.Parameters.Add("@VoucherNo", SqlDbType.NVarChar, 50).Value = voucherNo;
                            duplicateCommand.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;
                            duplicateCommand.Parameters.Add("@FeeMonth", SqlDbType.TinyInt).Value = feeMonth;
                            duplicateCommand.Parameters.Add("@FeeYear", SqlDbType.SmallInt).Value = feeYear;
                            if (Convert.ToInt32(duplicateCommand.ExecuteScalar()) > 0)
                            {
                                throw new DuplicateNameException("Voucher already exists.");
                            }
                        }

                        foreach (FeePayment payment in payments)
                        {
                            using (SqlCommand insertCommand = new SqlCommand(insertSql, connection, transaction))
                            {
                                insertCommand.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;
                                insertCommand.Parameters.Add("@FeeID", SqlDbType.Int).Value = payment.FeeId;
                                insertCommand.Parameters.Add("@FeeMonth", SqlDbType.TinyInt).Value = feeMonth;
                                insertCommand.Parameters.Add("@FeeYear", SqlDbType.SmallInt).Value = feeYear;
                                insertCommand.Parameters.Add("@DateOfDeposit", SqlDbType.Date).Value = depositDate.Date;

                                SqlParameter amountParameter = insertCommand.Parameters.Add(
                                    "@PaidAmount", SqlDbType.Decimal);
                                amountParameter.Precision = 18;
                                amountParameter.Scale = 2;
                                amountParameter.Value = payment.Amount;

                                insertCommand.Parameters.Add("@VoucherNo", SqlDbType.NVarChar, 50).Value = voucherNo;
                                insertCommand.Parameters.Add("@Remarks", SqlDbType.NVarChar, 250).Value =
                                    string.IsNullOrWhiteSpace(remarks) ? (object)DBNull.Value : remarks;
                                insertCommand.Parameters.Add("@CollectedByTeacherID", SqlDbType.Int).Value =
                                    PortalAuthorizationService.IsTeacher(Context)
                                        ? (object)PortalAuthorizationService.TeacherId(Context)
                                        : DBNull.Value;
                                insertCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private int GetSelectedClassId()
        {
            int classId;
            return int.TryParse(ddlClass.SelectedValue, out classId) ? classId : 0;
        }

        private void ClearVoucher()
        {
            pnlVoucher.Visible = false;
            pnlSavedVoucher.Visible = false;
            pnlAlreadyPaid.Visible = false;
            hfStudentID.Value = string.Empty;
            hfClassID.Value = string.Empty;
            hfVoucherNo.Value = string.Empty;
            hfCalculatedTotal.Value = "0";
            gvFeeItems.DataSource = null;
            gvFeeItems.DataBind();
        }

        private void ShowMessage(string message, string type)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = "fee-message fee-message-" + type;
            lblMessage.Text = message;
        }

        private void HideMessage()
        {
            pnlMessage.Visible = false;
            lblMessage.Text = string.Empty;
        }

        private void RegisterTotalCalculationScript()
        {
            ClientScript.RegisterStartupScript(
                GetType(),
                "calculateVoucherTotal",
                "window.setTimeout(calculateVoucherTotal, 0);",
                true);
        }

        private int GetSelectedFeeMonth()
        {
            int month;
            return int.TryParse(ddlFeeMonth.SelectedValue, out month) && month >= 1 && month <= 12
                ? month
                : DateTime.Today.Month;
        }

        private int GetSelectedFeeYear()
        {
            int year;
            return int.TryParse(ddlFeeYear.SelectedValue, out year) && year >= 2000 && year <= 2100
                ? year
                : DateTime.Today.Year;
        }

        private static string FormatFeePeriod(int month, int year)
        {
            return CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month) + " " +
                   year.ToString(CultureInfo.InvariantCulture);
        }

        private ExistingVoucher FindExistingVoucher(int studentId, int feeMonth, int feeYear)
        {
            const string sql = @"
SELECT TOP (1)
       ISNULL(VoucherNo, '') AS VoucherNo,
       MIN(DateOfDeposit) AS DateOfDeposit,
       SUM(PaidAmount) AS Total,
       MAX(ISNULL(Remarks, '')) AS Remarks
FROM dbo.FundsCollection
WHERE StudentID = @StudentID
  AND FeeMonth = @FeeMonth
  AND FeeYear = @FeeYear
  AND PaidAmount > 0
GROUP BY VoucherNo
ORDER BY MIN(CollectedAt) DESC;";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;
                command.Parameters.Add("@FeeMonth", SqlDbType.TinyInt).Value = feeMonth;
                command.Parameters.Add("@FeeYear", SqlDbType.SmallInt).Value = feeYear;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new ExistingVoucher
                    {
                        VoucherNo = Convert.ToString(reader["VoucherNo"]),
                        DepositDate = Convert.ToDateTime(reader["DateOfDeposit"], CultureInfo.InvariantCulture),
                        Total = Convert.ToDecimal(reader["Total"], CultureInfo.InvariantCulture),
                        Remarks = Convert.ToString(reader["Remarks"])
                    };
                }
            }
        }

        private static string GenerateVoucherNumber(int studentId, int feeMonth, int feeYear)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "FV-{0:D4}{1:D2}-{2}-{3}",
                feeYear,
                feeMonth,
                studentId,
                Guid.NewGuid().ToString("N").Substring(0, 4).ToUpperInvariant());
        }

        private static string NormalizeVoucherNumber(string voucherNo, int studentId, int feeMonth, int feeYear)
        {
            string normalized = (voucherNo ?? string.Empty).Trim();
            if (normalized.Length == 0 || normalized.Length > 50 ||
                !normalized.StartsWith("FV-", StringComparison.Ordinal))
            {
                return GenerateVoucherNumber(studentId, feeMonth, feeYear);
            }

            return normalized;
        }

        private static string GetInitials(string name)
        {
            string[] parts = (name ?? string.Empty)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return "ST";
            }

            if (parts.Length == 1)
            {
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
            }

            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1))
                .ToUpperInvariant();
        }

        private static string ValueOrNotAvailable(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Not available" : value.Trim();
        }

        private sealed class FeePayment
        {
            public int FeeId { get; set; }
            public decimal Amount { get; set; }
        }

        private sealed class ExistingVoucher
        {
            public string VoucherNo { get; set; }
            public DateTime DepositDate { get; set; }
            public decimal Total { get; set; }
            public string Remarks { get; set; }
        }
    }
}
