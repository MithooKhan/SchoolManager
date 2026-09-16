# Digital School Manager — Professional UI Update

## Version 13 improvements

- Corrected class-wise timetable mentor text to normalize saved whitespace and display one space between `Class Mentor:`, the assigned mentor name, and designation in the printed class header cell.
- Added **Administration > Examinations > Print Result Cards**, a professional result-report workspace that combines First Term, Second Term, and Final/Annual Term marks into one cumulative A4 student result card.
- Added complete-class result-card generation and individual student lookup by Registration No or student phone number, including an exact-result shortcut when only one record matches.
- Added isolated A4 printing with one student result card per page and a **Print / Save as PDF** action that excludes the master page, menus, filters, search results, and scroll containers.
- Added term-wise obtained/maximum totals, percentages, cumulative score and pass status alongside student photograph, identity, class, roll number, Form-B, phone, address, certificate number, and signature areas.
- Added an authenticated database-backed student-image handler so large class result reports load student photographs efficiently without embedding image bytes repeatedly in the HTML.
- Added an administrator-only examination monitor to the dashboard. It uses the latest examination with result entries to show students appeared, pass percentage, classes reported, and the top three students from every reported class with photographs and scores.

## Version 12 improvements

- Corrected the attendance report and analytics date-filter validation so every `out` parameter is assigned on all return paths, resolving Visual Studio compiler error `CS0177` on `toDate`.
- Added a professional class-first Student Attendance workspace with daily date selection, live class coverage, active student loading, Present/Absent/Leave color buttons, quick mark-all actions, and transactional whole-class submission.
- Added an idempotent `StudentAttendance` database structure that keeps one record per student/date, stores the class at the time of marking, records the logged-in system user, and safely updates existing entries when corrections are submitted.
- Added printable class attendance sheets for Previous Month, Current Month, Last 30 Days, or a custom period of up to 31 days. Every active student is displayed with roll number, father name, parent contact, daily P/A/L marks, totals, and attendance percentage.
- Added an attendance parent-reminder workspace. Absent students can receive an SMS through the existing configured gateway or a pre-filled WhatsApp message using the parent contact saved in `Students.ContactNo`; SMS attempts are retained in an auditable outbox.
- Added a class and student Attendance Progress page supporting up to a 366-day annual review, class comparison bars, attendance percentages, high-attendance recognition, configurable short-attendance thresholds, and intervention lists.
- Added a protected Student Attendance monitor to the administrator dashboard with today’s present/marked summary, class-wise 30-day rates, and students below the 75% target.
- Added Students > Student Attendance menu links for marking, printable sheets, and progress analysis, plus `StudentAttendance_Database_Update.sql` for manual database deployment when automatic schema creation is restricted.

## Version 11 improvements

- Corrected `StudentPrintData.aspx` printing. The page now maintains a dedicated unpaged copy of the current filtered result set and opens an isolated A4 landscape print document, so the selected student fields, photos, and every matching record print without the master page, pagination, or scroll container.
- Corrected the `ImageUploadProcessor` namespace aliases so `System.Drawing.Image` no longer conflicts with the ASP.NET WebForms `Image` control during compilation.
- Student registration and student record editing now validate image contents in memory and store the complete photo in `Students.StudentImageData` (`VARBINARY(MAX)`) with content type and original filename metadata. New student photos are never written to `images/students`.
- Added one shared image-processing pipeline for student, teacher, non-teaching staff, and Daak image uploads. It corrects camera orientation and repeatedly resizes/re-encodes large images until the database value is at most 1 MB. Daak PDFs remain unchanged and are validated separately up to 10 MB.
- Added `StudentImage_Database_Update.sql`. On first use, the application safely copies readable legacy student photo files into SQL without deleting the original files.
- Incoming and outgoing Daak documents are now stored in `OfficeDaakDiary.DocumentData` and `OfficeDaakDispatch.DocumentData` instead of `App_Data`. Each saved document includes its content type, size, original filename, and SHA-256 integrity value.
- Existing Version 9/10 Daak files are copied into the database automatically when their original protected files remain readable. The old files are retained as a recovery source and are not deleted automatically.
- The authenticated Daak document handler now streams protected document bytes directly from SchoolDatabase.
- Redesigned both Daak pages with a clearer workspace switcher, visible database-storage status, stronger workflow hierarchy, accessible forms, improved upload guidance, sticky save actions, and more readable searchable registers.
- Redesigned the result certificate with a stronger academic-document hierarchy and an A4 portrait print layout. Compact but readable profile, marks, term totals, cumulative totals, and signature areas are formatted to use the printable page efficiently.

## Version 10 improvements

