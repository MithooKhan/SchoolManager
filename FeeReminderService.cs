using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace DigitalSchoolManager
{
    internal static class FeeReminderService
    {
        internal static void EnsureSchema()
        {
            const string sql = @"
IF OBJECT_ID(N'dbo.FundsCollection', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.FundsCollection', 'FeeMonth') IS NULL
        ALTER TABLE dbo.FundsCollection ADD FeeMonth TINYINT NULL;
    IF COL_LENGTH('dbo.FundsCollection', 'FeeYear') IS NULL
        ALTER TABLE dbo.FundsCollection ADD FeeYear SMALLINT NULL;
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.FundsCollection')
          AND name = N'IX_FundsCollection_StudentPeriod'
    )
        CREATE NONCLUSTERED INDEX IX_FundsCollection_StudentPeriod
            ON dbo.FundsCollection (StudentID, FeeYear, FeeMonth)
            INCLUDE (VoucherNo, FeeID, DateOfDeposit, PaidAmount);
END;

IF OBJECT_ID(N'dbo.FeeReminderOutbox', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FeeReminderOutbox
    (
        ReminderID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FeeReminderOutbox PRIMARY KEY,
        StudentID INT NOT NULL,
        FeeMonth TINYINT NOT NULL,
        FeeYear SMALLINT NOT NULL,
        ParentContact NVARCHAR(30) NULL,
        StudentName NVARCHAR(150) NOT NULL,
        MessageText NVARCHAR(500) NOT NULL,
        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_FeeReminderOutbox_Status DEFAULT (N'Pending'),
        Attempts INT NOT NULL CONSTRAINT DF_FeeReminderOutbox_Attempts DEFAULT (0),
        CreatedByUserID INT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_FeeReminderOutbox_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        LastAttemptAtUtc DATETIME2(0) NULL,
        SentAtUtc DATETIME2(0) NULL,
        ProviderResponse NVARCHAR(500) NULL
    );
END;

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.FeeReminderOutbox')
      AND name = N'UX_FeeReminderOutbox_StudentPeriod'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_FeeReminderOutbox_StudentPeriod
        ON dbo.FeeReminderOutbox (StudentID, FeeYear, FeeMonth);
END;";

            using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        internal static ReminderDispatchResult SendOrQueue(
            int studentId,
            int feeMonth,
            int feeYear,
            int? createdByUserId)
        {
            if (studentId <= 0 || feeMonth < 1 || feeMonth > 12 || feeYear < 2000 || feeYear > 2100)
            {
                return ReminderDispatchResult.Failed("The reminder request is invalid.");
            }

            EnsureSchema();
            ReminderStudent student = LoadStudent(studentId);
            if (student == null)
            {
                return ReminderDispatchResult.Failed("The student record was not found.");
            }

            string period = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(feeMonth) + " " +
                            feeYear.ToString(CultureInfo.InvariantCulture);
            string message = BuildMessage(student, period);
            int reminderId;
            string currentStatus;

            using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    const string selectSql = @"
SELECT TOP (1) ReminderID, Status
FROM dbo.FeeReminderOutbox WITH (UPDLOCK, HOLDLOCK)
WHERE StudentID = @StudentID AND FeeMonth = @FeeMonth AND FeeYear = @FeeYear;";

                    using (SqlCommand select = new SqlCommand(selectSql, connection, transaction))
                    {
                        AddPeriodParameters(select, studentId, feeMonth, feeYear);
                        using (SqlDataReader reader = select.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                reminderId = Convert.ToInt32(reader["ReminderID"], CultureInfo.InvariantCulture);
                                currentStatus = Convert.ToString(reader["Status"]);
                            }
                            else
                            {
                                reminderId = 0;
                                currentStatus = string.Empty;
                            }
                        }
                    }

                    if (string.Equals(currentStatus, "Sent", StringComparison.OrdinalIgnoreCase))
                    {
                        transaction.Commit();
                        return ReminderDispatchResult.Sent("A reminder for this student and fee period has already been sent.");
                    }

                    if (reminderId == 0)
                    {
                        const string insertSql = @"
INSERT INTO dbo.FeeReminderOutbox
       (StudentID, FeeMonth, FeeYear, ParentContact, StudentName, MessageText,
        Status, Attempts, CreatedByUserID, CreatedAtUtc)
VALUES (@StudentID, @FeeMonth, @FeeYear, @ParentContact, @StudentName, @MessageText,
        N'Pending', 0, @CreatedByUserID, SYSUTCDATETIME());
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using (SqlCommand insert = new SqlCommand(insertSql, connection, transaction))
                        {
                            AddPeriodParameters(insert, studentId, feeMonth, feeYear);
                            insert.Parameters.Add("@ParentContact", SqlDbType.NVarChar, 30).Value =
                                string.IsNullOrWhiteSpace(student.ContactNo) ? (object)DBNull.Value : student.ContactNo;
                            insert.Parameters.Add("@StudentName", SqlDbType.NVarChar, 150).Value = student.Name;
                            insert.Parameters.Add("@MessageText", SqlDbType.NVarChar, 500).Value = message;
                            insert.Parameters.Add("@CreatedByUserID", SqlDbType.Int).Value =
                                createdByUserId.HasValue ? (object)createdByUserId.Value : DBNull.Value;
                            reminderId = Convert.ToInt32(insert.ExecuteScalar(), CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        const string updateSql = @"
UPDATE dbo.FeeReminderOutbox
SET ParentContact = @ParentContact,
    StudentName = @StudentName,
    MessageText = @MessageText,
    Status = N'Pending',
    ProviderResponse = NULL
WHERE ReminderID = @ReminderID;";
                        using (SqlCommand update = new SqlCommand(updateSql, connection, transaction))
                        {
                            update.Parameters.Add("@ParentContact", SqlDbType.NVarChar, 30).Value =
                                string.IsNullOrWhiteSpace(student.ContactNo) ? (object)DBNull.Value : student.ContactNo;
                            update.Parameters.Add("@StudentName", SqlDbType.NVarChar, 150).Value = student.Name;
                            update.Parameters.Add("@MessageText", SqlDbType.NVarChar, 500).Value = message;
                            update.Parameters.Add("@ReminderID", SqlDbType.Int).Value = reminderId;
                            update.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
            }

            if (string.IsNullOrWhiteSpace(student.ContactNo))
            {
                UpdateStatus(reminderId, "Failed", "No parent contact number is saved for this student.", false);
                return ReminderDispatchResult.Failed("No parent contact number is saved. The reminder was recorded as failed.");
            }

            string gatewayUrl = ConfigurationManager.AppSettings["SmsGatewayUrl"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(gatewayUrl))
            {
                UpdateStatus(reminderId, "Pending", "SMS gateway is not configured; reminder retained in the outbox.", false);
                return ReminderDispatchResult.Queued(
                    "Reminder generated and queued. Configure SmsGatewayUrl and SmsGatewayApiKey in Web.config to deliver it automatically.");
            }

            try
            {
                string providerResponse = SendToGateway(gatewayUrl, student.ContactNo, message);
                UpdateStatus(reminderId, "Sent", providerResponse, true);
                return ReminderDispatchResult.Sent("Reminder sent successfully to " + student.ContactNo + ".");
            }
            catch (Exception ex)
            {
                UpdateStatus(reminderId, "Failed", Limit(ex.Message, 500), false);
                return ReminderDispatchResult.Failed("The reminder could not be delivered. It remains available for retry.");
            }
        }

        private static ReminderStudent LoadStudent(int studentId)
        {
            const string sql = @"
SELECT TOP (1)
       ISNULL(s.Name, '') AS StudentName,
       ISNULL(s.Regno, '') AS Regno,
       ISNULL(s.ContactNo, '') AS ContactNo,
       ISNULL(c.ClassName, '') AS ClassName
FROM dbo.Students s
LEFT JOIN dbo.StudentClass sc ON sc.StudentID = s.StudentID
LEFT JOIN dbo.Classes c ON c.ClassID = sc.ClassID
WHERE s.StudentID = @StudentID
ORDER BY sc.ClassID DESC;";

            using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new ReminderStudent
                    {
                        Name = Convert.ToString(reader["StudentName"]),
                        Regno = Convert.ToString(reader["Regno"]),
                        ContactNo = Convert.ToString(reader["ContactNo"]),
                        ClassName = Convert.ToString(reader["ClassName"])
                    };
                }
            }
        }

        private static string BuildMessage(ReminderStudent student, string period)
        {
            return "Dear Parent, the school fee for " + student.Name +
                   (string.IsNullOrWhiteSpace(student.Regno) ? string.Empty : " (Reg. " + student.Regno + ")") +
                   (string.IsNullOrWhiteSpace(student.ClassName) ? string.Empty : ", " + student.ClassName) +
                   ", for " + period +
                   " is outstanding. Please deposit it at Government Higher Secondary School Maankot. " +
                   "Ignore this reminder if payment has already been made.";
        }

        private static string SendToGateway(string gatewayUrl, string contact, string message)
        {
            Uri uri;
            if (!Uri.TryCreate(gatewayUrl, UriKind.Absolute, out uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ConfigurationErrorsException("SmsGatewayUrl must be a valid HTTP or HTTPS URL.");
            }

            string senderId = ConfigurationManager.AppSettings["SmsSenderId"] ?? "GHSSMaankot";
            string payload = "{\"to\":\"" + JsonEscape(contact) + "\",\"message\":\"" +
                             JsonEscape(message) + "\",\"senderId\":\"" + JsonEscape(senderId) + "\"}";
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Accept = "application/json";
            request.Timeout = 15000;
            request.ReadWriteTimeout = 15000;
            request.ContentLength = payloadBytes.Length;

            string apiKey = ConfigurationManager.AppSettings["SmsGatewayApiKey"] ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers[HttpRequestHeader.Authorization] = "Bearer " + apiKey.Trim();
            }

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(payloadBytes, 0, payloadBytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream() ?? Stream.Null))
            {
                string responseText = reader.ReadToEnd();
                return Limit(((int)response.StatusCode).ToString(CultureInfo.InvariantCulture) + " " +
                             response.StatusDescription + " " + responseText, 500);
            }
        }

        private static void UpdateStatus(int reminderId, string status, string providerResponse, bool sent)
        {
            const string sql = @"
UPDATE dbo.FeeReminderOutbox
SET Status = @Status,
    Attempts = Attempts + 1,
    LastAttemptAtUtc = SYSUTCDATETIME(),
    SentAtUtc = CASE WHEN @Sent = 1 THEN SYSUTCDATETIME() ELSE SentAtUtc END,
    ProviderResponse = @ProviderResponse
WHERE ReminderID = @ReminderID;";

            using (SqlConnection connection = new SqlConnection(SystemUserSecurity.ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = status;
                command.Parameters.Add("@Sent", SqlDbType.Bit).Value = sent;
                command.Parameters.Add("@ProviderResponse", SqlDbType.NVarChar, 500).Value =
                    string.IsNullOrWhiteSpace(providerResponse) ? (object)DBNull.Value : Limit(providerResponse, 500);
                command.Parameters.Add("@ReminderID", SqlDbType.Int).Value = reminderId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void AddPeriodParameters(SqlCommand command, int studentId, int feeMonth, int feeYear)
        {
            command.Parameters.Add("@StudentID", SqlDbType.Int).Value = studentId;
            command.Parameters.Add("@FeeMonth", SqlDbType.TinyInt).Value = feeMonth;
            command.Parameters.Add("@FeeYear", SqlDbType.SmallInt).Value = feeYear;
        }

        private static string JsonEscape(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private static string Limit(string value, int length)
        {
            string text = value ?? string.Empty;
            return text.Length <= length ? text : text.Substring(0, length);
        }

        private sealed class ReminderStudent
        {
            public string Name { get; set; }
            public string Regno { get; set; }
            public string ContactNo { get; set; }
            public string ClassName { get; set; }
        }
    }

    internal sealed class ReminderDispatchResult
    {
        internal bool Success { get; private set; }
        internal bool IsQueued { get; private set; }
        internal string Message { get; private set; }

        internal static ReminderDispatchResult Sent(string message)
        {
            return new ReminderDispatchResult { Success = true, Message = message };
        }

        internal static ReminderDispatchResult Queued(string message)
        {
            return new ReminderDispatchResult { Success = true, IsQueued = true, Message = message };
        }

        internal static ReminderDispatchResult Failed(string message)
        {
            return new ReminderDispatchResult { Success = false, Message = message };
        }
    }
}
