DIGITAL SCHOOL MANAGER - VERSION 16 REVISION 7.4
================================================

REVISION 7.4.1 MANAGE TEACHERS CORRECTION
-----------------------------------------
The first-load IsWorkingAsHead database update now runs in separate SQL compilation
batches. This removes the repeated "Invalid column name 'IsWorkingAsHead'" message
when ManageTeachersData.aspx upgrades an existing Teachers table for the first time.
The manual TeacherHeadAssignment_Database_Update.sql script uses the same safe logic.

PUBLIC AUDIO AND VIDEO
----------------------
Public Content Management now accepts a public HTTPS media link, media type and player
title for each announcement. Supported players are:

- Video: YouTube, Facebook, Vimeo, or a direct HTTPS MP4/WebM/OGV file.
- Audio: SoundCloud, Spotify, or a direct HTTPS MP3/WAV/OGG/M4A file.

The editor accepts URLs only. It does not accept iframe or script code. The server
validates the provider and creates the safe player URL. A Facebook or other provider
post must be publicly accessible for visitors to play it.

WORKING AS SCHOOL HEAD
----------------------
Manage Teachers displays "Is this teacher currently working as Head?" only when the
vacancy register reports a vacant Principal, Senior Headmaster or Headmaster post.
Selecting Yes clears that duty from any previous teacher so only one current acting
assignment exists. Teachers appointed to the formal school-head post do not require
this flag.

ADMIN DASHBOARD
---------------
The dashboard now uses an executive command-centre header, school identity, live
database indicator, responsive operational panels and direct shortcuts for admission,
attendance, fee collection, results, public publishing and backup. Existing metrics,
fee reminders, fund balances, attendance summaries and result summaries are preserved.

DATABASE DEPLOYMENT
-------------------
Both affected pages apply their idempotent upgrades automatically when the website
database account has ALTER/CREATE permission. For restricted production accounts, run:

- PublicDashboard_Database_Update.sql
- TeacherHeadAssignment_Database_Update.sql

Run the scripts against SchoolDatabase, then rebuild the application and refresh the
browser once so the new SchoolTheme.css cache version is loaded.
