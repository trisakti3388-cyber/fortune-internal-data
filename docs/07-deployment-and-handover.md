# Deployment and Handover Guide

## 1. Version Control
- Host source code in a dedicated GitHub repository.
- Use a main branch for approved code and a develop branch for active work if needed.
- Protect main branch with pull request review if the team prefers.

## 2. Recommended Environments
- Local development
- Staging / UAT
- Production

## 3. Staging Deployment Recommendation
For initial testing, deploy to a staging server with:
- ASP.NET Core runtime
- MySQL 8+
- Nginx or IIS reverse proxy depending on hosting preference
- Protected uploads directory
- HTTPS enabled

## 4. Configuration Items
Application configuration should include:
- MySQL connection string
- Upload storage path
- Authentication cookie/session settings
- TOTP issuer name
- Logging configuration
- App base URL

## 5. Handover Package Checklist
- Source code in GitHub
- Database schema or EF migrations
- README with setup steps
- appsettings example file
- Sample import template
- Role/permission summary
- Deployment notes
- Admin bootstrap procedure
- UAT checklist

## 6. Suggested UAT Scenarios
1. Login with valid credentials.
2. Setup Google Authenticator and verify OTP.
3. Validate role-based menu visibility.
4. Upload a valid file with mixed results.
5. Confirm only new rows are imported.
6. Search for an imported number.
7. Edit status, WhatsApp status, and remark.
8. Review import history.
9. Create and edit user as Superadmin.
10. Verify invalid phone numbers are rejected.

## 7. Production Readiness Notes
Before production go-live:
- enable regular database backups,
- confirm HTTPS and certificate renewal,
- set secure cookie policies,
- review lockout and password rules,
- review server sizing for import volume,
- monitor database index performance,
- define support ownership inside the IT team.

## 8. Professional Handover Approach
The cleanest handover to IT is:
1. Publish the repository.
2. Walk through docs and schema.
3. Demo the import flow and role model.
4. Give staging access.
5. Let IT review code structure and conventions.
6. Capture final adjustments before production deployment.
