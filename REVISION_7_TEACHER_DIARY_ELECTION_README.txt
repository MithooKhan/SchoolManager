DIGITAL SCHOOL MANAGER - REVISION 7
Teacher/Class-Incharge Portal, Student Profile and Student Union Election
========================================================================

STARTUP
-------
1. Open DigitalSchoolManager.sln in Visual Studio.
2. Confirm the SchoolDB connection string in Web.config.
3. Run the website. New Revision 7 tables are created automatically.
4. If the website SQL identity cannot create tables, run:
   - Portal_Database_Update.sql
   - StudentUnionElection_Database_Update.sql
   Existing Student Attendance and Student Results update scripts remain applicable.

TEACHER LOGIN
-------------
Page: PortalLogin.aspx
Username: contactno from the active Teachers row.
Password: personalNo from the same active Teachers row.
After five failed attempts, that login key is locked for 15 minutes.
The "keep me signed in" option creates a 30-day protected forms-authentication ticket.

CLASS-INCHARGE SECURITY
-----------------------
TeachersClasses is the authoritative class-incharge assignment.
A teacher can mark attendance, enter examination results and collect fees only for
the assigned class. Server-side checks are applied to class selection and saving;
changing a URL or posted control value does not grant another class.
Administrators keep full system access.

TEACHERS DIARY
--------------
Page: TeacherDiary.aspx
Class and subject choices come only from the logged-in teacher's TimeTable rows.
Entries contain week, lesson date, upcoming topics, study plan, lesson plan,
assignment, class test, work done, due date and status.
Draft is teacher-private. Published and Delivered entries appear in StudentProfile.aspx.

STUDENT PROFILE
---------------
Page: StudentProfile.aspx
Search by Form-B number, registration number, or name.
The page is read-only for students and combines identity, image, class incharge,
attendance progress, examination results, upcoming exams, Teachers Diary work,
assignments and co-curricular activities. Only an authenticated administrator sees
the activity-entry controls.

STUDENT UNION ELECTION
----------------------
Administrator: StudentUnionElection.aspx
Public ballot: StudentUnionVoting.aspx

Required positions and eligibility:
- President: Class 10
- Vice President: Class 9
- General Secretary: Class 8

At least two published candidates per position are required before the administrator
can publish candidates or open voting. Election symbol images are optimized to a
maximum of 1 MB and stored in SQL Server.

Students find their active record and unlock the ballot with the exact Form-B number.
A unique database index permits only one vote per student for each position.
Results remain administrator-only until election status becomes Results Published.
Election actions are recorded in StudentUnionElectionAudit.

SECURITY NOTE
-------------
Teacher and student identifiers requested for this school workflow are existing
identity fields, not user-chosen passwords. Protect access to the school network and
keep those fields confidential. For internet deployment, use dedicated password
hashes and HTTPS before allowing external access.
