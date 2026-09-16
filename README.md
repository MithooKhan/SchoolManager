# Digital School Manager — Professional UI Update

## Version 16 revision 7.4.5 — school holiday date ranges

- Replaced the single holiday-date control with Start date and End date controls so one-day events, Eid holidays, summer vacations and winter vacations can be recorded as complete periods.
- Migrated every existing holiday safely as a one-day period by adding `HolidayEndDate` without deleting or renaming the existing `HolidayDate` data.
- Applied the shared holiday calendar to student, teaching-staff and non-teaching-staff attendance. All three marking pages now block attendance on every date inside a holiday period while retaining automatic Sunday closure.
- Added the holiday-period editor and register to Non-Teaching Staff Attendance, including validation, overlap protection, duration display and removal of a complete period.
- Updated teaching and non-teaching absence/leave totals, teacher history, student attendance sheets, printable reports and Excel/report data so saved attendance on a closed date is excluded from working-day totals.
- Improved long-vacation reporting by combining consecutive school-off dates into concise date ranges in the student attendance legend.
- Updated the responsive calendar form for desktop, tablet and mobile layouts and added an explicit stylesheet cache key for deployment.

## Version 16 revision 7.4.4 — official school logo integration

- Replaced the previous shared school emblem with the supplied high-resolution green-and-gold official logo.
- Applied the new logo automatically to the master header, administrator and public dashboards, portal login, funds and budget pages, result and attendance reports, timetables, fee vouchers, student and staff identity cards, leaving and character certificates, school council minutes, and other branded print views.
- Preserved context-specific dimensions for compact navigation, page heroes, cards, A4 reports and print headers while enforcing contained, centred rendering so the emblem is never cropped or distorted.

## Version 16 revision 7.4.3 — School Assets document-link correction

- Replaced the School Assets session-only View/Download URLs with short-lived, tamper-proof access URLs generated while the administrator is viewing the protected asset register.
- The School Resource document handler now accepts either the live administrator session or a valid 30-minute protected link, preventing false Login redirects caused by secondary request/session behavior on some IIS installations.
- Added explicit invalid, expired, missing-file and server-error responses without allowing Forms Authentication to convert handler errors into misleading Login-page redirects.
- Strengthened protected file streaming with private/no-store caching, content-type fallback, filename sanitization and browser content-sniffing protection.

## Version 16 revision 7.4.2 — protected archive and asset document access

- Corrected the Old School Records **View** and **Download** actions. `OldSchoolRecordDocument.ashx` now participates in ASP.NET session state, so the authenticated administrator session is available when protected files are streamed.
- Corrected the same false-login redirect for School Assets receipts and evidence through `SchoolResourceDocument.ashx`.
- Preserved the existing administrator-only authorization checks and database-backed file storage; the fix changes only session availability for the two protected handlers.

## Version 16 revision 7.4.1 — Manage Teachers database correction

- Corrected the Manage Teachers first-load failure that reported `Invalid column name 'IsWorkingAsHead'` three times. The automatic database upgrade now creates and verifies the column in a separate SQL execution batch before any query compiles against it.
- Updated `TeacherHeadAssignment_Database_Update.sql` with the same first-run-safe, idempotent migration for installations where the website database account cannot alter tables.
- Replaced the raw unformatted page error output with a readable, encoded message panel that follows the shared project styling.

## Version 16 revision 7.4 — public media, school-head duty and dashboard redesign

- Extended Public Content Management with safe URL-based video and audio publishing. Administrators can add a player title and a public YouTube, Facebook, Vimeo, SoundCloud, Spotify, or direct HTTPS media-file link without entering iframe or script code.
- Added provider validation, HTTPS enforcement, server-generated embed URLs, responsive media players, and media/provider status in the saved-announcement register. Existing announcements remain unchanged.
- Added the database-backed `IsWorkingAsHead` teacher duty. The Yes/No choice appears in Manage Teachers only while a Principal, Senior Headmaster, or Headmaster post is vacant, prevents more than one acting assignment, and is removed automatically when the condition is unavailable.
- Rebuilt the administrator-dashboard header as an executive command centre with the school emblem, operating status, stronger information hierarchy, six high-frequency workflow shortcuts, responsive cards, and refined operational panels while preserving every existing live metric and server event.
- Added idempotent schema upgrades, final-cascade responsive styling, a new stylesheet cache key, and deployment guidance for this revision.

## Version 16 revision 7.3 — resources, curriculum choices and public dashboard

