using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web;

namespace DigitalSchoolManager
{
    internal static class BudgetManagementService
    {
        private static string ConnectionString
        {
            get
            {
                ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["SchoolDB"];
                if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                    throw new ConfigurationErrorsException("The SchoolDB connection string is missing from Web.config.");
                return setting.ConnectionString;
            }
        }

        internal static void EnsureSchema()
        {
            // Budget staffing must always use the current active-service status,
            // even when this module is opened directly instead of via Dashboard.
            SchoolLifecycleService.EnsureSchema();

            string path = HttpContext.Current == null
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BudgetManagement_Database_Update.sql")
                : HttpContext.Current.Server.MapPath("~/BudgetManagement_Database_Update.sql");
            if (!File.Exists(path))
                throw new FileNotFoundException("BudgetManagement_Database_Update.sql was not found.", path);

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(File.ReadAllText(path), connection))
            {
                command.CommandTimeout = 120;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        internal static DataTable GetAllowances(bool activeOnly)
        {
            string sql = @"SELECT AllowanceID, AllowanceCode, AllowanceName, SortOrder, IsActive
FROM dbo.BudgetAllowances
WHERE @ActiveOnly=0 OR IsActive=1
ORDER BY SortOrder, AllowanceCode;";
            return Fill(sql, new SqlParameter("@ActiveOnly", SqlDbType.Bit) { Value = activeOnly });
        }

        internal static void SaveAllowance(int? allowanceId, string code, string name, int sortOrder, bool active)
        {
            code = Required(code, "allowance code", 25).ToUpperInvariant();
            name = Required(name, "allowance name", 160);
            string sql = allowanceId.HasValue
                ? @"UPDATE dbo.BudgetAllowances SET AllowanceCode=@Code, AllowanceName=@Name, SortOrder=@SortOrder, IsActive=@IsActive WHERE AllowanceID=@ID;"
                : @"INSERT dbo.BudgetAllowances(AllowanceCode,AllowanceName,SortOrder,IsActive) VALUES(@Code,@Name,@SortOrder,@IsActive);";
            var parameters = new List<SqlParameter>
            {
                Text("@Code", 25, code), Text("@Name", 160, name),
                new SqlParameter("@SortOrder", SqlDbType.Int) { Value = Math.Max(0, sortOrder) },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = active }
            };
            if (allowanceId.HasValue) parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = allowanceId.Value });
            Execute(sql, parameters.ToArray());
        }

        internal static DataTable GetPayScales()
        {
            return Fill("SELECT BPS,MinimumPay,MaximumPay,AnnualIncrement FROM dbo.BudgetPayScales ORDER BY BPS;");
        }

        internal static void SavePayScale(int bps, decimal minimum, decimal maximum, decimal increment)
        {
            if (bps < 1 || bps > 22 || minimum < 0 || maximum < minimum || increment < 0)
                throw new InvalidOperationException("Enter a valid BPS and pay-scale amounts.");
            Execute(@"UPDATE dbo.BudgetPayScales SET MinimumPay=@Minimum,MaximumPay=@Maximum,AnnualIncrement=@Increment,UpdatedAtUtc=SYSUTCDATETIME() WHERE BPS=@BPS;
IF @@ROWCOUNT=0 INSERT dbo.BudgetPayScales(BPS,MinimumPay,MaximumPay,AnnualIncrement) VALUES(@BPS,@Minimum,@Maximum,@Increment);",
                Int("@BPS", bps), Money("@Minimum", minimum), Money("@Maximum", maximum), Money("@Increment", increment));
        }

        internal static DataTable GetSourcePosts()
        {
            const string sql = @"
SELECT N'Teaching' StaffCategory, p.PostID SourcePostID, p.Description PostName, p.BPS,
       ISNULL(p.Sactioned,0) Sanctioned, ISNULL(p.Working,0) Working, ISNULL(p.Vacant,0) Vacant,
       m.BudgetPostID, ISNULL(m.PostCode,N'') PostCode
FROM dbo.TeachingVacancyPosition p
LEFT JOIN dbo.BudgetPostCodes m ON m.StaffCategory=N'Teaching' AND m.SourcePostID=p.PostID
UNION ALL
SELECT N'Non-Teaching', p.PostID, p.Description, p.BPS,
       ISNULL(p.Sactioned,0), ISNULL(p.Working,0), ISNULL(p.Vacant,0),
       m.BudgetPostID, ISNULL(m.PostCode,N'')
FROM dbo.Non_TeachingVacancyPosition p
LEFT JOIN dbo.BudgetPostCodes m ON m.StaffCategory=N'Non-Teaching' AND m.SourcePostID=p.PostID
ORDER BY StaffCategory DESC, BPS DESC, PostName;";
            return Fill(sql);
        }

        internal static void SavePostCode(string category, int sourcePostId, string postCode, string postName, int bps)
        {
            if (category != "Teaching" && category != "Non-Teaching")
                throw new InvalidOperationException("Invalid staff category.");
            postCode = Required(postCode, "post code", 30).ToUpperInvariant();
            postName = Required(postName, "post name", 160);
            const string sql = @"
UPDATE dbo.BudgetPostCodes SET PostCode=@PostCode,PostName=@PostName,BPS=@BPS,IsActive=1,UpdatedAtUtc=SYSUTCDATETIME()
WHERE StaffCategory=@Category AND SourcePostID=@SourcePostID;
IF @@ROWCOUNT=0 INSERT dbo.BudgetPostCodes(StaffCategory,SourcePostID,PostCode,PostName,BPS)
VALUES(@Category,@SourcePostID,@PostCode,@PostName,@BPS);";
            Execute(sql, Text("@Category", 20, category), Int("@SourcePostID", sourcePostId), Text("@PostCode", 30, postCode), Text("@PostName", 160, postName), Int("@BPS", bps));
        }

        internal static int CreateBudget(BudgetHeader entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            if (entry.FiscalStartYear < 2000 || entry.FiscalStartYear > 2200)
                throw new InvalidOperationException("Enter a valid fiscal start year.");
            entry.FiscalEndYear = entry.FiscalStartYear + 1;
            entry.BudgetName = Required(entry.BudgetName, "budget name", 120);
            const string sql = @"
INSERT dbo.AnnualBudgets(BudgetName,FiscalStartYear,FiscalEndYear,SchoolName,LocalGovernmentName,DemandName,GrantNo,DetailedFunctionCode,DetailedFunctionName,CostCenterCode,CreatedByUserID)
VALUES(@Name,@Start,@End,@School,@LocalGovernment,@Demand,@Grant,@FunctionCode,@FunctionName,@CostCenter,@UserID);
DECLARE @BudgetID INT=CAST(SCOPE_IDENTITY() AS INT);
INSERT dbo.BudgetObjectEntries(BudgetID,ObjectCode,ObjectName,SortOrder) VALUES
(@BudgetID,N'A03303',N'Electricity Charges',10),(@BudgetID,N'A03805',N'Travelling Allowance',20),
(@BudgetID,N'A04114',N'Leave Encashment',30),(@BudgetID,N'A06103',N'Cash Rewards',40);
SELECT @BudgetID;";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddRange(new[] {
                    Text("@Name",120,entry.BudgetName), Int("@Start",entry.FiscalStartYear), Int("@End",entry.FiscalEndYear),
                    Text("@School",250,Required(entry.SchoolName,"school name",250)),
                    Text("@LocalGovernment",200,Required(entry.LocalGovernmentName,"local government name",200)),
                    Text("@Demand",200,Required(entry.DemandName,"demand name",200)),
                    Text("@Grant",30,Required(entry.GrantNo,"grant number",30)),
                    Text("@FunctionCode",30,Required(entry.DetailedFunctionCode,"detailed function code",30)),
                    Text("@FunctionName",200,Required(entry.DetailedFunctionName,"detailed function name",200)),
                    Text("@CostCenter",30,Required(entry.CostCenterCode,"cost center code",30)),
                    new SqlParameter("@UserID",SqlDbType.Int) { Value=entry.CreatedByUserID.HasValue ? (object)entry.CreatedByUserID.Value : DBNull.Value }
                });
                connection.Open();
                int budgetId = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                connection.Close();
                try
                {
                    SyncBudgetStaff(budgetId);
                    return budgetId;
                }
                catch
                {
                    Execute("DELETE FROM dbo.AnnualBudgets WHERE BudgetID=@ID;", Int("@ID", budgetId));
                    throw;
                }
            }
        }

        internal static DataTable GetBudgets(string search)
        {
            return Fill(@"SELECT BudgetID,BudgetName,FiscalStartYear,FiscalEndYear,Status,CostCenterCode,UpdatedAtUtc
FROM dbo.AnnualBudgets WHERE @Search=N'' OR BudgetName LIKE N'%'+@Search+N'%' ORDER BY FiscalStartYear DESC,BudgetName;",
                Text("@Search", 120, (search ?? string.Empty).Trim()));
        }

        internal static DataRow GetBudget(int budgetId)
        {
            DataTable table = Fill("SELECT * FROM dbo.AnnualBudgets WHERE BudgetID=@ID;", Int("@ID", budgetId));
            if (table.Rows.Count == 0) throw new InvalidOperationException("The selected budget was not found.");
            return table.Rows[0];
        }

        internal static void SyncBudgetStaff(int budgetId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    EnsurePostCodes(connection, transaction);
                    const string staffSql = @"
INSERT dbo.BudgetStaffLines(BudgetID,BudgetPostID,StaffCategory,SourceStaffID,EmployeeName,Gender,Designation,PostCode,BPS,IsVacant,MinimumPay,MaximumPay,IncrementRate)
SELECT @BudgetID,m.BudgetPostID,N'Teaching',t.teacherid,ISNULL(NULLIF(LTRIM(RTRIM(t.Name)),N''),N'Teacher #'+CONVERT(NVARCHAR(12),t.teacherid)),
       CASE WHEN UPPER(ISNULL(t.Gender,N'')) LIKE N'F%' THEN N'Female' ELSE N'Male' END,
       p.Description,m.PostCode,p.BPS,0,ISNULL(s.MinimumPay,0),ISNULL(s.MaximumPay,0),ISNULL(s.AnnualIncrement,0)
FROM dbo.Teachers t INNER JOIN dbo.TeachingVacancyPosition p ON p.PostID=t.postID
INNER JOIN dbo.BudgetPostCodes m ON m.StaffCategory=N'Teaching' AND m.SourcePostID=p.PostID
LEFT JOIN dbo.BudgetPayScales s ON s.BPS=p.BPS
WHERE ISNULL(t.IsActive,1)=1 AND NOT EXISTS(SELECT 1 FROM dbo.BudgetStaffLines x WHERE x.BudgetID=@BudgetID AND x.StaffCategory=N'Teaching' AND x.SourceStaffID=t.teacherid);

INSERT dbo.BudgetStaffLines(BudgetID,BudgetPostID,StaffCategory,SourceStaffID,EmployeeName,Gender,Designation,PostCode,BPS,IsVacant,MinimumPay,MaximumPay,IncrementRate)
SELECT @BudgetID,m.BudgetPostID,N'Non-Teaching',t.StaffID,ISNULL(NULLIF(LTRIM(RTRIM(t.Name)),N''),N'Staff #'+CONVERT(NVARCHAR(12),t.StaffID)),
       CASE WHEN UPPER(ISNULL(t.Gender,N'')) LIKE N'F%' THEN N'Female' ELSE N'Male' END,
       p.Description,m.PostCode,p.BPS,0,ISNULL(s.MinimumPay,0),ISNULL(s.MaximumPay,0),ISNULL(s.AnnualIncrement,0)
FROM dbo.NonTeachingStaff t INNER JOIN dbo.Non_TeachingVacancyPosition p ON p.PostID=t.PostID
INNER JOIN dbo.BudgetPostCodes m ON m.StaffCategory=N'Non-Teaching' AND m.SourcePostID=p.PostID
LEFT JOIN dbo.BudgetPayScales s ON s.BPS=p.BPS
WHERE ISNULL(t.IsActive,1)=1 AND NOT EXISTS(SELECT 1 FROM dbo.BudgetStaffLines x WHERE x.BudgetID=@BudgetID AND x.StaffCategory=N'Non-Teaching' AND x.SourceStaffID=t.StaffID);";
                    using (SqlCommand command = new SqlCommand(staffSql, connection, transaction))
                    {
                        command.Parameters.Add("@BudgetID", SqlDbType.Int).Value = budgetId;
                        command.ExecuteNonQuery();
                    }

                    AddVacancyLines(connection, transaction, budgetId, "Teaching", "TeachingVacancyPosition");
                    AddVacancyLines(connection, transaction, budgetId, "Non-Teaching", "Non_TeachingVacancyPosition");
                    using (SqlCommand allowanceSnapshot = new SqlCommand(@"
INSERT dbo.BudgetStaffAllowances(BudgetStaffLineID,AllowanceID,AllowanceCodeSnapshot,AllowanceNameSnapshot,SortOrderSnapshot,MonthlyAmount)
SELECT l.BudgetStaffLineID,a.AllowanceID,a.AllowanceCode,a.AllowanceName,a.SortOrder,0
FROM dbo.BudgetStaffLines l CROSS JOIN dbo.BudgetAllowances a
WHERE l.BudgetID=@ID AND a.IsActive=1
AND NOT EXISTS(SELECT 1 FROM dbo.BudgetStaffAllowances x WHERE x.BudgetStaffLineID=l.BudgetStaffLineID AND x.AllowanceID=a.AllowanceID);", connection, transaction))
                    {
                        allowanceSnapshot.Parameters.Add("@ID", SqlDbType.Int).Value = budgetId;
                        allowanceSnapshot.ExecuteNonQuery();
                    }
                    using (SqlCommand touch = new SqlCommand("UPDATE dbo.AnnualBudgets SET UpdatedAtUtc=SYSUTCDATETIME() WHERE BudgetID=@ID;", connection, transaction))
                    {
                        touch.Parameters.Add("@ID", SqlDbType.Int).Value = budgetId;
                        touch.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }

        private static void EnsurePostCodes(SqlConnection connection, SqlTransaction transaction)
        {
            const string sql = @"
INSERT dbo.BudgetPostCodes(StaffCategory,SourcePostID,PostCode,PostName,BPS)
SELECT N'Teaching',p.PostID,N'T-'+RIGHT(N'000'+CONVERT(NVARCHAR(10),p.PostID),3),p.Description,p.BPS
FROM dbo.TeachingVacancyPosition p WHERE NOT EXISTS(SELECT 1 FROM dbo.BudgetPostCodes x WHERE x.StaffCategory=N'Teaching' AND x.SourcePostID=p.PostID);
INSERT dbo.BudgetPostCodes(StaffCategory,SourcePostID,PostCode,PostName,BPS)
SELECT N'Non-Teaching',p.PostID,N'NT-'+RIGHT(N'000'+CONVERT(NVARCHAR(10),p.PostID),3),p.Description,p.BPS
FROM dbo.Non_TeachingVacancyPosition p WHERE NOT EXISTS(SELECT 1 FROM dbo.BudgetPostCodes x WHERE x.StaffCategory=N'Non-Teaching' AND x.SourcePostID=p.PostID);";
            using (SqlCommand command = new SqlCommand(sql, connection, transaction)) command.ExecuteNonQuery();
        }

        private static void AddVacancyLines(SqlConnection connection, SqlTransaction transaction, int budgetId, string category, string tableName)
        {
            string sql = @"SELECT p.PostID,ISNULL(p.Vacant,0),p.Description,p.BPS,m.BudgetPostID,m.PostCode,ISNULL(s.AnnualIncrement,0),ISNULL(s.MinimumPay,0),ISNULL(s.MaximumPay,0)
FROM dbo." + tableName + @" p INNER JOIN dbo.BudgetPostCodes m ON m.StaffCategory=@Category AND m.SourcePostID=p.PostID
LEFT JOIN dbo.BudgetPayScales s ON s.BPS=p.BPS WHERE ISNULL(p.Vacant,0)>0;";
            using (SqlCommand command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@Category", SqlDbType.NVarChar, 20).Value = category;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var posts = new List<object[]>();
                    while (reader.Read())
                        posts.Add(new object[] { reader.GetInt32(0), Convert.ToInt32(reader[1]), Convert.ToString(reader[2]), Convert.ToInt32(reader[3]), Convert.ToInt32(reader[4]), Convert.ToString(reader[5]), Convert.ToDecimal(reader[6]), Convert.ToDecimal(reader[7]), Convert.ToDecimal(reader[8]) });
                    reader.Close();
                    foreach (object[] post in posts)
                    {
                        int count = Convert.ToInt32(post[1]);
                        for (int sequence = 1; sequence <= count; sequence++)
                        {
                            const string insert = @"IF NOT EXISTS(SELECT 1 FROM dbo.BudgetStaffLines WHERE BudgetID=@BudgetID AND BudgetPostID=@BudgetPostID AND IsVacant=1 AND VacancySequence=@Sequence)
INSERT dbo.BudgetStaffLines(BudgetID,BudgetPostID,StaffCategory,VacancySequence,EmployeeName,Gender,Designation,PostCode,BPS,IsVacant,MinimumPay,MaximumPay,IncrementRate)
VALUES(@BudgetID,@BudgetPostID,@Category,@Sequence,N'Vacant',N'Male',@Designation,@PostCode,@BPS,1,@Minimum,@Maximum,@Increment);";
                            using (SqlCommand add = new SqlCommand(insert, connection, transaction))
                            {
                                add.Parameters.Add("@BudgetID", SqlDbType.Int).Value = budgetId;
                                add.Parameters.Add("@BudgetPostID", SqlDbType.Int).Value = Convert.ToInt32(post[4]);
                                add.Parameters.Add("@Category", SqlDbType.NVarChar, 20).Value = category;
                                add.Parameters.Add("@Sequence", SqlDbType.Int).Value = sequence;
                                add.Parameters.Add("@Designation", SqlDbType.NVarChar, 160).Value = post[2];
                                add.Parameters.Add("@PostCode", SqlDbType.NVarChar, 30).Value = post[5];
                                add.Parameters.Add("@BPS", SqlDbType.Int).Value = post[3];
                                add.Parameters.Add("@Minimum", SqlDbType.Decimal).Value = post[7];
                                add.Parameters.Add("@Maximum", SqlDbType.Decimal).Value = post[8];
                                add.Parameters.Add("@Increment", SqlDbType.Decimal).Value = post[6];
                                add.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        internal static DataTable GetBudgetStaff(int budgetId)
        {
            const string sql = @"SELECT l.BudgetStaffLineID,l.EmployeeName,l.StaffCategory,l.Designation,l.PostCode,l.BPS,l.Gender,l.IsVacant,
l.BasicPay,l.IncrementDate,l.IncrementRate,l.RecruitmentPlanned,l.Remarks,l.MinimumPay,l.MaximumPay
FROM dbo.BudgetStaffLines l
WHERE l.BudgetID=@ID ORDER BY l.BPS DESC,l.Designation,l.IsVacant,l.EmployeeName;";
            return Fill(sql, Int("@ID", budgetId));
        }

        internal static void SaveStaffLine(int lineId, decimal basicPay, DateTime? incrementDate, decimal incrementRate, bool recruitmentPlanned, string remarks)
        {
            if (basicPay < 0 || incrementRate < 0) throw new InvalidOperationException("Pay figures cannot be negative.");
            Execute(@"UPDATE dbo.BudgetStaffLines SET BasicPay=@BasicPay,IncrementDate=@IncrementDate,IncrementRate=@IncrementRate,
RecruitmentPlanned=@Recruitment,Remarks=@Remarks,UpdatedAtUtc=SYSUTCDATETIME() WHERE BudgetStaffLineID=@ID;",
                Money("@BasicPay", basicPay), new SqlParameter("@IncrementDate", SqlDbType.Date) { Value = incrementDate.HasValue ? (object)incrementDate.Value.Date : DBNull.Value },
                Money("@IncrementRate", incrementRate), new SqlParameter("@Recruitment", SqlDbType.Bit) { Value = recruitmentPlanned },
                NullableText("@Remarks", 300, remarks), Int("@ID", lineId));
        }

        internal static DataTable GetStaffAllowances(int staffLineId)
        {
            Execute(@"INSERT dbo.BudgetStaffAllowances(BudgetStaffLineID,AllowanceID,AllowanceCodeSnapshot,AllowanceNameSnapshot,SortOrderSnapshot,MonthlyAmount)
SELECT @LineID,a.AllowanceID,a.AllowanceCode,a.AllowanceName,a.SortOrder,0 FROM dbo.BudgetAllowances a WHERE a.IsActive=1
AND NOT EXISTS(SELECT 1 FROM dbo.BudgetStaffAllowances x WHERE x.BudgetStaffLineID=@LineID AND x.AllowanceID=a.AllowanceID);", Int("@LineID", staffLineId));
            return Fill(@"SELECT x.AllowanceID,x.AllowanceCodeSnapshot AllowanceCode,x.AllowanceNameSnapshot AllowanceName,x.MonthlyAmount
FROM dbo.BudgetStaffAllowances x WHERE x.BudgetStaffLineID=@LineID
ORDER BY x.SortOrderSnapshot,x.AllowanceCodeSnapshot;", Int("@LineID", staffLineId));
        }

        internal static void SaveStaffAllowance(int staffLineId, int allowanceId, decimal monthlyAmount)
        {
            if (monthlyAmount < 0) throw new InvalidOperationException("Allowance amounts cannot be negative.");
            Execute(@"UPDATE dbo.BudgetStaffAllowances SET MonthlyAmount=@Amount,UpdatedAtUtc=SYSUTCDATETIME() WHERE BudgetStaffLineID=@LineID AND AllowanceID=@AllowanceID;
IF @@ROWCOUNT=0 INSERT dbo.BudgetStaffAllowances(BudgetStaffLineID,AllowanceID,AllowanceCodeSnapshot,AllowanceNameSnapshot,SortOrderSnapshot,MonthlyAmount)
SELECT @LineID,a.AllowanceID,a.AllowanceCode,a.AllowanceName,a.SortOrder,@Amount FROM dbo.BudgetAllowances a WHERE a.AllowanceID=@AllowanceID;",
                Int("@LineID", staffLineId), Int("@AllowanceID", allowanceId), Money("@Amount", monthlyAmount));
        }

        internal static DataTable GetObjectEntries(int budgetId)
        {
            return Fill("SELECT BudgetObjectEntryID,ObjectCode,ObjectName,PreviousBudget,CurrentRevised,ProposedBudget FROM dbo.BudgetObjectEntries WHERE BudgetID=@ID ORDER BY SortOrder,ObjectCode;", Int("@ID", budgetId));
        }

        internal static void SaveObjectEntry(int entryId, decimal previous, decimal revised, decimal proposed)
        {
            if (previous < 0 || revised < 0 || proposed < 0) throw new InvalidOperationException("Budget figures cannot be negative.");
            Execute(@"UPDATE dbo.BudgetObjectEntries SET PreviousBudget=@Previous,CurrentRevised=@Revised,ProposedBudget=@Proposed,UpdatedAtUtc=SYSUTCDATETIME() WHERE BudgetObjectEntryID=@ID;",
                Money("@Previous", previous), Money("@Revised", revised), Money("@Proposed", proposed), Int("@ID", entryId));
        }

        internal static void FinalizeBudget(int budgetId, bool finalized)
        {
            Execute("UPDATE dbo.AnnualBudgets SET Status=@Status,UpdatedAtUtc=SYSUTCDATETIME() WHERE BudgetID=@ID;",
                Text("@Status", 20, finalized ? "Finalized" : "Draft"), Int("@ID", budgetId));
        }

        internal static BudgetTotals GetTotals(int budgetId)
        {
            const string sql = @"
SELECT ISNULL(SUM(BasicPay*12 + CASE WHEN IncrementDate IS NULL THEN 0 ELSE IncrementRate *
CASE WHEN MONTH(IncrementDate) BETWEEN 7 AND 12 THEN 19-MONTH(IncrementDate) ELSE 7-MONTH(IncrementDate) END END),0)
FROM dbo.BudgetStaffLines WHERE BudgetID=@ID;
SELECT ISNULL(SUM(a.MonthlyAmount*12),0) FROM dbo.BudgetStaffAllowances a INNER JOIN dbo.BudgetStaffLines l ON l.BudgetStaffLineID=a.BudgetStaffLineID WHERE l.BudgetID=@ID;
SELECT ISNULL(SUM(ProposedBudget),0) FROM dbo.BudgetObjectEntries WHERE BudgetID=@ID;";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@ID", SqlDbType.Int).Value = budgetId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    var totals = new BudgetTotals();
                    if (reader.Read()) totals.Pay = Convert.ToDecimal(reader[0]);
                    if (reader.NextResult() && reader.Read()) totals.Allowances = Convert.ToDecimal(reader[0]);
                    if (reader.NextResult() && reader.Read()) totals.OtherObjects = Convert.ToDecimal(reader[0]);
                    return totals;
                }
            }
        }

        internal static DataSet GetReportData(int budgetId)
        {
            const string sql = @"
SELECT * FROM dbo.AnnualBudgets WHERE BudgetID=@ID;
SELECT l.* FROM dbo.BudgetStaffLines l WHERE l.BudgetID=@ID ORDER BY l.BPS DESC,l.Designation,l.EmployeeName;
SELECT a.BudgetStaffLineID,a.AllowanceID,a.AllowanceCodeSnapshot AllowanceCode,a.AllowanceNameSnapshot AllowanceName,a.MonthlyAmount
FROM dbo.BudgetStaffAllowances a INNER JOIN dbo.BudgetStaffLines l ON l.BudgetStaffLineID=a.BudgetStaffLineID
WHERE l.BudgetID=@ID ORDER BY a.SortOrderSnapshot,a.AllowanceCodeSnapshot;
SELECT * FROM dbo.BudgetObjectEntries WHERE BudgetID=@ID ORDER BY SortOrder,ObjectCode;
SELECT l.BudgetPostID,MAX(l.StaffCategory) StaffCategory,MAX(l.PostCode) PostCode,MAX(l.Designation) PostName,MAX(l.BPS) BPS,
COUNT(*) Sanctioned,SUM(CASE WHEN l.IsVacant=0 THEN 1 ELSE 0 END) Working,SUM(CASE WHEN l.IsVacant=1 THEN 1 ELSE 0 END) Vacant
FROM dbo.BudgetStaffLines l WHERE l.BudgetID=@ID GROUP BY l.BudgetPostID ORDER BY MAX(l.BPS) DESC,MAX(l.Designation);
SELECT * FROM dbo.BudgetPayScales ORDER BY BPS;";
            var data = new DataSet();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@ID", SqlDbType.Int).Value = budgetId;
                adapter.Fill(data);
            }
            if (data.Tables.Count < 6 || data.Tables[0].Rows.Count == 0)
                throw new InvalidOperationException("The selected budget could not be loaded.");
            return data;
        }

        private static DataTable Fill(string sql, params SqlParameter[] parameters)
        {
            var table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                if (parameters != null && parameters.Length > 0) command.Parameters.AddRange(parameters);
                adapter.Fill(table);
            }
            return table;
        }

        private static void Execute(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null && parameters.Length > 0) command.Parameters.AddRange(parameters);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static string Required(string value, string fieldName, int maximumLength)
        {
            value = (value ?? string.Empty).Trim();
            if (value.Length == 0) throw new InvalidOperationException("Enter the " + fieldName + ".");
            if (value.Length > maximumLength) throw new InvalidOperationException("The " + fieldName + " is too long.");
            return value;
        }

        private static SqlParameter Text(string name, int size, string value) { return new SqlParameter(name, SqlDbType.NVarChar, size) { Value = value ?? string.Empty }; }
        private static SqlParameter NullableText(string name, int size, string value) { return new SqlParameter(name, SqlDbType.NVarChar, size) { Value = string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim() }; }
        private static SqlParameter Int(string name, int value) { return new SqlParameter(name, SqlDbType.Int) { Value = value }; }
        private static SqlParameter Money(string name, decimal value) { var p = new SqlParameter(name, SqlDbType.Decimal); p.Precision = 18; p.Scale = 2; p.Value = decimal.Round(value, 2); return p; }
    }

    internal sealed class BudgetHeader
    {
        internal string BudgetName { get; set; }
        internal int FiscalStartYear { get; set; }
        internal int FiscalEndYear { get; set; }
        internal string SchoolName { get; set; }
        internal string LocalGovernmentName { get; set; }
        internal string DemandName { get; set; }
        internal string GrantNo { get; set; }
        internal string DetailedFunctionCode { get; set; }
        internal string DetailedFunctionName { get; set; }
        internal string CostCenterCode { get; set; }
        internal int? CreatedByUserID { get; set; }
    }

    internal sealed class BudgetTotals
    {
        internal decimal Pay { get; set; }
        internal decimal Allowances { get; set; }
        internal decimal OtherObjects { get; set; }
        internal decimal GrandTotal { get { return Pay + Allowances + OtherObjects; } }
    }
}
