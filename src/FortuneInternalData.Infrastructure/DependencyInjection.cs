using FortuneInternalData.Application.Interfaces;
using FortuneInternalData.Application.Validators;
using FortuneInternalData.Infrastructure.Identity;
using FortuneInternalData.Infrastructure.Persistence;
using FortuneInternalData.Infrastructure.Repositories;
using FortuneInternalData.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FortuneInternalData.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services
            .AddIdentity<IdentityApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

        services.AddHttpContextAccessor();

        // Validators
        services.AddScoped<IPhoneNumberValidationService, PhoneNumberValidator>();

        // Identity & Auth
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITotpSetupService, TotpSetupService>();
        services.AddScoped<IdentityBootstrapService>();

        // Import
        services.AddSingleton<ImportFileParserFactory>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IImportBackgroundQueue, ImportBackgroundQueue>();
        services.AddHostedService<ImportBackgroundWorker>();

        // Query Services (use Infrastructure implementations)
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IImportQueryService, ImportQueryService>();
        services.AddScoped<IUserQueryService, UserQueryService>();
        services.AddScoped<IUserService, UserService>();

        // Permissions
        services.AddScoped<IPermissionService, PermissionService>();

        // Repositories
        services.AddScoped<IPhoneNumberRepository, PhoneNumberRepository>();
        services.AddScoped<IImportBatchRepository, ImportBatchRepository>();
        services.AddScoped<IImportBatchRowRepository, ImportBatchRowRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
