using FortuneInternalData.Domain.Constants;

namespace FortuneInternalData.Web.Security;

public static class AuthorizationPolicies
{
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.SuperadminOnly, policy =>
                policy.RequireRole(Roles.Superadmin));

            options.AddPolicy(PolicyNames.AdminOrAbove, policy =>
                policy.RequireRole(Roles.Superadmin, Roles.Admin));

            options.AddPolicy(PolicyNames.ManagerOrAbove, policy =>
                policy.RequireRole(Roles.Superadmin, Roles.Admin, Roles.Manager));

            options.AddPolicy(PolicyNames.StaffOrAbove, policy =>
                policy.RequireRole(Roles.Superadmin, Roles.Admin, Roles.Manager, Roles.Staff));

            options.AddPolicy(PolicyNames.ImportConfirm, policy =>
                policy.RequireRole(Roles.Superadmin, Roles.Admin));

            options.AddPolicy(PolicyNames.UserManagement, policy =>
                policy.RequireRole(Roles.Superadmin));
        });

        return services;
    }
}
