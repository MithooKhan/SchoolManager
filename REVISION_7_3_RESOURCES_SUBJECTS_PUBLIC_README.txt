DIGITAL SCHOOL MANAGER - VERSION 16 REVISION 7.3
================================================

NEW MODULES
-----------
1. Governance & Records > School Assets
   Maintains land, buildings, trees, furniture, equipment, electrical installations,
   purchase details, inspections, usability, disposal and database-backed evidence.

2. Governance & Records > Laboratory Records
   Maintains laboratory profiles and equipment-level quantities, working/non-functional
   status, inspection, disposal, receipts and scanned evidence.

3. Academics > Class Subjects
   Existing assignments are preserved. Each assignment can now be Compulsory or
   Optional, limited by religion, and placed in Faith, Group 2, Group 3 or a custom
   student-choice group. Student Registration displays these choices after class selection.

4. Public Dashboard and Public Announcements
   PublicDashboard.aspx is now the website landing page. Administrators can update the
   principal profile, introduction, school history and announcements from
   Governance & Records > Public Announcements.

DATABASE DEPLOYMENT
-------------------
The pages automatically apply their idempotent SQL updates when the website database
account has ALTER/CREATE permission. For restricted production accounts, run these once
against SchoolDatabase in SQL Server Management Studio:

- SchoolResources_Database_Update.sql
- ClassSubjects_Database_Update.sql
- PublicDashboard_Database_Update.sql

UPLOAD RULES
------------
School resource evidence is stored directly in SQL Server. Images are validated and
optimized to no more than 1 MB. Each PDF evidence file may be up to 5 MB.

FIRST USE
---------
1. Configure class subjects and choice groups before new student admission.
2. Open Public Announcements and choose the active principal/teacher record.
3. Enter the school introduction/history and publish the first announcement.
4. Enter asset and laboratory opening balances/inventories from verified registers.