- Added a professional School Assets register for buildings, land/area, trees, furniture, equipment, electrical installations and other resources, including quantities, purchase/acquisition data, inspections, physical condition, usability, disposal history and printable filtered records.
- Added database-backed asset receipts and evidence with multiple-file upload, automatic image optimization to 1 MB, verified PDFs up to 5 MB, protected viewing and download.
- Added a Laboratory Records workspace for Computer, Science, Chemistry and other labs, with editable lab profiles, incharge/safety information, equipment-level total/working/non-functional quantities, purchase and disposal records, printable inventories and scanned evidence.
- Rebuilt Class Subjects as a curriculum grouping module. Existing assignments remain valid and can now be classified as Compulsory or Optional, limited to Muslim/Non-Muslim/all students, and organized into Faith, Group 2, Group 3 or custom choice groups.
- Updated Student Registration to load the selected class curriculum, apply religion-based eligibility, show compulsory subjects, require one selection from every configured choice group, and save all subject selections transactionally with admission and class placement.
- Added `Religion` and a permanent `StudentSubjectSelections` register through an idempotent database update without replacing the existing Students or ClassSubjects tables.
- Added a dedicated public landing dashboard containing only the principal profile/image, school introduction, history, public announcements, upcoming examinations, and totals for active students, teachers and classes.
- Added an administrator-managed public-content page for principal assignment, school profile text, blog-style posts, cover images, publishing and pinned announcements. The existing detailed Admin Dashboard is now protected and remains available after administrator login.
- Added shared responsive and print styling, menu links, project-file entries, deployment guidance, and the three idempotent database scripts required by the new modules.

## Version 16 revision 7.2 — Old School Records archive correction

- Corrected the Old School Records catalogue query by including its update timestamp in the grouped columns before sorting by that timestamp. SQL Server can now load, search and refresh the archive instead of rejecting the query with the `UpdatedAtUtc` `ORDER BY`/`GROUP BY` error.
- Reviewed the remaining Old School Records queries to confirm that no other aggregate query sorts by an ungrouped archive column.

## Version 16 revision 7.1 — student registration loading correction

- Corrected the Student Registration loading overlay so it activates only after all ASP.NET client validators pass. A validation-cancelled postback can no longer leave the page permanently blurred and blocked.
- Added reliable overlay cleanup after full page loads, browser history restoration, server validation/errors and ASP.NET partial postbacks, plus a 60-second interruption safety reset.
- Improved the saving indicator with an accessible status message and a high-contrast project-matched card.

## Version 16 revision 7 — class-incharge portal, student diary and elections

- Added a dedicated teacher portal using the active teacher's saved cell number as username and personal number as password, with failed-login lockout and optional remembered login.
- Added a class-incharge dashboard showing the assigned class, active students, today’s attendance, timetable subjects, upcoming examinations and weekly diary progress.
- Restricted teacher attendance, result and fee entry to the class assigned in TeachersClasses, with server-side ownership and student-membership checks plus teacher audit identifiers.
- Added Teachers Diary with timetable-scoped class/subject choices, weekly teaching topics, study and lesson plans, assignments, class tests, completed work, due dates and Draft/Published/Delivered visibility.
- Added a read-only Student Profile and Diary search by Form-B, registration number or name, combining the student image and identity with class incharge, attendance, results, upcoming exams, published lessons, assignments and activity achievements.
- Added administrator-managed student activity records for sports, games, elections, quiz competitions, debates, community service and other achievements.
- Added a complete Student Union election system: schedule announcement, required two-or-more candidates per position, Class 10 President/Class 9 Vice President/Class 8 General Secretary eligibility, database-backed optimized symbol images, public candidate list, Form-B voter verification, one vote per student per position, audit trail, live administrator count and explicit result publication.
- Added Portal_Database_Update.sql, StudentUnionElection_Database_Update.sql, complete project entries, responsive project-matched UI, and a Revision 7 deployment/readme guide.

## Version 16 revision 6 — governance and historical records

- Added an SLC-controlled Student Character Certificate register. Certificates use the permanent leaving-certificate identity/photo snapshot, enforce one issue per SLC, store class-incharge and administration remarks, include two signatories, and print as a professional A4 certificate.
- Added **Funds & Accounts > SMC Account** with editable bank details, receipts, approved utilizations, cheque/receipt evidence, running balance and date-range printable statements.
- Extended the shared fund schema safely with the `SMC` fund type without changing existing NSB or FTF transactions.
- Added a database-backed Old School Records Archive for registers, teacher files, accounts files, letters, orders and other historical records. Every entry receives a permanent archive number and supports metadata editing, searching, multiple optimized scans, verified PDFs, protected viewing and download.
- Added **Governance & Records > School Council** with chairman, teacher, parent and society membership; active/inactive term management; editable monthly meeting proceedings; economic, social and welfare decisions; follow-up actions; and professional printable minutes.
- Added idempotent database scripts, explicit project-file entries, responsive shared styling and protected administrator access for all new modules.

## Version 16 revision 3 — monthly expenditure statements

