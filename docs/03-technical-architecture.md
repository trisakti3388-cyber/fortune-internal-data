# Technical Architecture

## 1. Recommended Solution Shape
Build the MVP as an ASP.NET Core MVC application backed by MySQL. This is the most practical architecture for a business-focused internal system that needs rapid delivery, straightforward maintenance, and easy handover to a .NET team.

## 2. Proposed Project Structure

```text
FortuneInternalData.sln
src/
  FortuneInternalData.Web/
    Controllers/
    Views/
    ViewModels/
    wwwroot/
    Areas/
      Identity/
  FortuneInternalData.Application/
    Interfaces/
    Services/
    DTOs/
    Validators/
  FortuneInternalData.Domain/
    Entities/
    Enums/
    Constants/
  FortuneInternalData.Infrastructure/
    Persistence/
    Identity/
    Services/
    Repositories/
docs/
samples/
```

## 3. Layer Responsibilities

### 3.1 Web
- MVC controllers and views
- Authentication UI
- Validation display
- Role-protected pages
- Dashboard and administrative pages

### 3.2 Application
- Import workflow logic
- Phone number normalization and validation
- Duplicate detection logic
- Dashboard aggregation logic
- Authorization-aware services

### 3.3 Domain
- Core business entities
- Enumerations and constants
- Shared validation rules at the domain level

### 3.4 Infrastructure
- Entity Framework Core DbContext
- ASP.NET Identity integration
- MySQL persistence
- File storage for uploaded imports
- Activity logging persistence

## 4. Authentication and Authorization
- Use ASP.NET Identity for user management.
- Use role-based authorization with four roles: Superadmin, Admin, Manager, Staff.
- Enforce two-factor authentication using TOTP compatible with Google Authenticator.
- Require 2FA setup on first successful login where enabled by policy.

## 5. Import Processing Design

### 5.1 Upload Lifecycle
1. User uploads CSV or XLSX file.
2. System stores batch metadata.
3. System parses rows into an intermediate batch table.
4. System validates and classifies each row.
5. User reviews preview summary and detail rows.
6. User confirms import.
7. System inserts new valid rows into master table in bulk.

### 5.2 Why Intermediate Batch Tables Matter
Intermediate batch tables are essential because they:
- support review before commit,
- preserve auditability,
- allow retries and troubleshooting,
- separate validation from final insertion,
- reduce operational risk for large imports.

## 6. Performance Strategy
- Store phone number as a varchar field with strict input validation.
- Create a unique index on PhoneNumber.
- Use pagination for all list pages.
- Use exact-match and indexed filtering wherever possible.
- Process import files in chunks for large uploads.
- Perform bulk insert for confirmed new rows.
- Avoid loading large full datasets into application memory.

## 7. Security Controls
- Password hashing through ASP.NET Identity.
- TOTP secrets stored securely and protected at rest.
- Role-based access checks on every protected endpoint.
- CSRF protection on all authenticated forms.
- Server-side validation for all input and upload workflows.
- Activity logging for sensitive changes.
- Config-driven session timeout and lockout policy.

## 8. File Storage
Uploaded files should be stored in a controlled server path or object storage bucket, depending on deployment target. For MVP staging, local protected storage is acceptable if backed up and access-controlled.

## 9. Deployment Topology
Recommended initial topology:
- 1 application server hosting ASP.NET Core app
- 1 MySQL server or managed MySQL instance
- GitHub repository for source control
- Staging environment before production handover

## 10. Handover Considerations
- Keep naming clear and conventional for .NET teams.
- Use migrations for schema management.
- Provide environment variable guidance and sample configuration.
- Keep business rules documented in docs, not only in code.
