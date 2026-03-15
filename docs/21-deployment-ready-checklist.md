# Deployment-Ready Checklist — Fortune Internal Data

Use this before telling anyone the system is ready for staging or production deployment.

## 1. Build Readiness
- [ ] Solution opens successfully in Visual Studio / .NET 8 environment
- [ ] NuGet packages restore successfully
- [ ] Solution builds with no blocking errors
- [ ] Publish profile or deployment method is confirmed

## 2. Database Readiness
- [ ] MySQL 8+ server is available
- [ ] Database created
- [ ] Schema applied via SQL or EF migrations
- [ ] Indexes reviewed for phone number lookup performance
- [ ] Connection string tested successfully

## 3. Authentication Readiness
- [ ] ASP.NET Identity integration completed
- [ ] Login works
- [ ] Logout works
- [ ] Password rules confirmed
- [ ] Lockout policy confirmed
- [ ] Google Authenticator TOTP setup works
- [ ] OTP verification works
- [ ] Privileged roles require 2FA

## 4. Authorization Readiness
- [ ] Superadmin role works
- [ ] Admin role works
- [ ] Manager role works
- [ ] Staff role works
- [ ] Protected menus/pages are role-limited correctly

## 5. Import Readiness
- [ ] CSV import works
- [ ] XLSX import works
- [ ] Invalid phone numbers are rejected correctly
- [ ] Duplicate-in-file detection works
- [ ] Existing-number detection works
- [ ] Preview summary is correct
- [ ] Confirm import inserts only valid new rows
- [ ] Import history is recorded

## 6. Core Functional Readiness
- [ ] Phone number list/search works
- [ ] Phone number edit works
- [ ] WhatsApp status edit works
- [ ] Remark edit works
- [ ] Dashboard summary works
- [ ] User management works for privileged role

## 7. Operational Readiness
- [ ] Upload storage path exists and is protected
- [ ] Logging is enabled
- [ ] Unhandled error behavior is acceptable
- [ ] Backup plan for MySQL is defined
- [ ] Bootstrap superadmin path is disabled after first use

## 8. Environment Readiness
- [ ] HTTPS is enabled
- [ ] Base URL is correct
- [ ] Appsettings / secrets are not hardcoded in repo
- [ ] Environment variables / secret storage are documented
- [ ] Server permissions for file upload directory are correct

## 9. UAT Readiness
- [ ] Test using `samples/import-template.csv`
- [ ] UAT checklist in `docs/08-uat-checklist.md` has been executed
- [ ] Critical defects are fixed
- [ ] User sign-off obtained if required

## 10. Final Release Decision
System should only be marked deployment-ready if:
- [ ] IT can build and run it from clean checkout
- [ ] first Superadmin can access it successfully
- [ ] import preview + confirm flow works end-to-end
- [ ] data can be searched and updated after import
- [ ] no critical auth/import/data-loss risk remains
