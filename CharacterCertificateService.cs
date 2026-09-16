using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Web;

namespace DigitalSchoolManager
{
    internal static class CharacterCertificateService
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
            SchoolLifecycleService.EnsureSchema();
            string path = HttpContext.Current.Server.MapPath("~/CharacterCertificate_Database_Update.sql");
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(File.ReadAllText(path), connection))
            {
                command.CommandTimeout = 120;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        internal static DataTable SearchEligible(string search)
        {
            return Fill(@"SELECT s.CertificateID,s.CertificateNumber,s.StudentNameSnapshot,s.FatherNameSnapshot,
 s.RegistrationNoSnapshot,s.FormBNoSnapshot,s.ClassNameSnapshot,s.IssueDate,s.StudentPhotoData,
 CASE WHEN c.CharacterCertificateID IS NULL THEN N'Ready to issue' ELSE c.CharacterCertificateNumber END AS CharacterStatus,
 c.CharacterCertificateID
FROM dbo.StudentSchoolLeavingCertificates s
LEFT JOIN dbo.StudentCharacterCertificates c ON c.SchoolLeavingCertificateID=s.CertificateID
WHERE @Search=N'' OR s.CertificateNumber LIKE @Pattern OR s.StudentNameSnapshot LIKE @Pattern
 OR ISNULL(s.RegistrationNoSnapshot,N'') LIKE @Pattern OR ISNULL(s.FormBNoSnapshot,N'') LIKE @Pattern
ORDER BY s.IssueDate DESC,s.CertificateID DESC;",
                Text("@Search", 120, (search ?? string.Empty).Trim()),
                Text("@Pattern", 130, "%" + (search ?? string.Empty).Trim() + "%"));
        }

        internal static DataRow GetSLC(int certificateId)
        {
            DataTable table = Fill(@"SELECT s.CertificateID,s.CertificateNumber,s.StudentID,s.StudentNameSnapshot,s.FatherNameSnapshot,
 s.RegistrationNoSnapshot,s.FormBNoSnapshot,s.DateOfBirthSnapshot,s.ClassNameSnapshot,
 COALESCE(s.StudentPhotoData,st.StudentImageData) AS StudentPhotoData,s.IssueDate,s.LeavingDate,s.Conduct,s.CertificateRemarks
FROM dbo.StudentSchoolLeavingCertificates s LEFT JOIN dbo.Students st ON st.StudentID=s.StudentID
WHERE s.CertificateID=@ID;", Int("@ID", certificateId));
            if (table.Rows.Count == 0) throw new InvalidOperationException("The selected school leaving certificate was not found.");
            return table.Rows[0];
        }

        internal static int Create(int slcId, DateTime issueDate, string classRemarks, string adminRemarks,
            string signatoryOne, string designationOne, string signatoryTwo, string designationTwo, int? userId)
        {
            classRemarks = Required(classRemarks, "class incharge remarks", 1000);
            adminRemarks = Required(adminRemarks, "school administration remarks", 1000);
            if (issueDate.Year < 2000 || issueDate.Date > DateTime.Today.AddDays(31))
                throw new InvalidOperationException("Enter a valid certificate issue date.");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    int slcCount;
                    using (SqlCommand check = new SqlCommand("SELECT COUNT(*) FROM dbo.StudentSchoolLeavingCertificates WHERE CertificateID=@ID;", connection, transaction))
                    {
                        check.Parameters.Add("@ID", SqlDbType.Int).Value = slcId;
                        slcCount = Convert.ToInt32(check.ExecuteScalar(), CultureInfo.InvariantCulture);
                    }
                    if (slcCount == 0)
                        throw new InvalidOperationException("Issue the student's School Leaving Certificate before issuing a Character Certificate.");

                    int id;
                    using (SqlCommand insert = new SqlCommand(@"INSERT dbo.StudentCharacterCertificates
(CharacterCertificateNumber,SchoolLeavingCertificateID,IssueDate,ClassInchargeRemarks,AdministrationRemarks,
 SignatoryOneName,SignatoryOneDesignation,SignatoryTwoName,SignatoryTwoDesignation,CreatedByUserID)
VALUES(N'PENDING-'+CONVERT(nvarchar(36),NEWID()),@SLC,@Date,@ClassRemarks,@AdminRemarks,@Name1,@Designation1,@Name2,@Designation2,@UserID);
SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
                    {
                        insert.Parameters.Add("@SLC", SqlDbType.Int).Value = slcId;
                        insert.Parameters.Add("@Date", SqlDbType.Date).Value = issueDate.Date;
                        insert.Parameters.Add("@ClassRemarks", SqlDbType.NVarChar, 1000).Value = classRemarks;
                        insert.Parameters.Add("@AdminRemarks", SqlDbType.NVarChar, 1000).Value = adminRemarks;
                        AddNullable(insert, "@Name1", 200, signatoryOne); AddNullable(insert, "@Designation1", 150, designationOne);
                        AddNullable(insert, "@Name2", 200, signatoryTwo); AddNullable(insert, "@Designation2", 150, designationTwo);
                        insert.Parameters.Add("@UserID", SqlDbType.Int).Value = userId.HasValue ? (object)userId.Value : DBNull.Value;
                        try { id = Convert.ToInt32(insert.ExecuteScalar(), CultureInfo.InvariantCulture); }
                        catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                        { throw new InvalidOperationException("A Character Certificate has already been issued for this School Leaving Certificate."); }
                    }
                    string number = string.Format(CultureInfo.InvariantCulture, "GHSS-CC-{0:yyyyMMdd}-{1:D4}", issueDate, id);
                    using (SqlCommand update = new SqlCommand("UPDATE dbo.StudentCharacterCertificates SET CharacterCertificateNumber=@Number WHERE CharacterCertificateID=@ID;", connection, transaction))
                    {
                        update.Parameters.Add("@Number", SqlDbType.NVarChar, 60).Value = number;
                        update.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                        update.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    return id;
                }
            }
        }

        internal static DataRow GetCertificate(int id)
        {
            DataTable table = Fill(@"SELECT c.*,s.CertificateNumber AS SchoolLeavingCertificateNumber,
 s.StudentNameSnapshot,s.FatherNameSnapshot,s.DateOfBirthSnapshot,s.FormBNoSnapshot,s.RegistrationNoSnapshot,
 s.ClassNameSnapshot,COALESCE(s.StudentPhotoData,st.StudentImageData) AS StudentPhotoData,s.LeavingDate,s.Conduct
FROM dbo.StudentCharacterCertificates c
INNER JOIN dbo.StudentSchoolLeavingCertificates s ON s.CertificateID=c.SchoolLeavingCertificateID
LEFT JOIN dbo.Students st ON st.StudentID=s.StudentID
WHERE c.CharacterCertificateID=@ID;", Int("@ID", id));
            if (table.Rows.Count == 0) throw new InvalidOperationException("The Character Certificate was not found.");
            return table.Rows[0];
        }

        private static DataTable Fill(string sql, params SqlParameter[] parameters)
        {
            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            { if (parameters != null) command.Parameters.AddRange(parameters); adapter.Fill(table); }
            return table;
        }
        private static SqlParameter Text(string name, int size, string value) { return new SqlParameter(name, SqlDbType.NVarChar, size) { Value = value ?? string.Empty }; }
        private static SqlParameter Int(string name, int value) { return new SqlParameter(name, SqlDbType.Int) { Value = value }; }
        private static string Required(string value, string label, int max) { value = (value ?? string.Empty).Trim(); if (value.Length == 0) throw new InvalidOperationException("Enter the " + label + "."); if (value.Length > max) throw new InvalidOperationException("The " + label + " is too long."); return value; }
        private static void AddNullable(SqlCommand command, string name, int size, string value) { value = (value ?? string.Empty).Trim(); if (value.Length > size) value = value.Substring(0, size); command.Parameters.Add(name, SqlDbType.NVarChar, size).Value = value.Length == 0 ? (object)DBNull.Value : value; }
    }
}
