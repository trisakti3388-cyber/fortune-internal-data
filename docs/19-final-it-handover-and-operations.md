# Final IT Handover Guide — Fortune Internal Data

## 1. Purpose
This document is written only for the internal IT team.

Goal: make it easy for IT to take the repository, upload it to GitHub or a server, open it in Visual Studio, complete any missing package restore/configuration work, and run the system as quickly as possible using **.NET 8 + MySQL 8+**.

This project is an internal web application to replace Excel-based phone number storage with a proper database-backed system for phone number upload, validation, search, editing, and administration.

---

## 2. What IT Is Receiving
The repository already contains:
- business specification
- technical architecture
- MySQL schema draft
- role/permission matrix
- import workflow design
- identity and 2FA design
- deployment notes
- bootstrap superadmin guide
- solution structure for .NET
- starter code scaffolding for web/application/domain/infrastructure layers

Main project folder:
- `fortune-internal-data/`

Main solution file:
- `fortune-internal-data/FortuneInternalData.sln`

Main documentation to read first:
1. `README.md`
2. `docs/15-repository-map.md`
3. `docs/16-implementation-status.md`
4. `docs/09-implementation-notes-for-it.md`
5. `docs/12-import-service-design.md`
6. `docs/13-identity-and-2fa-design.md`

---

## 3. Expected Stack
IT should implement and run this system using:
- **ASP.NET Core MVC (.NET 8)**
- **MySQL 8+**
- **Entity Framework Core**
- **ASP.NET Identity**
- **Google Authenticator compatible TOTP**

