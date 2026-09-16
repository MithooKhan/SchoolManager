DIGITAL SCHOOL MANAGER - REVISION 7.4.8
BOARD EXAMINATION TEACHER AND SUBJECT-WISE RESULTS

WHAT WAS ADDED
--------------
1. BoardResultManagement.aspx
   - Maintains permanent result files for Classes 9th, 10th, 11th and 12th.
   - Supports Humanities, Pre-Medical, Pre-Engineering, Computer Science,
     Commerce, General and custom study-group names.
   - Saves the responsible Head Teacher / Incharge as a historical snapshot.
   - Saves teacher and subject-wise appeared, passed and board result figures.
   - Calculates failed students and school pass percentage automatically.
   - Compares each subject result with the entered board percentage.
   - Maintains first, second and third position holders.
   - Provides class, year, examination, teacher and subject search.
   - Allows saved result files and subject rows to be reopened and updated.

2. BoardResultReport.aspx
   - Provides (A) HEAD WISE RESULT.
   - Provides POSITION HOLDER RESULT.
   - Provides (B) TEACHER AND SUBJECT WISE RESULT.
   - Uses A4 landscape print rules and supports browser Print / Save PDF.
   - Shows the official school logo and saved result-year staff snapshots.

3. BoardResult_Database_Update.sql
   - Creates dbo.BoardResultSessions.
   - Creates dbo.TeacherSubjectBoardResults.
   - Creates dbo.BoardResultPositionHolders.
   - Adds validation constraints and indexed permanent record retrieval.

CALCULATION RULES
-----------------
- Failed students = Appeared students - Passed students.
- School pass percentage = Passed students / Appeared students x 100.
- Above Board: School percentage is greater than Board percentage.
- Below Board: School percentage is less than Board percentage.
- Equal to Board: Both percentages are equal after two-decimal calculation.
- Grade-wise totals cannot exceed the total number of passed students.

CLASS COHORT FORMAT
-------------------
- Classes 9th and 11th use registered, current appeared, passed and failed data.
- Classes 10th and 12th also retain the previous-class appeared cohort.
- Registered year, previous result year and current result year are preserved
  with the result file so old reports do not change with current school data.

DATABASE INSTALLATION
---------------------
The page attempts to create the new tables automatically when an administrator
opens it. If the website database account cannot alter the database, execute
BoardResult_Database_Update.sql once against SchoolDatabase in SQL Server
Management Studio.
