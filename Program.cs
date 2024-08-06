using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ContactManager.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using ContactManager.Authorization;

// Create the WebApplication instance
var builder = WebApplication.CreateBuilder(args);

// Retrieve connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configure services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddScoped<IAuthorizationHandler, ContactIsOwnerAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ContactAdministratorsAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ContactManagerAuthorizationHandler>();

// Build the app
var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    var testAdminPw = builder.Configuration.GetValue<string>("SeedAdminPW");
    var testManagerPw = builder.Configuration.GetValue<string>("SeedManagerPW");

    await SeedData.Initialize(services, testAdminPw: testAdminPw, testManagerPw: testManagerPw);
}

// Configure middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Configure endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.MapRazorPages();

// Run the application
app.Run();
