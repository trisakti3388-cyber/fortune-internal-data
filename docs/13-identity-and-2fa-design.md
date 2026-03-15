# Identity and 2FA Design

## Preferred Approach
Use ASP.NET Identity as the standard authentication and authorization framework.

## Components
- `UserManager<IdentityApplicationUser>`
- `SignInManager<IdentityApplicationUser>`
- Identity roles for Superadmin, Admin, Manager, Staff
- TOTP setup service compatible with Google Authenticator

## Login Flow
1. User submits email and password.
2. Identity validates credentials.
3. If user requires 2FA, redirect to OTP verification page.
4. User enters authenticator code.
5. On success, grant authenticated session.

## 2FA Setup Flow
1. User navigates to setup page.
2. System generates authenticator secret.
3. System creates QR code URI.
4. User scans QR code using Google Authenticator.
5. User enters verification code.
6. System verifies code and enables authenticator login.

## Recommended Libraries
- Otp.NET for TOTP generation/verification
- QRCoder for QR code rendering

## Security Notes
- Enforce 2FA for privileged roles.
- Protect bootstrap credentials via environment variables.
- Use strong password and lockout policy.
- Log important identity operations.
- Consider recovery codes in a later phase.