- Added a professional **Funds & Accounts > Monthly Expenditure** workspace based on the supplied school expenditure workbooks.
- Reproduced the official `6202`, `Schedule`, and `STATEMENT OF EXCESSES & SURRENDERS (REVISED ESTIMATES)` forms with A4 landscape print and browser PDF output.
- Added a one-time detailed object/G/L account register seeded with the supplied reference codes and descriptions.
- Added permanent July-to-June monthly statement snapshots, AG Office document/posting-date capture, departmental and AG progressive calculations, and reconciliation variation checks.
- Added a Previous Months workspace for entering opening or historical figures when digital recordkeeping begins partway through a financial year.
- Added finalization controls that require zero departmental/AG variation and complete Schedule of Payment references; administrators can reopen a finalized file for an authorized edit.
- Added searchable saved statements using the required `Monthly Expenditure Statement for the Month of Month Year` naming convention, plus direct Edit Statement and Print / Save PDF actions.
- Added the latest monthly expenditure figure and statement name to the administrator dashboard.
- Added `MonthlyExpenditure_Database_Update.sql` and `MONTHLY_EXPENDITURE_MODULE_README.txt`; the database update is idempotent and is also applied automatically by the new pages.

## Version 16 revision 1 improvements

- Corrected the administrator-dashboard examination ribbon with a consistent dark-green background, white heading/date text, and a high-contrast white-on-green result-entry action.
- Added a **Keep me logged in on this device** option. Remembered logins use a protected 30-day authentication cookie and safely rebuild the teacher-linked server session when required.
- Limited the First/Second Term prerequisite lock for Final Term result entry to Classes 6th, 7th, and 8th. Classes 9th, 10th, 11th, and 12th can enter Final Term results without that lock.
- Added concurrency-safe automatic student registration numbers in the format `GHSS-Maankot-YYYYMMDD-0000`, including an automatic database-column length upgrade and a manual SQL update script.
- Reworked Non-Teaching Vacancy Edit/Delete actions to use dark amber/red surfaces with white text and clear hover/focus states.
- Replaced UI-facing Unicode punctuation and symbols with plain readable text to prevent encoding artifacts in labels, buttons, menus, tables, and print views.

## Version 16 improvements

- Added the supplied GHSS Maankot identity artwork as `images/SchoolLogo.png` and embedded it in the shared application header, reports, print layouts, identity cards, attendance sheets, timetable documents, fee vouchers, and staff documents.
- Updated the master stylesheet cache key to Version 16 and added final-priority Times New Roman, form-control, GridView, dashboard, backup, vacancy-action, and result-summary contrast rules so text remains readable on both light and dark green surfaces.
- Corrected result-card photographs by enabling ASP.NET session state for `StudentImage.ashx`; the authenticated image handler previously received no session and rejected otherwise valid requests.
- Updated result-card printing to wait for student photographs and logos before opening the browser print dialog.
- Removed the `System.ValueTuple` runtime dependency from `PrintTeacherWorkLoad.aspx.cs`, resolving the workload page-load error on the project's .NET Framework 4.7.2 target.

## Version 15 corrections

- Corrected compiler error `CS0165` in `StudentAttendanceReport.aspx.cs` by initializing the optional closed-day label before the short-circuit holiday lookup.
- Aligned `Web.config` runtime targeting with the existing project target of **.NET Framework 4.7.2**, allowing publication for Windows computers that cannot install .NET Framework 4.8.

## Version 14 improvements

- Replaced `Digital School Manager` in every timetable print/export heading with **Government Higher Secondary School Mankot Tehsil Kabirwala District Khanewal** and temporarily applies the same identity to the browser print-document title.
- Updated Student Attendance class cards to load the assigned class mentor from `TeachersClasses` and display the normalized class name and mentor name with exactly one separating space.
- Added automatic Sunday detection to Student Attendance and Teaching Staff Attendance. A Sunday is shown as a school-off day and attendance loading/submission is blocked for that date.
- Added one shared SQL-backed custom holiday calendar on both attendance pages. Administrators can save or remove a named date such as Eid, Independence Day, Defence Day, or another school closure; the change applies to both student and teacher attendance.
- Updated printable student attendance sheets to retain every calendar date, mark Sundays with `S`, named holidays with `H`, show the full holiday name in the date heading, and print a school-off-date legend.
- Updated teaching staff history, print, and Excel data so Sundays and named holidays appear as explicit **School Closed** records instead of disappearing from the selected report range.
- Moved the complete Version 13 Student Result Reports/dashboard summary stylesheet to the final cascade position, added final scoped page rules, and corrected the report print stylesheet loading priority for a consistent professional interface.
- Added `AttendanceCalendar_Database_Update.sql`; the attendance pages also apply the same idempotent schema automatically when the configured SQL account has permission.

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
