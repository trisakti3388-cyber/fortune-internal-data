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
            .AddIdentity<IdentityApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IPhoneNumberValidationService, PhoneNumberValidator>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITotpSetupService, TotpSetupService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IImportFileParser, CsvImportFileParser>();

        services.AddScoped<IPhoneNumberRepository, PhoneNumberRepository>();
        services.AddScoped<IImportBatchRepository, ImportBatchRepository>();
        services.AddScoped<IImportBatchRowRepository, ImportBatchRowRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IdentityBootstrapService>();

        return services;
    }
}
