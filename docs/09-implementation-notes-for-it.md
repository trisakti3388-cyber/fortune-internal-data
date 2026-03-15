# Implementation Notes for IT Team

## 1. Current Scaffold Status
This repository is a professional starter scaffold, not a finished production build. The project structure, DTOs, entities, interfaces, views, and core documentation are in place to accelerate implementation.

## 2. Priority Implementation Order
1. Replace or align the custom `ApplicationUser` entity with ASP.NET Identity user model strategy.
2. Configure authentication cookies and sign-in flow.
3. Implement Google Authenticator TOTP setup and verification.
4. Add EF Core migrations.
5. Implement repository classes.
6. Implement import parsing for CSV and XLSX.
7. Implement preview, confirm, and history screens with database queries.
8. Add role-based `[Authorize]` attributes and policy checks.
9. Add bootstrap flow for first Superadmin account.
10. Add logging, validation, and production hardening.

## 3. Recommended NuGet Packages
- Pomelo.EntityFrameworkCore.MySql
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.AspNetCore.Identity.UI
- CsvHelper
- ClosedXML or EPPlus
- QRCoder
- Otp.NET
- FluentValidation (optional)

## 4. Identity Recommendation
The cleanest implementation is to use ASP.NET Identity as the source of truth for authentication and role membership. The custom `users` table in the schema can be adapted in one of two ways:

### Option A - Preferred
Use ASP.NET Identity tables for user management and keep business-specific user profile fields as extensions.

### Option B
Map current `users` business table and implement custom auth logic. This is possible but less standard.

Preferred approach for maintainability: Option A.

## 5. Import Logic Recommendation
- Parse file rows into memory in chunks.
- Normalize and validate phone numbers immediately.
- Use a hash set to detect duplicate rows within the uploaded file.
- Query existing phone numbers in batches.
- Persist validation results into `import_batch_rows`.
- On confirm, bulk insert only `new` rows.
- Wrap confirm operation in transaction.

## 6. Performance Notes
- Keep page size modest, e.g. 20 to 100.
- Prefer exact search over wildcard search for large volume.
- Consider background job processing for very large uploads later.
- Monitor index size and query plans in staging.

## 7. Suggested First Working Milestone
The fastest path to a useful staging build is:
- login,
- role protection,
- phone number list,
- edit page,
- CSV upload preview,
- confirm import,
- basic dashboard.

## 8. Quality Expectations
Before production release, ensure:
- audit logs are reliable,
- invalid uploads are handled safely,
- duplicate protection is tested at database and service level,
- 2FA is enforced for privileged roles,
- backup and restore process is documented.
