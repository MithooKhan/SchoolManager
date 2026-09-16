using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.Security;

namespace DigitalSchoolManager
{
    internal static class PortalIdentityService
    {
        private const int MaximumFailures = 5;
        private const int LockoutMinutes = 15;

        internal static void EnsureSchema()
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.PortalLoginAttempts',N'U') IS NULL
BEGIN
 CREATE TABLE dbo.PortalLoginAttempts
 (
  LoginKey NVARCHAR(120) NOT NULL CONSTRAINT PK_PortalLoginAttempts PRIMARY KEY,
  FailedCount INT NOT NULL CONSTRAINT DF_PortalLoginAttempts_Failed DEFAULT(0),
  LockoutEndUtc DATETIME2(0) NULL,
  LastAttemptUtc DATETIME2(0) NOT NULL CONSTRAINT DF_PortalLoginAttempts_Last DEFAULT(SYSUTCDATETIME())
 );
END;";
            Execute(sql);
        }

        internal static PortalLoginResult AuthenticateTeacher(string cellNumber, string personalNumber)
        {
            EnsureSchema();
            string login = Normalize(cellNumber);
            string secret = Normalize(personalNumber);
            if (login.Length == 0 || secret.Length == 0)
                return PortalLoginResult.Fail("Enter the teacher cell number and personal number.");

            string key = "TEACHER:" + login;
            DateTime? lockedUntil = GetLockout(key);
            if (lockedUntil.HasValue && lockedUntil.Value > DateTime.UtcNow)
                return PortalLoginResult.Fail("Too many failed attempts. Try again after " + lockedUntil.Value.ToLocalTime().ToString("dd MMM yyyy, hh:mm tt") + ".");

            const string sql = @"
SELECT t.teacherid,ISNULL(t.Name,N'Teacher') TeacherName,
 ISNULL(v.Description,N'Teacher') Designation,t.contactno,t.personalNo
FROM dbo.Teachers t
LEFT JOIN dbo.TeachingVacancyPosition v ON v.PostID=t.PostID
WHERE ISNULL(t.IsActive,1)=1;";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (Normalize(Convert.ToString(reader["contactno"])) == login &&
                            Normalize(Convert.ToString(reader["personalNo"])) == secret)
                        {
                            ClearAttempts(key);
                            return PortalLoginResult.SuccessResult(
                                Convert.ToInt32(reader["teacherid"]),
                                Convert.ToString(reader["TeacherName"]),
                                Convert.ToString(reader["Designation"]),
                                Convert.ToString(reader["contactno"]));
                        }
                    }
                }
            }

            RegisterFailure(key);
            return PortalLoginResult.Fail("The cell number or personal number is incorrect, or the teacher is inactive.");
        }

        internal static void EstablishTeacherSession(HttpContext context, PortalLoginResult result, bool remember)
        {
            context.Session.Clear();
            context.Session["SystemUserID"] = 0;
            context.Session["TeacherID"] = result.TeacherId;
            context.Session["Username"] = result.Username;
            context.Session["DisplayName"] = result.DisplayName;
            context.Session["UserRole"] = PortalAuthorizationService.TeacherRole;
            context.Session["Designation"] = result.Designation;
            context.Session["AuthenticatedAtUtc"] = DateTime.UtcNow;

            DateTime expires = remember ? DateTime.Now.AddDays(30) : DateTime.Now.AddMinutes(30);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                1, "PORTAL:TEACHER:" + result.TeacherId, DateTime.Now, expires,
                remember, PortalAuthorizationService.TeacherRole, FormsAuthentication.FormsCookiePath);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            cookie.HttpOnly = true;
            cookie.Secure = context.Request.IsSecureConnection;
            if (remember)
                cookie.Expires = expires;
            context.Response.Cookies.Add(cookie);
        }

        internal static bool TryRestoreSession(HttpContext context)
        {
            if (context == null || context.Session == null || context.User == null ||
                context.User.Identity == null || !context.User.Identity.IsAuthenticated)
                return false;
            string identity = context.User.Identity.Name ?? string.Empty;
            const string prefix = "PORTAL:TEACHER:";
            int teacherId;
            if (!identity.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(identity.Substring(prefix.Length), out teacherId) || teacherId <= 0)
                return false;

            const string sql = @"SELECT TOP(1) t.Name,t.contactno,ISNULL(v.Description,N'Teacher') Designation
FROM dbo.Teachers t LEFT JOIN dbo.TeachingVacancyPosition v ON v.PostID=t.PostID
WHERE t.teacherid=@TeacherID AND ISNULL(t.IsActive,1)=1;";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@TeacherID", SqlDbType.Int).Value = teacherId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return false;
                    context.Session["SystemUserID"] = 0;
                    context.Session["TeacherID"] = teacherId;
                    context.Session["Username"] = Convert.ToString(reader["contactno"]);
                    context.Session["DisplayName"] = Convert.ToString(reader["Name"]);
                    context.Session["UserRole"] = PortalAuthorizationService.TeacherRole;
                    context.Session["Designation"] = Convert.ToString(reader["Designation"]);
                    context.Session["AuthenticatedAtUtc"] = DateTime.UtcNow;
                    return true;
                }
            }
        }

        private static string Normalize(string value)
        {
            StringBuilder output = new StringBuilder();
            foreach (char character in (value ?? string.Empty).Trim().ToUpperInvariant())
                if (char.IsLetterOrDigit(character)) output.Append(character);
            return output.ToString();
        }

        private static DateTime? GetLockout(string key)
        {
            using (SqlConnection c = new SqlConnection(ConnectionString))
            using (SqlCommand q = new SqlCommand("SELECT LockoutEndUtc FROM dbo.PortalLoginAttempts WHERE LoginKey=@Key", c))
            {
                q.Parameters.Add("@Key", SqlDbType.NVarChar, 120).Value = key;
                c.Open(); object value = q.ExecuteScalar();
                return value == null || value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value);
            }
        }

        private static void RegisterFailure(string key)
        {
            const string sql = @"MERGE dbo.PortalLoginAttempts AS target
USING(SELECT @Key LoginKey) source ON target.LoginKey=source.LoginKey
WHEN MATCHED THEN UPDATE SET FailedCount=CASE WHEN LockoutEndUtc IS NOT NULL AND LockoutEndUtc<SYSUTCDATETIME() THEN 1 ELSE FailedCount+1 END,
 LockoutEndUtc=CASE WHEN (CASE WHEN LockoutEndUtc IS NOT NULL AND LockoutEndUtc<SYSUTCDATETIME() THEN 1 ELSE FailedCount+1 END)>=@Maximum THEN DATEADD(MINUTE,@Minutes,SYSUTCDATETIME()) ELSE LockoutEndUtc END,LastAttemptUtc=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT(LoginKey,FailedCount,LastAttemptUtc) VALUES(@Key,1,SYSUTCDATETIME());";
            using (SqlConnection c = new SqlConnection(ConnectionString))
            using (SqlCommand q = new SqlCommand(sql, c))
            {
                q.Parameters.Add("@Key", SqlDbType.NVarChar, 120).Value = key;
                q.Parameters.Add("@Maximum", SqlDbType.Int).Value = MaximumFailures;
                q.Parameters.Add("@Minutes", SqlDbType.Int).Value = LockoutMinutes;
                c.Open(); q.ExecuteNonQuery();
            }
        }

        private static void ClearAttempts(string key)
        {
            using (SqlConnection c = new SqlConnection(ConnectionString))
            using (SqlCommand q = new SqlCommand("DELETE FROM dbo.PortalLoginAttempts WHERE LoginKey=@Key", c))
            { q.Parameters.Add("@Key", SqlDbType.NVarChar, 120).Value=key; c.Open(); q.ExecuteNonQuery(); }
        }

        private static void Execute(string sql)
        {
            using (SqlConnection c = new SqlConnection(ConnectionString))
            using (SqlCommand q = new SqlCommand(sql,c)) { c.Open(); q.ExecuteNonQuery(); }
        }

        private static string ConnectionString { get { return ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString; } }
    }

    internal sealed class PortalLoginResult
    {
        internal bool Success { get; private set; }
        internal string Message { get; private set; }
        internal int TeacherId { get; private set; }
        internal string DisplayName { get; private set; }
        internal string Designation { get; private set; }
        internal string Username { get; private set; }
        internal static PortalLoginResult Fail(string message) { return new PortalLoginResult { Message=message }; }
        internal static PortalLoginResult SuccessResult(int id,string name,string designation,string username)
        { return new PortalLoginResult { Success=true,TeacherId=id,DisplayName=name,Designation=designation,Username=username,Message="Login successful." }; }
    }
}
