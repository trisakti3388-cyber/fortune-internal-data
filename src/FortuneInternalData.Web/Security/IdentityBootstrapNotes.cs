namespace FortuneInternalData.Web.Security;

public static class IdentityBootstrapNotes
{
    public const string RecommendedApproach = @"
Recommended production approach:
1. Replace custom user auth flow with ASP.NET Identity UserManager/SignInManager.
2. Store role membership using Identity roles.
3. Enforce 2FA for privileged roles.
4. Seed first Superadmin from a secure bootstrap command or migration-time routine.
5. Use Otp.NET + QRCoder for Google Authenticator onboarding.
";
}
