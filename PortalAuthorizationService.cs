using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace DigitalSchoolManager
{
    internal static class PortalAuthorizationService
    {
        internal const string AdministratorRole = "Admin";
        internal const string TeacherRole = "Teacher";

        internal static string Role(HttpContext context)
        {
            return context == null || context.Session == null
                ? string.Empty
                : Convert.ToString(context.Session["UserRole"]);
        }

        internal static bool IsTeacher(HttpContext context)
        {
            return SystemUserSecurity.IsAuthenticated(context) &&
                   string.Equals(Role(context), TeacherRole, StringComparison.OrdinalIgnoreCase);
        }

        internal static int TeacherId(HttpContext context)
        {
            int value;
            return context != null && context.Session != null &&
                   int.TryParse(Convert.ToString(context.Session["TeacherID"]), out value)
                ? value : 0;
        }

        internal static bool TeacherOwnsClass(HttpContext context, int classId)
        {
            if (!IsTeacher(context) || classId <= 0)
                return false;

            const string sql = @"SELECT CASE WHEN EXISTS
(
    SELECT 1 FROM dbo.TeachersClasses tc
    INNER JOIN dbo.Teachers t ON t.teacherid=tc.InchargeID AND ISNULL(t.IsActive,1)=1
    WHERE tc.InchargeID=@TeacherID AND tc.ClassID=@ClassID
) THEN 1 ELSE 0 END;";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@TeacherID", SqlDbType.Int).Value = TeacherId(context);
                command.Parameters.Add("@ClassID", SqlDbType.Int).Value = classId;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        internal static bool TeacherTeachesClassSubject(HttpContext context, int classId, int subjectId)
        {
            if (!IsTeacher(context) || classId <= 0 || subjectId <= 0)
                return false;

            const string sql = @"SELECT CASE WHEN EXISTS
(
    SELECT 1 FROM dbo.TimeTable tt
    INNER JOIN dbo.Teachers t ON t.teacherid=tt.TeacherID AND ISNULL(t.IsActive,1)=1
    WHERE tt.TeacherID=@TeacherID AND tt.ClassID=@ClassID AND tt.SubjectID=@SubjectID
) THEN 1 ELSE 0 END;";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@TeacherID", SqlDbType.Int).Value = TeacherId(context);
                command.Parameters.Add("@ClassID", SqlDbType.Int).Value = classId;
                command.Parameters.Add("@SubjectID", SqlDbType.Int).Value = subjectId;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        internal static void RequireTeacherClass(HttpContext context, int classId)
        {
            if (IsTeacher(context) && !TeacherOwnsClass(context, classId))
                throw new UnauthorizedAccessException("You can work only with the class assigned to you as class incharge.");
        }

        internal static bool CanAccessPage(HttpContext context, string pageName)
        {
            if (SystemUserSecurity.IsAdministrator(context))
                return true;
            if (!IsTeacher(context))
                return false;

            string name = (Path.GetFileNameWithoutExtension(pageName) ?? string.Empty).ToLowerInvariant();
            return name == "teacherdashboard" || name == "teacherdiary" ||
                   name == "studentattendance" || name == "studentresultentry" ||
                   name == "feecollection";
        }

        internal static string RoleHome(HttpContext context)
        {
            return IsTeacher(context) ? "~/TeacherDashboard.aspx" : "~/AdminDashboard.aspx";
        }

        private static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString; }
        }
    }
}
