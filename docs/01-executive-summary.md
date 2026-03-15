# Executive Summary

## Project Name
Fortune Internal Data

## Project Objective
Fortune Internal Data is an internal web-based system designed to replace spreadsheet-based phone number storage with a secure, scalable, and auditable platform. The system will allow authorized users to upload, validate, review, store, search, and maintain more than 3,000,000 phone number records efficiently.

## Primary Business Goals
- Replace Microsoft Excel as the primary data store.
- Reduce duplicate records through validation before import.
- Improve operational control through centralized role-based access.
- Secure user access with password authentication and Google Authenticator two-factor authentication.
- Provide a clean codebase and deployment package suitable for GitHub-based handover to an internal .NET team.

## Core Capabilities
- Secure login with Google Authenticator (TOTP).
- Role-based back office access for Superadmin, Admin, Manager, and Staff.
- Excel/CSV bulk upload with pre-import validation.
- Preview of new, existing, invalid, and duplicate-in-file records before confirmation.
- Master data storage for phone numbers.
- Manual update of phone status, WhatsApp status, and remark.
- Dashboard and import history for operational visibility.
- Activity logging for auditability.

## Recommended Technology Stack
- ASP.NET Core MVC
- MySQL 8+
- Entity Framework Core
- ASP.NET Identity
- TOTP-based 2FA compatible with Google Authenticator
- GitHub for version control and handover

## Data Rules
- Phone number stored as text only.
- Allowed length: 10 to 14 digits.
- Digits only; no spaces and no symbols.
- Duplicate comparison based on normalized text value.

## MVP Outcome
The MVP should provide a professional, maintainable internal platform that is ready for staging deployment and practical handover to the IT team for long-term ownership.
