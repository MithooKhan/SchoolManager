DIGITAL SCHOOL MANAGER - REVISION 7.4.6
CURRENT CLASS AND STUDENT PROMOTION
====================================

This revision separates the student's permanent admission record from the
student's live class placement.

DATABASE UPDATE
---------------
StudentClassEnrollment_Database_Update.sql is executed automatically by the
updated student pages. It can also be run manually in SQL Server Management
Studio. The script is safe to run repeatedly.

Students table additions:
  * CurrentClassID (INT, foreign key to Classes.ClassID)
  * CurrentClassEnrollmentDate (DATE)

Permanent history table:
  * StudentClassEnrollmentHistory
  * One row is retained for every admission, migrated placement, and promotion.
  * Previous records are closed and retained; they are never overwritten.

BUSINESS RULES
--------------
1. At registration, AdmissionClass and CurrentClassID are the same, and
   DateofAdmission and CurrentClassEnrollmentDate are the same.
2. AdmissionClass and DateofAdmission are permanent original-admission fields.
3. Promotion changes only CurrentClassID, CurrentClassEnrollmentDate, the live
   StudentClass assignment, and creates a new permanent history row.
4. The Manage Student page does not allow class changes that could bypass the
   history. Use Students > Student Promotion.
5. The School Leaving Certificate snapshots both the original admission class
   and the current/last class, including their relevant dates.

STUDENT PROMOTION PAGE
----------------------
StudentPromotion.aspx supports:
  * individual student promotion;
  * bulk class promotion;
  * separate completed and new academic sessions;
  * a promotion/current-class enrolment date;
  * automatic destination-class roll numbers;
  * permanent per-student enrolment history;
  * recent-promotion audit history.

All students selected in a promotion are processed in one serializable SQL
transaction. If any selected student is no longer in the source class, the
entire batch is rolled back and the administrator is asked to reload the list.

UPDATED PAGES
-------------
  * StudentRegistrationForm.aspx
  * ManageStudentData.aspx
  * StudentPrintData.aspx
  * SchoolLeavingCertificate.aspx
  * DSM.Master (Students menu)

