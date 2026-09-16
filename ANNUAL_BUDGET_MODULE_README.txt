DIGITAL SCHOOL MANAGER - ANNUAL BUDGET MODULE
================================================

Reference format
----------------
The module follows the form names and data structure in the supplied
"Budget Estimate 2024-25 GHS Mankot.xls" workbook:

1. Front 414
2. Summary 414
3. 414-Pay
4. 414-BDO-3
5. 414-BDO-4 (Allow)
6. 414-BDC-2
7. 414-BDC-3
8. 414-BDC-4
9. 414-BDC-5
10. 414-BM-10

First use
---------
1. Open Funds & Accounts > Annual Budget > Budget Master Setup.
2. Review allowance names/codes and the seeded BPS 1-20 pay scales.
3. Assign permanent codes to every teaching and non-teaching post.
4. Open Develop Annual Budget and create the July-June budget file.
5. Enter staff basic pay, increment data, monthly allowances, and other heads.
6. Open Budget Forms and PDF, search the budget name, and print either one
   form or the complete budget. Select "Save as PDF" in the print dialog.

Database update
---------------
The pages automatically apply the idempotent
BudgetManagement_Database_Update.sql script. It can also be run manually in
SQL Server Management Studio if the web application account cannot create
tables. Existing school data is not deleted or replaced.

Persistence
-----------
Allowance, pay-scale, and post-code setup is reusable across years. Each
annual budget stores a permanent snapshot of staff, vacancies, pay figures,
allowances, recruitment plans, and object-head estimates. A later change to
the staff register does not overwrite historical budget amounts.

Printing
--------
Budget forms are formatted for A4 landscape with black text, black table
borders, school identification, signatures, and fixed page breaks. The
browser print dialog creates the final PDF without requiring a PDF component
or software installation on the server.