- Corrected the class-mentor timetable lookup to use `TeachersClasses`, the table used by Class Mentor Management. Every class-wise timetable row now prints the mentor name and sanctioned-post designation directly below the class name in the same cell.
- Removed the Student Result Card QR code and compacted the certificate header. The school name and complete printed header are forced to bold black text with narrower padding.
- Added Final Term prerequisite enforcement. The selected student's complete First Term and Second Term subject results are required before Final Term marks can be entered.
- Final Term cards now load First, Second, and Final Term entries together, show a separate total/percentage/grade for each examination, and calculate an additional cumulative total, percentage, and grade.
- Term matching uses examination names and dates. Use clear names such as `First Term`, `Second Term`, and `Final Term`; the closest matching First and Second Term exams in the preceding 18 months are used.
- Added teacher-identity password recovery using the account username plus the linked teacher CNIC and personal number. Successful recovery resets lockout state and stores an audit event.
- Added an authenticated administrator-transfer workspace. The current administrator must confirm their password, choose another registered teacher, create the replacement credentials, and decide whether the departing administrator account is deactivated.
- Added the `SystemAccountAudit` database table and updated `Login_Database_Update.sql` for recovery and administrator-transfer audit records.

## Version 9 improvements

- Added a new **Office Daak** master-menu section with **Daak Dispatch** and **Daak Diary** pages. Both pages use the existing authenticated management-page protection.
- Added a professional incoming Daak workflow for received date, issuing office, sender reference, subject, category, priority, receiving method, assignment, action deadline, status, description, remarks, and a protected scanned copy.
- Added permanent, concurrency-safe incoming diary numbers in the format `DY-YYYY-00001`. The saved number and date are displayed in a clear diary-stamp panel for entry on the physical letter.
- Added a complete outgoing dispatch workflow for recipient office and address, contact, reference, subject, category, priority, dispatch method, courier tracking, preparer, approving officer, status, description, remarks, and the signed office copy.
- Added permanent, concurrency-safe outgoing dispatch numbers in the format `DSP-YYYY-00001`.
- Added searchable and pageable incoming and outgoing registers with status and date filters, operational summary cards, and authenticated viewing of saved document copies.
- Scanned files are validated as JPG, JPEG, PNG, WebP, or PDF up to 10 MB and stored privately under `App_Data/OfficeDaak`, preventing direct public access.
- Added `OfficeDaakCounters`, `OfficeDaakDiary`, and `OfficeDaakDispatch` database tables with unique register-number and register-search indexes. Both pages apply the idempotent schema update automatically.
- Added `OfficeDaak_Database_Update.sql` for installations where the website account cannot create database objects automatically.

## Version 8 improvements

- Redesigned the Student Result Card header as a clean certificate header with a white print background, official Punjab logo, stronger information hierarchy, and a dedicated QR verification block.
- Forced the complete printed top header, Government caption, certificate title, and school name to render in bold black text so browser print color adjustments cannot change them to brown.
- Added a persistent certificate number for every examination, student, and class combination using the required `GHSS-YYYYMMDD-0000` format.
- Added the `StudentResultCertificates` database table with unique indexes for the result record and certificate number. Existing certificate numbers are reused on every reprint.
- Added a locally generated QR code containing the certificate number, school, examination, student identity, class, roll number, total, percentage, and grade. No internet QR service is required.
- Updated `StudentResults_Database_Update.sql`; the result page applies the same idempotent database upgrade automatically when permitted.

## Version 7 improvements

- Upgraded Fee Collection to use a clear fee month and year. A student can pay in each billing period, while a second submission for the same period is blocked inside a serializable database transaction.
- Saved payments now reopen as a read-only, printable voucher with the original voucher number, collection date, charge amounts, total, remarks, and a clear Paid notice.
- Added an administrator-only outstanding-fee monitor to the dashboard. It lists students whose current class has configured charges but no payment for the selected period; student and parent contact data remains hidden from guests.
- Added individual and bulk parent reminders with an auditable `FeeReminderOutbox` status of Pending, Sent, or Failed. Reminders use `Students.ContactNo` and are never duplicated after a successful delivery for the same student and period.
- Added optional SMS webhook settings in `Web.config`. Configure `SmsGatewayUrl`, `SmsGatewayApiKey`, and `SmsSenderId` for automatic HTTP delivery; without a gateway, generated reminders remain safely queued as Pending.
- Added a professional Student Result Entry page under Administration > Examinations. It loads examinations, classes, students, and the subjects shared by `ExamSubjects` and `ClassSubjects`.
- Added progressive subject entry: after marks are saved, that subject is removed from the available list and appears immediately on the live student result card. A correction action can remove a row and return the subject to the entry list.
- Added a printable result card with student image, name, father name, Form-B, Registration No, class, roll number, address, subject marks, percentage, grades, totals, and signature areas.
- Added idempotent database updates in `FeeCollection_Database_Update.sql` and `StudentResults_Database_Update.sql`; both pages also apply their safe schema updates automatically when permitted.

## Version 6 improvements

