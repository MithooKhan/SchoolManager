Digital School Manager - Revision 4: Accounts, Lifecycle and Daak
================================================================

Database setup
--------------
The pages attempt to create their required tables automatically. If the IIS
database identity cannot run DDL, execute these scripts once in SchoolDatabase:

1. FundManagement_Database_Update.sql
2. SchoolLifecycle_Database_Update.sql
3. OfficeDaak_Database_Update.sql

Delivered functions
-------------------
- Editable NSB and FTF bank profiles: bank, branch code/address and IBAN.
- Bell timetable high-contrast actions and isolated, single-copy A4 printing.
- Teaching/non-teaching service status, transfer evidence and vacancy updates.
- Inactive teachers are removed from mentorship, timetable and system access.
- Permanent School Leaving Certificate register with one-page A4 printing.
- Multi-page incoming/outgoing Daak scans and PDF copies stored in SQL Server.
- Daak register metadata can be edited; archived pages can be added, viewed,
  downloaded or deleted. Images are optimized below 1 MB; PDFs are capped at
  5 MB each.
- The restore page no longer applies a fixed application-level size rejection
  to uploaded .bak files. ASP.NET and IIS request limits are raised to their
  platform maximums; server disk space,
  request infrastructure and SQL Server still determine the practical limit.

Compatibility
-------------
The project remains targeted to .NET Framework 4.7.2 and SQL Server Express.
Open DigitalSchoolManager.sln from a normal Visual Studio session. Restore the
NuGet packages if Visual Studio requests it, then verify the SchoolDB connection
string in Web.config for the target computer.

Compiler hotfix
---------------
- Corrected the lifecycle-service namespace reference used by the non-teaching
  staff attendance page.
- Corrected monthly expenditure report-header conversion from DataRow to
  DataTable.

Revision 5 interface update
---------------------------
- Redesigned the School Leaving Certificate workspace and printable A4
  certificate. The selected student's database photograph is shown in the
  entry summary, stored as a certificate snapshot, and printed on the official
  certificate.
- Redesigned Staff Service Status for teaching and non-teaching employees with
  clearer Active / Former states, departure evidence, editing, reactivation,
  vacancy recalculation, and operational-assignment guidance.
- Added an Official Staff Cards menu item. Cards may be prepared for one staff
  member or every active staff member, for teaching staff, non-teaching staff,
  or both categories. Single-card and A4 multi-card print layouts are included.
- Bell Timetable Edit buttons use light green with black text; Delete buttons
  use red with black text for consistent, accessible contrast.
