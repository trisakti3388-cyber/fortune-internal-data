# Bootstrap Superadmin Guide

## Objective
Create the first Superadmin account securely so the system can be administered after deployment.

## Recommended Approaches

### Option A - One-time bootstrap command
Create a one-time command or startup task that:
1. checks whether any Superadmin exists,
2. creates the first Superadmin from secure environment variables,
3. forces password change and 2FA setup on first login.

Recommended environment variables:
- `BOOTSTRAP_SUPERADMIN_EMAIL`
- `BOOTSTRAP_SUPERADMIN_PASSWORD`
- `BOOTSTRAP_SUPERADMIN_NAME`

### Option B - Seed migration with temporary credentials
Use carefully in non-production or controlled deployment only. Credentials must be rotated immediately.

## Mandatory Rules
- Do not hardcode production credentials in source control.
- Force privileged users to enable Google Authenticator.
- Log the bootstrap action in activity logs.
- Remove or disable bootstrap path after first use.

## Suggested First Login Flow
1. Superadmin logs in.
2. System requires password reset if temporary password used.
3. System requires Google Authenticator setup.
4. System grants dashboard access only after 2FA is enabled.
