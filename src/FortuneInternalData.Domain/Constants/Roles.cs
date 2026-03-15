namespace FortuneInternalData.Domain.Constants;

public static class Roles
{
    public const string Superadmin = "Superadmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Staff = "Staff";

    public static readonly string[] All =
    {
        Superadmin,
        Admin,
        Manager,
        Staff
    };
}
