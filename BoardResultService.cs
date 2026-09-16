using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace DigitalSchoolManager
{
    internal static class BoardResultService
    {
        private static readonly object SchemaSync = new object();
        private static volatile bool _schemaReady;

        internal static string ConnectionString
        {
            get
            {
                ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["SchoolDB"];
                if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                    throw new ConfigurationErrorsException("The SchoolDB connection string is missing from Web.config.");
                return setting.ConnectionString;
            }
        }

        internal static void EnsureSchema(Page page)
        {
            if (_schemaReady) return;
            lock (SchemaSync)
            {
                if (_schemaReady) return;
                string path = page.Server.MapPath("~/BoardResult_Database_Update.sql");
                if (!File.Exists(path))
                    throw new FileNotFoundException("The board result database update script is missing.", path);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(File.ReadAllText(path), connection))
                {
                    command.CommandTimeout = 180;
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                _schemaReady = true;
            }
        }

        internal static DataTable Fill(string sql, params SqlParameter[] parameters)
        {
            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);
                adapter.Fill(table);
            }
            return table;
        }

        internal static int Execute(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        internal static int ExecuteIdentity(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);
                connection.Open();
                object value = command.ExecuteScalar();
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
        }

        internal static int GetClassLevel(string className)
        {
            Match match = Regex.Match(className ?? string.Empty, @"(?<!\d)(9|10|11|12)(?:st|nd|rd|th)?(?!\d)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            int level;
            return match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out level) ? level : 0;
        }

        internal static string PerformanceStatus(decimal schoolPercentage, decimal boardPercentage)
        {
            decimal difference = decimal.Round(schoolPercentage - boardPercentage, 2);
            if (difference > 0) return "Above Board";
            if (difference < 0) return "Below Board";
            return "Equal to Board";
        }
    }
}
