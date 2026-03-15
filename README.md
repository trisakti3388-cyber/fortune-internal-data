# Fortune Internal Data

Fortune Internal Data is a .NET 8 + MySQL internal web application starter repository for managing large-scale phone number data without relying on Excel as the main storage tool.

This repository is intended for the IT team to take over, complete quickly, and run in a proper internal environment.

## Main Goal
Replace spreadsheet-based phone number storage with a structured internal system that supports:
- secure login
- Google Authenticator 2FA
- role-based access
- CSV/XLSX bulk upload
- duplicate detection before insert
- import preview and confirm flow
- phone number search and editing
- import history and audit support

## Tech Stack
- ASP.NET Core MVC (.NET 8)
- MySQL 8+
- Entity Framework Core
- ASP.NET Identity
- Google Authenticator compatible TOTP

## Important Reality
This repository is a **strong starter scaffold and handover package**, not a finished zero-code production application.

Your IT team should be able to:
- open the solution,
- restore packages,
- wire configuration,
- finish the remaining implementation pieces,
- and get it running quickly.

## Start Here
IT should read these files first:
1. `docs/19-final-it-handover-and-operations.md`
2. `docs/20-it-action-checklist.md`
3. `docs/21-deployment-ready-checklist.md`
4. `docs/15-repository-map.md`
5. `docs/16-implementation-status.md`
6. `docs/09-implementation-notes-for-it.md`

## Project Structure
### Root
- `FortuneInternalData.sln` - solution file
- `global.json` - .NET SDK target
- `docs/` - handover and implementation docs
- `samples/` - sample import file
- `src/` - source code

### Source Projects
- `src/FortuneInternalData.Web` - MVC web app
- `src/FortuneInternalData.Application` - application services/contracts
- `src/FortuneInternalData.Domain` - entities/constants
- `src/FortuneInternalData.Infrastructure` - persistence/repositories/import/identity scaffolding

## What IT Receives
- business and technical documentation
- MySQL schema draft
- .NET solution structure
- starter application scaffold
- identity and 2FA design guidance
- import workflow design
- role model
- handover checklists

## Quick Start for IT
### 1. Prepare environment
Install:
- .NET 8 SDK
- MySQL 8+
- Visual Studio 2022 or equivalent C# tooling

### 2. Open the solution
Open:
- `FortuneInternalData.sln`

Then:
- restore NuGet packages
- build the solution

### 3. Prepare database
Use one of these approaches:
- apply the drafted SQL from `docs/04-database-schema-mysql.sql`
- or finalize EF Core model and generate migrations

### 4. Configure application
Create the required appsettings/environment config with at least:
- MySQL connection string
- upload storage path
- application/TOTP issuer name
- base URL
- logging settings

### 5. Bootstrap first Superadmin
Recommended environment variables:
- `BOOTSTRAP_SUPERADMIN_EMAIL`
- `BOOTSTRAP_SUPERADMIN_PASSWORD`
- `BOOTSTRAP_SUPERADMIN_NAME`

Then run one-time bootstrap logic and require first user to complete password change and 2FA setup.

### 6. Finish critical implementation
IT still needs to complete:
- ASP.NET Identity wiring
- login/logout flow
- Google Authenticator TOTP setup and verification
- EF Core migrations
- CSV parser implementation
- XLSX parser implementation
- import preview/confirm flow
- dashboard queries
- user management
- logging and production hardening

### 7. Run and test
Use sample file:
- `samples/import-template.csv`

Confirm these work:
- login
- 2FA
- phone number list/search/edit
- import preview
- confirm import
- import history

## The 3 Main IT Jobs
To keep the implementation simple, treat the system as 3 main jobs:

### Job 1 — Import Intake
- upload file
- parse rows
- normalize phone numbers
- validate structure
- mark duplicate rows in file
- create import preview data

### Job 2 — Validation and Confirm Import
- compare against existing records
- classify rows
- show preview result
- insert valid new rows only
- record activity log

### Job 3 — Master Data and Administration
- search/edit phone records
- manage users and roles
- review dashboard/import history
- operate the system daily

## Most Important Reference Files
- `docs/04-database-schema-mysql.sql`
- `docs/08-uat-checklist.md`
- `docs/09-implementation-notes-for-it.md`
- `docs/11-bootstrap-superadmin-guide.md`
- `docs/12-import-service-design.md`
- `docs/13-identity-and-2fa-design.md`
- `docs/15-repository-map.md`
- `docs/16-implementation-status.md`
- `docs/19-final-it-handover-and-operations.md`
- `docs/20-it-action-checklist.md`
- `docs/21-deployment-ready-checklist.md`

## Final Summary
This repository should save IT from redesigning the system from zero.

It gives the team a clear .NET + MySQL starting point, documented business rules, a defined import model, identity direction, and enough project structure to move quickly toward a runnable internal system.
