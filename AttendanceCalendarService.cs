using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DigitalSchoolManager
{
    /// <summary>
    /// One authoritative school-closure calendar for student and staff attendance.
    /// Sundays are calculated automatically; named holiday periods are stored in SQL Server.
    /// </summary>
    internal static class AttendanceCalendarService
    {
        internal const int HolidayNameMaximumLength = 150;
        private static readonly object SchemaSync = new object();
        private static volatile bool _schemaReady;

        internal static void EnsureSchema()
        {
            if (_schemaReady)
            {
                return;
            }

            lock (SchemaSync)
            {
                if (_schemaReady)
                {
                    return;
                }

                using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
                {
                    connection.Open();

                    // Each migration stage is deliberately executed as a separate
                    // SQL command. SQL Server can otherwise compile references to
                    // HolidayEndDate before an ALTER TABLE in the same batch has
                    // made that column visible on an older installation.
                    ExecuteSchemaCommand(connection, @"
IF OBJECT_ID(N'dbo.SchoolAttendanceHolidays', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchoolAttendanceHolidays
    (
        HolidayID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchoolAttendanceHolidays PRIMARY KEY,
        HolidayDate DATE NOT NULL,
        HolidayEndDate DATE NOT NULL,
        HolidayName NVARCHAR(150) NOT NULL,
        CreatedByUserID INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SchoolAttendanceHolidays_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        UpdatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SchoolAttendanceHolidays_UpdatedAtUtc DEFAULT (SYSUTCDATETIME())
    );
END;");

                    ExecuteSchemaCommand(connection, @"
IF COL_LENGTH(N'dbo.SchoolAttendanceHolidays', N'HolidayEndDate') IS NULL
BEGIN
    ALTER TABLE dbo.SchoolAttendanceHolidays ADD HolidayEndDate DATE NULL;
END;");

                    ExecuteSchemaCommand(connection, @"
UPDATE dbo.SchoolAttendanceHolidays
SET HolidayEndDate = HolidayDate
WHERE HolidayEndDate IS NULL;");

                    ExecuteSchemaCommand(connection, @"
IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.SchoolAttendanceHolidays')
      AND name = N'HolidayEndDate'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE dbo.SchoolAttendanceHolidays ALTER COLUMN HolidayEndDate DATE NOT NULL;
END;");

                    ExecuteSchemaCommand(connection, @"
IF NOT EXISTS
(
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.SchoolAttendanceHolidays')
      AND name = N'CK_SchoolAttendanceHolidays_DateRange'
)
BEGIN
    ALTER TABLE dbo.SchoolAttendanceHolidays WITH CHECK
        ADD CONSTRAINT CK_SchoolAttendanceHolidays_DateRange
        CHECK (HolidayEndDate >= HolidayDate);
END;");

                    ExecuteSchemaCommand(connection, @"
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.SchoolAttendanceHolidays')
      AND name = N'UX_SchoolAttendanceHolidays_Date'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_SchoolAttendanceHolidays_Date
        ON dbo.SchoolAttendanceHolidays (HolidayDate);
END;");

                    _schemaReady = true;
                }
            }
        }

        private static void ExecuteSchemaCommand(SqlConnection connection, string sql)
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = 120;
                command.ExecuteNonQuery();
            }
        }

        internal static bool TryGetClosedDay(DateTime date, out string closedDayName)
        {
            date = date.Date;
            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                closedDayName = "Sunday";
                return true;
            }

            EnsureSchema();
            using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
SELECT TOP (1) HolidayName
FROM dbo.SchoolAttendanceHolidays
WHERE @HolidayDate >= HolidayDate
  AND @HolidayDate <= HolidayEndDate
ORDER BY HolidayDate DESC;", connection))
            {
                command.Parameters.Add("@HolidayDate", SqlDbType.Date).Value = date;
                connection.Open();
                object value = command.ExecuteScalar();
                closedDayName = value == null || value == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(value);
            }

            return !string.IsNullOrWhiteSpace(closedDayName);
        }

        internal static Dictionary<DateTime, string> GetClosedDays(DateTime fromDate, DateTime toDate)
        {
            fromDate = fromDate.Date;
            toDate = toDate.Date;
            if (fromDate > toDate)
            {
                throw new ArgumentException("The start date cannot be later than the end date.");
            }

            var result = new Dictionary<DateTime, string>();
            for (DateTime date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    result[date] = "Sunday";
                }
            }

            EnsureSchema();
            using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
SELECT HolidayDate, HolidayEndDate, HolidayName
FROM dbo.SchoolAttendanceHolidays
WHERE HolidayDate <= @ToDate
  AND HolidayEndDate >= @FromDate
ORDER BY HolidayDate;", connection))
            {
                command.Parameters.Add("@FromDate", SqlDbType.Date).Value = fromDate;
                command.Parameters.Add("@ToDate", SqlDbType.Date).Value = toDate;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime holidayStart = Convert.ToDateTime(reader["HolidayDate"]).Date;
                        DateTime holidayEnd = Convert.ToDateTime(reader["HolidayEndDate"]).Date;
                        DateTime firstDate = holidayStart < fromDate ? fromDate : holidayStart;
                        DateTime lastDate = holidayEnd > toDate ? toDate : holidayEnd;
                        string holidayName = Convert.ToString(reader["HolidayName"]);
                        for (DateTime date = firstDate; date <= lastDate; date = date.AddDays(1))
                        {
                            if (!result.ContainsKey(date))
                            {
                                result.Add(date, holidayName);
                            }
                        }
                    }
                }
            }

            return result;
        }

        internal static DataTable LoadHolidayList()
        {
            EnsureSchema();
            var table = new DataTable();
            using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
SELECT TOP (24)
       HolidayID,
       HolidayDate,
       HolidayDate AS HolidayStartDate,
       HolidayEndDate,
       HolidayName,
       DATEDIFF(DAY, HolidayDate, HolidayEndDate) + 1 AS DurationDays
FROM dbo.SchoolAttendanceHolidays
ORDER BY CASE WHEN HolidayEndDate >= CAST(GETDATE() AS DATE) THEN 0 ELSE 1 END,
         CASE WHEN HolidayEndDate >= CAST(GETDATE() AS DATE) THEN HolidayDate END ASC,
         HolidayEndDate DESC;", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(table);
            }
            return table;
        }

        internal static void SaveHoliday(
            DateTime holidayStartDate,
            DateTime holidayEndDate,
            string holidayName,
            int? userId)
        {
            holidayStartDate = holidayStartDate.Date;
            holidayEndDate = holidayEndDate.Date;
            holidayName = NormalizeHolidayName(holidayName);
            if (holidayEndDate < holidayStartDate)
            {
                throw new InvalidOperationException("The holiday end date cannot be earlier than the start date.");
            }
            if (string.IsNullOrWhiteSpace(holidayName))
            {
                throw new InvalidOperationException("Enter a holiday name.");
            }
            if (holidayName.Length > HolidayNameMaximumLength)
            {
                throw new InvalidOperationException("Holiday name cannot exceed 150 characters.");
            }

            EnsureSchema();
            using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (SqlCommand overlapCommand = new SqlCommand(@"
SELECT TOP (1) HolidayName, HolidayDate, HolidayEndDate
FROM dbo.SchoolAttendanceHolidays WITH (UPDLOCK, HOLDLOCK)
WHERE HolidayDate <> @HolidayStartDate
  AND HolidayDate <= @HolidayEndDate
  AND HolidayEndDate >= @HolidayStartDate
ORDER BY HolidayDate;", connection, transaction))
                        {
                            overlapCommand.Parameters.Add("@HolidayStartDate", SqlDbType.Date).Value = holidayStartDate;
                            overlapCommand.Parameters.Add("@HolidayEndDate", SqlDbType.Date).Value = holidayEndDate;
                            using (SqlDataReader reader = overlapCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string existingName = Convert.ToString(reader["HolidayName"]);
                                    DateTime existingStart = Convert.ToDateTime(reader["HolidayDate"]).Date;
                                    DateTime existingEnd = Convert.ToDateTime(reader["HolidayEndDate"]).Date;
                                    throw new InvalidOperationException(
                                        "This period overlaps " + existingName + " (" +
                                        existingStart.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + " to " +
                                        existingEnd.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + ").");
                                }
                            }
                        }

                        using (SqlCommand command = new SqlCommand(@"
UPDATE dbo.SchoolAttendanceHolidays
SET HolidayName = @HolidayName,
    HolidayEndDate = @HolidayEndDate,
    CreatedByUserID = @CreatedByUserID,
    UpdatedAtUtc = SYSUTCDATETIME()
WHERE HolidayDate = @HolidayStartDate;

IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO dbo.SchoolAttendanceHolidays
           (HolidayDate, HolidayEndDate, HolidayName, CreatedByUserID, CreatedAtUtc, UpdatedAtUtc)
    VALUES (@HolidayStartDate, @HolidayEndDate, @HolidayName, @CreatedByUserID, SYSUTCDATETIME(), SYSUTCDATETIME());
END;", connection, transaction))
                        {
                            command.Parameters.Add("@HolidayStartDate", SqlDbType.Date).Value = holidayStartDate;
                            command.Parameters.Add("@HolidayEndDate", SqlDbType.Date).Value = holidayEndDate;
                            command.Parameters.Add("@HolidayName", SqlDbType.NVarChar, HolidayNameMaximumLength).Value = holidayName;
                            command.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value =
                                userId.HasValue ? (object)userId.Value : DBNull.Value;
                            command.ExecuteNonQuery();
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

        internal static bool DeleteHoliday(int holidayId)
        {
            if (holidayId <= 0)
            {
                return false;
            }

            EnsureSchema();
            using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
            using (SqlCommand command = new SqlCommand(
                "DELETE FROM dbo.SchoolAttendanceHolidays WHERE HolidayID = @HolidayID;", connection))
            {
                command.Parameters.Add("@HolidayID", SqlDbType.Int).Value = holidayId;
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        private static string NormalizeHolidayName(string value)
        {
            return Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();
        }
    }
}
