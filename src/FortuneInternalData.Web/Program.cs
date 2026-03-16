using FortuneInternalData.Infrastructure.Identity;
using FortuneInternalData.Infrastructure.Persistence;
using FortuneInternalData.Web.Extensions;
using FortuneInternalData.Web.Filters;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<MustChangePasswordFilter>();
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
