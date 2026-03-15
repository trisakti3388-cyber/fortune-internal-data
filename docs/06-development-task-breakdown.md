# Development Task Breakdown

## Phase 0 - Project Setup
1. Create GitHub repository.
2. Initialize ASP.NET Core MVC solution.
3. Add layered project structure or simplified MVC-first structure.
4. Configure MySQL provider and EF Core.
5. Establish environment configuration strategy.
6. Define coding standards and branching approach.

## Phase 1 - Authentication and Authorization
1. Configure ASP.NET Identity.
2. Add role definitions: Superadmin, Admin, Manager, Staff.
3. Implement login and logout.
4. Implement Google Authenticator TOTP setup.
5. Implement OTP verification flow.
6. Add role-protected navigation and controller authorization.
7. Add user management for Superadmin.

## Phase 2 - Domain and Persistence
1. Create domain entities.
2. Create DbContext and mappings.
3. Add migrations.
4. Add indexes and constraints.
5. Add seed/bootstrap strategy for first Superadmin.

## Phase 3 - Phone Data Management
1. Build phone data list page.
2. Implement server-side pagination.
3. Implement exact search by phone number.
4. Implement filters for status, WhatsApp status, upload date, modified date.
5. Implement edit modal/page.
6. Update modified date and activity logs on edit.

## Phase 4 - Import Module
1. Build upload page.
2. Add CSV/XLSX parsing service.
3. Implement phone number validation rules.
4. Implement duplicate-in-file detection.
5. Implement existing-in-database detection.
6. Save validation results to import batch tables.
7. Build preview page.
8. Implement confirm import transaction and bulk insert.
9. Implement cancel import flow.

## Phase 5 - Dashboard and History
1. Build dashboard summary queries.
2. Build recent uploads widget.
3. Build recent updates widget.
4. Build import history list page.
5. Build import detail page.

## Phase 6 - Audit and Hardening
1. Add activity logging service.
2. Add login lockout and password policy.
3. Add CSRF and validation checks.
4. Add error handling and logging.
5. Add file upload size and type restrictions.

## Phase 7 - QA and Handover
1. Prepare sample import files.
2. Run unit tests for validators and services.
3. Run UAT checklist.
4. Prepare README and setup guide.
5. Prepare deployment notes.
6. Tag release candidate in GitHub.

## Recommended Milestones
- Milestone 1: Login, roles, 2FA
- Milestone 2: Phone data list and edit
- Milestone 3: Upload preview and confirm import
- Milestone 4: Dashboard, history, audit, handover
