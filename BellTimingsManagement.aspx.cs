using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DigitalSchoolManager
{
    public partial class WebForm20 : System.Web.UI.Page
    {
        // ──────────────────────────────────────────────────────────────────────
        //  Connection string - store in Web.config under <connectionStrings>
        //  <add name="SchoolDB" connectionString="Data Source=.\SQLEXPRESS;
        //       Initial Catalog=SchoolDB;Integrated Security=True" />
        // ──────────────────────────────────────────────────────────────────────
        private string ConnStr =>
            ConfigurationManager.ConnectionStrings["SchoolDB"].ConnectionString;

        // ══ PAGE LOAD ══════════════════════════════════════════════════════════
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    EnsureTableExists();   // Creates table if missing (dev convenience)
                    BindGrid();
                    SetEditMode(false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Page load error: " + ex.Message, false);
            }
        }

        // ══ SAVE (INSERT) ══════════════════════════════════════════════════════
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsValid) return;

                string periodName = txtPeriodName.Text.Trim();
                string startTime = FormatTime(txtStartTime.Text.Trim(), ddlStartAMPM.SelectedValue);
                string endTime = FormatTime(txtEndTime.Text.Trim(), ddlEndAMPM.SelectedValue);

                // Validate logical time order
                if (!ValidateTimeOrder(startTime, endTime, out string timeError))
                {
                    ShowMessage(timeError, false);
                    return;
                }

                // Check uniqueness of period name
                if (IsPeriodNameDuplicate(periodName, 0))
                {
                    ShowMessage($" A period named '<strong>{periodName}</strong>' already exists. Period names must be unique.", false);
                    return;
                }

                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    string sql = @"INSERT INTO BellTimeTable (PeriodName, PeriodStartTime, PeriodEndTime)
                                   VALUES (@PeriodName, @StartTime, @EndTime)";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@PeriodName", periodName);
                        cmd.Parameters.AddWithValue("@StartTime", startTime);
                        cmd.Parameters.AddWithValue("@EndTime", endTime);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage($" Period '<strong>{periodName}</strong>' has been added successfully.", true);
                ClearForm();
                BindGrid();
            }
            catch (SqlException ex)
            {
                ShowMessage("Database error while saving: " + ex.Message, false);
            }
            catch (Exception ex)
            {
                ShowMessage("Unexpected error while saving: " + ex.Message, false);
            }
        }

        // ══ UPDATE ═════════════════════════════════════════════════════════════
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsValid) return;

                if (!int.TryParse(hfBellID.Value, out int bellID) || bellID == 0)
                {
                    ShowMessage("No record selected for update.", false);
                    return;
                }

                string periodName = txtPeriodName.Text.Trim();
                string startTime = FormatTime(txtStartTime.Text.Trim(), ddlStartAMPM.SelectedValue);
                string endTime = FormatTime(txtEndTime.Text.Trim(), ddlEndAMPM.SelectedValue);

                if (!ValidateTimeOrder(startTime, endTime, out string timeError))
                {
                    ShowMessage(timeError, false);
                    return;
                }

                if (IsPeriodNameDuplicate(periodName, bellID))
                {
                    ShowMessage($" Another period named '<strong>{periodName}</strong>' already exists.", false);
                    return;
                }

                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    string sql = @"UPDATE BellTimeTable
                                   SET PeriodName = @PeriodName,
                                       PeriodStartTime = @StartTime,
                                       PeriodEndTime   = @EndTime
                                   WHERE BellID = @BellID";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@PeriodName", periodName);
                        cmd.Parameters.AddWithValue("@StartTime", startTime);
                        cmd.Parameters.AddWithValue("@EndTime", endTime);
                        cmd.Parameters.AddWithValue("@BellID", bellID);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage($" Period '<strong>{periodName}</strong>' has been updated successfully.", true);
                ClearForm();
                SetEditMode(false);
                BindGrid();
            }
            catch (SqlException ex)
            {
                ShowMessage("Database error while updating: " + ex.Message, false);
            }
            catch (Exception ex)
            {
                ShowMessage("Unexpected error while updating: " + ex.Message, false);
            }
        }

        // ══ GRIDVIEW ROW COMMAND (EDIT / DELETE) ══════════════════════════════
        protected void gvBellTimetable_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (!int.TryParse(e.CommandArgument?.ToString(), out int bellID)) return;

                if (e.CommandName == "EditRow")
                {
                    LoadRecordForEdit(bellID);
                }
                else if (e.CommandName == "DeleteRow")
                {
                    DeleteRecord(bellID);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Row command error: " + ex.Message, false);
            }
        }

        // ══ CLEAR / CANCEL ════════════════════════════════════════════════════
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            SetEditMode(false);
            pnlMsg.Visible = false;
        }

        // ══ PRIVATE HELPERS ═══════════════════════════════════════════════════

        /// <summary>Load a record into the form for editing.</summary>
        private void LoadRecordForEdit(int bellID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    string sql = "SELECT * FROM BellTimeTable WHERE BellID = @BellID";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@BellID", bellID);
                        con.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                hfBellID.Value = bellID.ToString();
                                txtPeriodName.Text = dr["PeriodName"].ToString();

                                // Split stored "hh:mm AM/PM" back into components
                                SplitStoredTime(dr["PeriodStartTime"].ToString(),
                                                out string sTime, out string sAMPM);
                                SplitStoredTime(dr["PeriodEndTime"].ToString(),
                                                out string eTime, out string eAMPM);

                                txtStartTime.Text = sTime;
                                ddlStartAMPM.SelectedValue = sAMPM;
                                txtEndTime.Text = eTime;
                                ddlEndAMPM.SelectedValue = eAMPM;

                                SetEditMode(true);
                                ShowMessage($"Editing period: <strong>{dr["PeriodName"]}</strong> - make your changes and click Update.", true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading record: " + ex.Message, false);
            }
        }

        /// <summary>Delete a record from the database.</summary>
        private void DeleteRecord(int bellID)
        {
            try
            {
                string deletedName = string.Empty;

                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    // Fetch name for feedback message
                    using (SqlCommand getName = new SqlCommand(
                        "SELECT PeriodName FROM BellTimeTable WHERE BellID=@ID", con))
                    {
                        getName.Parameters.AddWithValue("@ID", bellID);
                        con.Open();
                        deletedName = getName.ExecuteScalar()?.ToString() ?? "Unknown";
                    }
                }

                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "DELETE FROM BellTimeTable WHERE BellID = @BellID", con))
                    {
                        cmd.Parameters.AddWithValue("@BellID", bellID);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                ShowMessage($" Period '<strong>{deletedName}</strong>' has been deleted.", true);
                ClearForm();
                SetEditMode(false);
                BindGrid();
            }
            catch (SqlException ex)
            {
                ShowMessage("Database error while deleting: " + ex.Message, false);
            }
            catch (Exception ex)
            {
                ShowMessage("Unexpected error while deleting: " + ex.Message, false);
            }
        }

        /// <summary>Bind the GridView and update summary stats.</summary>
        private void BindGrid()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    string sql = "SELECT BellID, PeriodName, PeriodStartTime, PeriodEndTime FROM BellTimeTable ORDER BY BellID";
                    using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Add computed Duration column
                        dt.Columns.Add("Duration", typeof(string));
                        int totalMinutes = 0;
                        string firstBell = "", lastBell = "";

                        foreach (DataRow row in dt.Rows)
                        {
                            try
                            {
                                DateTime start = DateTime.Parse(row["PeriodStartTime"].ToString());
                                DateTime end = DateTime.Parse(row["PeriodEndTime"].ToString());
                                int mins = (int)(end - start).TotalMinutes;

                                row["Duration"] = mins > 0
                                    ? FormatDuration(mins)
                                    : "-";

                                if (mins > 0) totalMinutes += mins;
                            }
                            catch
                            {
                                row["Duration"] = "-";
                            }
                        }

                        // Stats
                        lblTotalPeriods.Text = dt.Rows.Count.ToString();
                        lblTotalMins.Text = totalMinutes > 0 ? FormatDuration(totalMinutes) : "0";

                        if (dt.Rows.Count > 0)
                        {
                            firstBell = dt.Rows[0]["PeriodStartTime"].ToString();
                            lastBell = dt.Rows[dt.Rows.Count - 1]["PeriodEndTime"].ToString();
                            lblFirstBell.Text = firstBell;
                            lblLastBell.Text = lastBell;
                        }
                        else
                        {
                            lblFirstBell.Text = "-";
                            lblLastBell.Text = "-";
                        }

                        gvBellTimetable.DataSource = dt;
                        gvBellTimetable.DataBind();
                    }
                }
            }
            catch (SqlException ex)
            {
                ShowMessage("Error loading timetable: " + ex.Message, false);
            }
            catch (Exception ex)
            {
                ShowMessage("Unexpected error loading grid: " + ex.Message, false);
            }
        }

        /// <summary>Check if a period name is already used (excluding the current record on edit).</summary>
        private bool IsPeriodNameDuplicate(string name, int excludeBellID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    string sql = "SELECT COUNT(1) FROM BellTimeTable WHERE LOWER(PeriodName) = LOWER(@Name) AND BellID <> @ID";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@ID", excludeBellID);
                        con.Open();
                        return (int)cmd.ExecuteScalar() > 0;
                    }
                }
            }
            catch
            {
                return false; // Let the DB constraint catch it if needed
            }
        }

        /// <summary>Combine hh:mm + AM/PM into a parsable time string.</summary>
        private string FormatTime(string hhmm, string ampm)
        {
            return $"{hhmm} {ampm}";
        }

        /// <summary>Split "hh:mm AM" back into time part and AM/PM.</summary>
        private void SplitStoredTime(string stored, out string time, out string ampm)
        {
            time = "12:00";
            ampm = "AM";
            if (string.IsNullOrWhiteSpace(stored)) return;
            string[] parts = stored.Trim().Split(' ');
            if (parts.Length >= 2)
            {
                time = parts[0];
                ampm = parts[1].ToUpper() == "PM" ? "PM" : "AM";
            }
        }

        /// <summary>Ensure end time is after start time.</summary>
        private bool ValidateTimeOrder(string start, string end, out string error)
        {
            error = string.Empty;
            try
            {
                DateTime s = DateTime.Parse(start);
                DateTime e = DateTime.Parse(end);
                if (e <= s)
                {
                    error = " End time must be after start time.";
                    return false;
                }
                return true;
            }
            catch
            {
                error = " Invalid time format. Please use hh:mm and select AM/PM.";
                return false;
            }
        }

        /// <summary>Convert total minutes into a readable "Xh Ym" string.</summary>
        private string FormatDuration(int totalMinutes)
        {
            if (totalMinutes <= 0) return "0 min";
            int h = totalMinutes / 60;
            int m = totalMinutes % 60;
            if (h > 0 && m > 0) return $"{h}h {m}m";
            if (h > 0) return $"{h}h";
            return $"{m} min";
        }

        /// <summary>Reset all form fields to default empty state.</summary>
        private void ClearForm()
        {
            hfBellID.Value = "0";
            txtPeriodName.Text = string.Empty;
            txtStartTime.Text = string.Empty;
            txtEndTime.Text = string.Empty;
            ddlStartAMPM.SelectedValue = "AM";
            ddlEndAMPM.SelectedValue = "AM";
        }

        /// <summary>Toggle between Add mode and Edit mode UI.</summary>
        private void SetEditMode(bool editing)
        {
            btnSave.Visible = !editing;
            btnUpdate.Visible = editing;
            btnCancel.Visible = editing;
        }

        /// <summary>Display a themed alert message below the hero.</summary>
        private void ShowMessage(string message, bool success)
        {
            pnlMsg.Visible = true;
            pnlMsg.CssClass = success ? "alert alert-success" : "alert alert-danger";
            litMsgIcon.Text = success ? "" : "";
            litMsg.Text = message;
            upNotify.Update();
        }

        // ══ TABLE AUTO-CREATE (development convenience) ════════════════════════
        /// <summary>
        /// Creates the BellTimeTable if it doesn't exist.
        /// Remove this in production - run the SQL script instead.
        /// </summary>
        private void EnsureTableExists()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnStr))
                {
                    string sql = @"
                        IF NOT EXISTS (
                            SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                            WHERE TABLE_NAME = 'BellTimeTable'
                        )
                        BEGIN
                            CREATE TABLE BellTimeTable (
                                BellID          INT IDENTITY(1,1) PRIMARY KEY,
                                PeriodName      NVARCHAR(50) NOT NULL,
                                PeriodStartTime NVARCHAR(10) NOT NULL,
                                PeriodEndTime   NVARCHAR(10) NOT NULL,
                                CONSTRAINT UQ_PeriodName UNIQUE (PeriodName)
                            );
                        END";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log silently - user will see DB error when they try to save
                System.Diagnostics.Debug.WriteLine("Table creation warning: " + ex.Message);
            }
        }
    }
}