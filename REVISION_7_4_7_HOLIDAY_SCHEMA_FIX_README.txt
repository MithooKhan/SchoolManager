DIGITAL SCHOOL MANAGER - REVISION 7.4.7
HOLIDAY DATE-RANGE DATABASE FIX
========================================

Resolved error
--------------
  Invalid column name 'HolidayEndDate'.

Affected areas now initialize correctly
----------------------------------------
  * Teaching staff attendance
  * Non-teaching staff attendance
  * Student attendance
  * Student attendance reports and analytics
  * Admin dashboard attendance summary

Cause
-----
Older databases contained SchoolAttendanceHolidays.HolidayDate but did not
contain HolidayEndDate. The previous migration added HolidayEndDate and then
referenced it later in the same SQL command. SQL Server could compile the later
statements against the old table definition before the ALTER TABLE completed.

Correction
----------
AttendanceCalendarService now executes schema creation, column addition,
existing-record backfill, NOT NULL conversion, date-range constraint creation,
and index creation as ordered separate commands. The successful migration is
cached for the current application session, while a failed partial migration
remains safe to retry.

Existing single-day holidays are preserved by setting:
  HolidayEndDate = HolidayDate

The standalone AttendanceCalendar_Database_Update.sql has also been corrected
with deferred dynamic SQL and remains safe to run manually in SQL Server
Management Studio if required.

