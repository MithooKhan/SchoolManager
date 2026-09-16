DIGITAL SCHOOL MANAGER - REVISION 6 GOVERNANCE AND RECORDS
==========================================================

This revision adds four SQL Server-backed modules:

1. Student Character Certificate
   - Available after a School Leaving Certificate has been issued.
   - Uses the permanent SLC identity and student photograph snapshot.
   - Stores remarks, signatories and a unique GHSS-CC certificate number.
   - Provides a professional A4 print view.

2. SMC Account
   - Bank profile, receipts, utilizations, evidence images, running balance and printable period statement.
   - Uses the existing secure Fund Management tables with the new SMC fund type.

3. Old School Records Archive
   - Catalogues registers, teacher files, accounts files, letters, orders and other historical records.
   - Stores multiple optimized scanned images or verified PDF files directly in SQL Server.
   - Images are optimized to 1 MB; each PDF may be up to 5 MB.

4. School Council
   - Maintains chairman, teacher, parent and society membership.
   - Records monthly agendas, attendees, economic decisions, social decisions, welfare decisions and follow-up actions.
   - Meeting minutes can be reviewed, edited and printed.

DATABASE SETUP
--------------
The pages create/upgrade their required tables on first authorized use. For controlled production deployment, a database administrator may run these scripts in this order:

1. SchoolLifecycle_Database_Update.sql
2. CharacterCertificate_Database_Update.sql
3. FundManagement_Database_Update.sql
4. OldSchoolRecords_Database_Update.sql
5. SchoolCouncil_Database_Update.sql

No existing school, student, staff, fee, attendance, result, timetable, budget or Daak records are removed.
