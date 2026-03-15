# IT Action Checklist — Fortune Internal Data

Use this checklist if your goal is simple: **take the repository, make it build, connect MySQL, and get the system running quickly**.

## A. Open and Verify Project
- [ ] Open `FortuneInternalData.sln`
- [ ] Confirm .NET SDK version from `global.json`
- [ ] Restore all NuGet packages
- [ ] Build the solution
- [ ] Fix any missing references/packages if needed

## B. Prepare Database
- [ ] Create MySQL database for Fortune Internal Data
- [ ] Review `docs/04-database-schema-mysql.sql`
- [ ] Decide whether to use SQL schema directly or EF Core migrations
- [ ] Configure connection string in appsettings/environment config
- [ ] Confirm application can connect to MySQL successfully

## C. Finish Authentication
- [ ] Finalize ASP.NET Identity integration
- [ ] Configure authentication cookies/session
- [ ] Implement login/logout flow
- [ ] Implement role assignment for Superadmin/Admin/Manager/Staff
- [ ] Implement Google Authenticator TOTP setup and verification

## D. Bootstrap First Admin
- [ ] Configure one-time bootstrap environment variables
- [ ] Create first Superadmin account
- [ ] Force password reset if temporary password is used
- [ ] Force 2FA setup for privileged user
- [ ] Disable bootstrap path after first use

## E. Finish Core Phone Data Module
- [ ] Implement phone number list page
- [ ] Implement search and filter
- [ ] Implement edit page for phone status / WhatsApp status / remark
- [ ] Confirm data rules: 10–14 digits, text only, digits only

## F. Finish Import Workflow
- [ ] Implement CSV parser with CsvHelper
- [ ] Implement XLSX parser with ClosedXML or EPPlus
- [ ] Save uploaded files to protected storage
- [ ] Create import batch and import batch rows
- [ ] Detect duplicate rows in file
- [ ] Detect existing numbers in database
- [ ] Implement preview summary page
- [ ] Implement confirm import transaction
- [ ] Insert valid new rows only
- [ ] Write activity log for import actions

## G. Finish Admin / Audit Features
- [ ] Implement user management pages
- [ ] Implement dashboard summary queries
- [ ] Implement import history page
- [ ] Implement activity log view if included in scope

## H. Make It Deployable
- [ ] Add logging and error handling
- [ ] Confirm upload path permissions
- [ ] Confirm HTTPS config in staging/production
- [ ] Test sample file: `samples/import-template.csv`
- [ ] Run UAT checklist from `docs/08-uat-checklist.md`

## I. Minimum Go/No-Go Criteria
Do not hand over as “runnable” until all of these work:
- [ ] solution builds
- [ ] DB connects
- [ ] Superadmin can log in
- [ ] 2FA works
- [ ] phone list works
- [ ] import preview works
- [ ] confirm import works
- [ ] imported rows can be searched and edited