- Added a **System** menu with **Login** and **Data Backup** pages.
- Added a secure `SystemUsers` database table linked to `Teachers.teacherid`, with salted PBKDF2 password hashes, active-account status, failed-login tracking, 15-minute lockout, role, creation date, and last-login date.
- Added first-time administrator setup on the Login page. The first administrator must be selected from an existing Teachers record.
- Added forms-authentication and session protection. The dashboard and menu remain available to guests; attempting to open another `.aspx` page redirects to Login with a clear access message.
- Added signed-in user status and Login/Logout controls to the master header.
- Added full SQL Server backup creation with `COPY_ONLY`, checksum, timestamped storage, and `RESTORE VERIFYONLY` validation.
- Added safe restore from a stored or uploaded `.bak` file. The selected backup is checked as a full backup and must identify the configured SchoolDatabase before replacement is allowed.
- Added a stored-backup register and secure backup download action.
- Added `Login_Database_Update.sql` and backup-folder permission guidance under `App_Data/DatabaseBackups`.

## Version 5 improvements

- Expanded the Fee Management menu with **Add Fee Rates** and **Collect Fee** actions.
- Added a professional voucher-style Fee Collection workspace with class filtering and student search by name, Registration No, or Form-B No.
- Loads the selected class's configured fee charges, starts every received amount at zero, and calculates the voucher total automatically.
- Saves each positive charge as a transactional fee collection record with one voucher number, deposit date, amount, remarks, and collection timestamp.
- Added an idempotent `FeeCollection_Database_Update.sql` script. The page applies the same safe update automatically when the database account has permission.
- Added a focused printable fee voucher that excludes the application menu and student-search controls.

## Version 4 improvements

- Rebuilt the Teaching and Non-Teaching Staff Attendance interfaces with professional cards, controls, search panels, and responsive layouts.
- Added filled attendance choices: Present is green, Absent is red, and Casual Leave is blue, with reliable visual synchronization after selection and postback.
- Forced timetable print cells to use solid black borders, black text, and white backgrounds.
- Redesigned StudentRegistrationForm and ManageStudentData as advanced light-green record workspaces with high-contrast headers, workflow shortcuts, improved cards, and modern form controls.
- Corrected custom StudentPrintData field selection and created a table-only landscape print area without scrollbars or page controls.
- Added the official Government of Punjab emblem locally to student and teacher cards for offline use.
- Updated the teacher identity card print layout to the standard 85.6 mm by 54 mm card size with no surrounding A4 page content.

## Version 3 improvements

- Added dark-green, high-contrast page headers to the timetable, student registration, and student record management pages.
- Removed decorative dash and ellipsis characters from the requested timetable selectors, report filters, and student-card controls.
- Corrected StudentPrintData filtering so the selected criteria are applied to the report query.
- Corrected StudentPrintData GridView photos by resolving byte images, saved virtual paths, and missing-image fallbacks through the shared image helper.
- Rebuilt the student identity card with a professional school-branded layout, reliable student photos, clear English controls, and an A4 print arrangement.
- Rebuilt the teacher school card with a premium green-and-gold staff identity layout, locally generated QR verification, additional staff details, print support, and PNG download support.

## Version 2 improvements

- All pages, forms, labels, controls, buttons, grids, and print layouts now use **Times New Roman**.
- Decorative icon characters and fallback glyphs that could appear as unreadable foreign-language symbols were removed while genuine Urdu content was preserved.
- A consistent light-green application background and high-contrast form, label, control, card, and GridView colors are applied across the project.
- Student registration and student-record grids now display the saved student photo, use consistent passport-style cropping, normalize stored image paths, and fall back to `images/noimage.png` when a file is missing.
- Teacher registration and teacher-record grids now display database images with correct PNG, GIF, BMP, or JPEG handling, consistent sizing, and a missing-image fallback.
- Visual Studio now uses portable IIS Express settings on `http://localhost:54966/` instead of the administrator-only Local IIS application path.

## Main improvements

- All page, component, responsive, and print styles are consolidated in `DigitalSchoolManager/SchoolTheme.css`.
- Page styles are scoped by page name so they cannot change the master menu or another page.
- The master page now provides a consistent horizontal navigation bar, school identity header, responsive content area, and footer.
- The dashboard was rebuilt with live SchoolDB summary values, recent admissions, staff attendance, upcoming examinations, and working shortcuts.
- The dashboard fails safely when the database is unavailable and shows a clear connection message instead of sample data.
- The non-teaching attendance page now uses the master page and shared navigation.
- Inline style attributes, remote font/icon dependencies, malformed style markup, and obsolete stylesheet references were removed.
- Print layouts retain print-specific rules while hiding the application header and footer.

## Before running on another computer

Update the `SchoolDB` connection string in `DigitalSchoolManager/Web.config` so its SQL Server instance and database name match that computer.

Version 11 does not require write permission on `App_Data/OfficeDaak` for new documents. Keep the legacy folders available during the first application run only when older Version 9/10 files need to be copied into the database.

Open `DigitalSchoolManager.sln` in Visual Studio normally (administrator mode is not required), restore NuGet packages if requested, build the solution, and run it through IIS Express.
