using FortuneInternalData.Domain.Entities;
using FortuneInternalData.Infrastructure.Identity;
using FortuneInternalData.Infrastructure.Persistence;
using FortuneInternalData.Web.Extensions;
using FortuneInternalData.Web.Filters;
using FortuneInternalData.Web.Middleware;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FortuneInternalData.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Allow large file uploads (up to 1 GB)
builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 1_073_741_824; // 1 GB
});

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 1_073_741_824; // 1 GB
});

builder.Services.AddScoped<MustChangePasswordFilter>();
builder.Services.AddScoped<PermissionAuthorizationFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<MustChangePasswordFilter>();
    options.Filters.AddService<PermissionAuthorizationFilter>();
});
builder.Services.AddFortuneInternalData(builder.Configuration);

var app = builder.Build();

// Apply database migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await IdentityRoleSeeder.SeedAsync(roleManager);

        var bootstrapService = services.GetRequiredService<IdentityBootstrapService>();
        await bootstrapService.EnsureSuperadminAsync();

        // Seed initial allowed IPs if table is empty
        var anyIps = await dbContext.AllowedIps.AnyAsync();
        if (!anyIps)
        {
            var seedIps = new[]
            {
                new AllowedIp { IpAddress = "172.188.217.112", Description = "Seeded - server IP 1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new AllowedIp { IpAddress = "52.237.88.249",   Description = "Seeded - server IP 2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new AllowedIp { IpAddress = "20.6.33.33",      Description = "Seeded - server IP 3", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            };
            dbContext.AllowedIps.AddRange(seedIps);
            await dbContext.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database or seeding data.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// IP whitelist check BEFORE authentication
app.UseMiddleware<IpWhitelistMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