Recommended NuGet packages:
- `Pomelo.EntityFrameworkCore.MySql`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.Identity.UI`
- `CsvHelper`
- `ClosedXML` or `EPPlus`
- `Otp.NET`
- `QRCoder`

---

## 4. Business Scope IT Should Implement
### Core modules
1. Authentication and authorization
2. User management
3. Phone number master data management
4. Bulk import preview and confirm flow
5. Dashboard
6. Import history
7. Activity log

### Main roles
- Superadmin
- Admin
- Manager
- Staff

### Core functions
- login with password
- 2FA with Google Authenticator
- upload CSV/XLSX file
- validate phone numbers before import
- detect duplicates in file
- detect existing numbers in database
- preview import results
- confirm import of valid new rows only
- search, filter, and edit phone-number records
- track import history and user activity

---

## 5. Core Data Rules
IT should enforce these rules consistently in parser, service, validation layer, and database constraints where practical:
- phone number stored as **text**, not numeric
- allowed length: **10 to 14 digits**
- digits only
- no spaces
- no symbols
- duplicate comparison based on normalized phone-number text

---

## 6. Repository Structure
### Root
- `FortuneInternalData.sln`
- `global.json`
- `README.md`
- `docs/`
- `samples/`

### `src/FortuneInternalData.Web`
MVC entry project:
- startup wiring
- controllers
- views
- security policies
- dependency injection extensions

### `src/FortuneInternalData.Application`
Application layer:
- DTOs
- interfaces
- services
- validators

### `src/FortuneInternalData.Domain`
Domain layer:
- entities
- role/status/import constants

### `src/FortuneInternalData.Infrastructure`
Infrastructure layer:
- DbContext
- repositories
- file storage service
- import parser service
- identity scaffold

---

## 7. What Is Already Prepared vs What IT Must Finish
### Already prepared
- repository structure
- solution structure
- business and architecture documentation
- starter entities and constants
- starter DTOs and view models
- starter repository and service contracts
- starter controllers and views
- authorization scaffold
- import workflow scaffold
- identity direction and 2FA design
- MySQL schema draft

### IT still needs to finish
- final package restore and build verification
- real ASP.NET Identity integration
- real authentication cookie/session setup
- login and logout flow
- TOTP secret generation and verification
- QR rendering for authenticator setup
- EF Core migrations
- MySQL connection wiring
- actual repository query implementation
- dashboard queries
- CSV parser implementation
- XLSX parser implementation
- import preview logic
- import confirm transaction logic
- bootstrap superadmin execution path
- error handling, logging, and production hardening

So the handover should be understood as: **well-prepared starter repository + implementation blueprint**, not “production-complete code.”

---

## 8. The 3 Main Jobs IT Should Build
To make ownership simple, IT should think of the system as 3 main jobs/workflows.

### Job 1 — Import Intake
**Purpose:** receive uploaded file and stage the rows for validation.

IT should implement:
- upload endpoint/page
- save file to protected storage
- create import batch record
- parse CSV/XLSX rows
- normalize phone numbers
- validate row structure
- detect duplicate rows inside the uploaded file
- save row-level result into import batch detail storage

Expected output:
- import preview summary showing:
  - new rows
  - existing rows
  - invalid rows
  - duplicate-in-file rows

### Job 2 — Validation and Confirm Import
**Purpose:** let user review results, then insert only valid new rows into master table.

IT should implement:
- preview page and summary query
- existing-number lookup in MySQL
- row result statuses
- confirm button/endpoint
- transaction around confirm process
- bulk insert of valid new rows only
- write activity log for auditability

Expected output:
- new phone numbers inserted safely
- duplicate and invalid rows blocked
- full import history preserved

### Job 3 — Master Data and Administration
**Purpose:** allow business users to manage the stored data after import.

IT should implement:
- phone-number list/search/filter page
- edit page for phone status, WhatsApp status, and remark
- user management for privileged roles
- dashboard summary counts
- import history view
- activity log view if included in MVP

Expected output:
- business team can use the system daily without returning to Excel
- admin team can control access and review activity

---

## 9. Recommended IT Execution Order
### Phase 1 — Open and run solution
1. Open `FortuneInternalData.sln`
2. Restore NuGet packages
3. Confirm target SDK in `global.json`
4. Build solution
5. Fix any missing package references or namespace issues

### Phase 2 — Wire database and Identity
1. Configure MySQL connection string
2. Add Entity Framework Core migrations
3. Apply database migration or align with SQL schema
4. Finalize ASP.NET Identity integration
5. Seed or bootstrap first Superadmin user

### Phase 3 — Make login usable
1. Implement login page flow
2. Implement logout
3. Implement TOTP setup page
4. Implement QR generation
5. Implement OTP verification page
6. Enforce 2FA for privileged roles

### Phase 4 — Make core business flow usable
1. Implement phone number list/search/edit
2. Implement CSV parser
3. Implement XLSX parser
4. Implement upload preview flow
5. Implement confirm import flow
6. Implement import history

### Phase 5 — Make handover-ready build
1. Validate role-based authorization
2. Add logging and error handling
3. Confirm sample import works
4. Test bootstrap superadmin flow
5. Run UAT checklist
6. Publish build to staging

---

## 10. Quick Start for IT
If IT wants the shortest path to first runnable result, do this:

### Step A — Prepare environment
Install:
- .NET 8 SDK
- MySQL 8+
- Visual Studio 2022 or VS Code with C# tooling

### Step B — Open repository
- open `FortuneInternalData.sln`
- restore NuGet packages
- inspect `README.md`

### Step C — Create database
Option 1:
- create database manually in MySQL
- use the drafted SQL schema in `docs/04-database-schema-mysql.sql`

Option 2:
- finalize EF Core model
- generate migrations
- apply migrations

### Step D — Configure appsettings
IT should create environment config with at least:
- MySQL connection string
- upload storage path
- TOTP issuer/app name
- base URL
- logging settings

### Step E — Bootstrap first Superadmin
Recommended environment variables:
- `BOOTSTRAP_SUPERADMIN_EMAIL`
- `BOOTSTRAP_SUPERADMIN_PASSWORD`
- `BOOTSTRAP_SUPERADMIN_NAME`

Then run one-time bootstrap logic and require password reset / 2FA setup on first login.

### Step F — Run application
- run the MVC web project
- login as Superadmin
- complete 2FA setup
- test sample import using `samples/import-template.csv`

---

## 11. Recommended MySQL Notes
IT should optimize the database for very large phone-number volume.

Recommendations:
- use indexed normalized phone-number column
- use unique protection where business rules require it
- keep phone number as VARCHAR/text-style field, not integer
- batch existence checks during import
- avoid per-row DB query during validation
- use transaction during confirm import

If record count will exceed several million rows, IT should also:
- review index design carefully
- test import performance on staging data
- test exact-match search performance

---

## 12. Authentication / 2FA Recommendation
Preferred implementation:
- use ASP.NET Identity as the source of truth for auth
- use role-based authorization through Identity roles
- use Otp.NET for TOTP
- use QRCoder for QR generation

Privileged roles should be forced to enable 2FA.

Avoid:
- custom authentication unless absolutely necessary
- hardcoded credentials in repository
- leaving bootstrap credentials active after first use

---

## 13. Import Design Recommendation
For clean and maintainable implementation, IT should keep this flow:
1. Upload file
2. Save file to protected storage
3. Create import batch
4. Parse rows
5. Normalize and validate rows
6. Detect duplicates in file
7. Detect existing numbers in DB in batches
8. Save import row results
9. Show preview
10. Confirm import
11. Insert valid new rows only
12. Log action

Recommended parser strategy:
- `CsvHelper` for CSV
- `ClosedXML` or `EPPlus` for XLSX
- one parser interface selected by file extension

---

## 14. Minimum Definition of “Runnable” for IT
IT can consider the repository “ready to run” once these are working:
- solution builds successfully
- MySQL connection works
- first Superadmin can be created
- login works
- 2FA works
- phone list page works
- upload preview works
- confirm import works
- imported data can be searched and edited

That is the real first operational target.

---

## 15. Suggested Final Handover Message to IT
Use this message if needed:

> This repository is the approved starter implementation for Fortune Internal Data using .NET 8 and MySQL. Please treat it as a professional scaffold intended to let IT move quickly into a runnable internal system. The main work remaining is package restore/build verification, Identity and 2FA completion, database migrations, import parser implementation, preview/confirm import workflow, and staging deployment. The easiest execution model is to treat the system as 3 main jobs: Import Intake, Validation and Confirm Import, and Master Data/Admin operations.

---

## 16. Most Important Files for IT
- `README.md`
- `docs/04-database-schema-mysql.sql`
- `docs/09-implementation-notes-for-it.md`
- `docs/11-bootstrap-superadmin-guide.md`
- `docs/12-import-service-design.md`
- `docs/13-identity-and-2fa-design.md`
- `docs/15-repository-map.md`
- `docs/16-implementation-status.md`
- `samples/import-template.csv`

---

## 17. Plain-English Summary
This handover is meant to reduce thinking time for IT.

Your IT team should not need to redesign the system from zero. They should be able to:
- open the solution,
- wire MySQL,
- restore packages,
- finish the scaffolded implementation,
- run the system,
- and continue enhancement from a structured baseline.

If IT follows this document plus the referenced docs already included in the repository, they should have a clear path to getting the project running quickly in a proper .NET + MySQL environment.
