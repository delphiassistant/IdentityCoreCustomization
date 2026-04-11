using IdentityCoreCustomization;
using IdentityCoreCustomization.Classes.Identity;
using IdentityCoreCustomization.Data;
using IdentityCoreCustomization.Models.Identity;
using IdentityCoreCustomization.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Globalization;
using IdentityCoreCustomization.Classes.Extensions;
using IdentityCoreCustomization.Classes.ModelBinding;
using IdentityCoreCustomization.Classes.Logging;
using IdentityCoreCustomization.Middleware;
using System.IO;


CultureInfo.DefaultThreadCurrentCulture
    = CultureInfo.DefaultThreadCurrentUICulture = PersianDateExtensionMethods.GetPersianCulture();

// Bootstrap logger — captures failures before host/DI is available.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.With<PersianTimestampEnricher>()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appSettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true) // loaded later → higher precedence
    .AddEnvironmentVariables();

builder.Host.UseSerilog((context, services, config) =>
    config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.With<PersianTimestampEnricher>());




var services = builder.Services;
var configuration = builder.Configuration;

services.Configure<IdentityCoreCustomization.Models.GeneralConfig>(
    configuration.GetSection("GeneralConfig"));

var appName = configuration["GeneralConfig:ApplicationName"] ?? "IdentityCoreCustomization";

services.AddDataProtection()
    .SetApplicationName(appName)
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")));

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddDatabaseDeveloperPageExceptionFilter();

// Read Identity configuration
var checkPasswordWithHIBP = configuration.GetValue<bool>("Identity:Password:CheckPasswordWithHIBP");

// Conditionally register Pwned Passwords HTTP Client (uses k-anonymity for privacy)
if (checkPasswordWithHIBP)
{
    services.AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned: 1);
}

var identityBuilder = services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = false;
        
        // Configure password options from appSettings
        options.Password.RequireDigit = configuration.GetValue<bool>("Identity:Password:RequireDigit");
        options.Password.RequiredLength = configuration.GetValue<int>("Identity:Password:RequiredLength");
        options.Password.RequireNonAlphanumeric = configuration.GetValue<bool>("Identity:Password:RequireNonAlphanumeric");
        options.Password.RequireUppercase = configuration.GetValue<bool>("Identity:Password:RequireUppercase");
        options.Password.RequireLowercase = configuration.GetValue<bool>("Identity:Password:RequireLowercase");
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<PersianIdentityErrorDescriber>();

// Conditionally add Pwned Password Validator
if (checkPasswordWithHIBP)
{
    identityBuilder.AddPwnedPasswordValidator<ApplicationUser>(options =>
    {
        options.ErrorMessage = "هشدار: این رمز عبور امن نیست و در لیست رمزهای پخش شده در دارک وب دیده شده است. لطفاً یک رمز عبور قوی‌تر و منحصربه‌فرد انتخاب کنید.";
    });
}

services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Register DatabaseTicketStore as both interface and concrete type
services.AddSingleton<DatabaseTicketStore>();
services.AddSingleton<ITicketStore>(provider => provider.GetRequiredService<DatabaseTicketStore>());

services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
    .Configure<ITicketStore>((options, store) => { options.SessionStore = store; });

services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = appName;
    options.LoginPath = new PathString("/Users/Account/Login");
    options.LogoutPath = new PathString("/Users/Account/Logout");
    options.AccessDeniedPath = new PathString("/Users/Account/AccessDenied");
});

services.AddTransient<IUserStore<ApplicationUser>, UserStore>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
services.AddTransient<IEmailSender, EmailSender>();

// Register background email queue and service
services.AddSingleton<IBackgroundEmailQueue, BackgroundEmailQueue>();
services.AddHostedService<BackgroundEmailService>();

services.AddScoped<ISmsService, SmsService>();
// Register background SMS queue and service
services.AddSingleton<IBackgroundSmsQueue, BackgroundSmsQueue>();
services.AddHostedService<BackgroundSmsService>();

// FIXED: Changed from Transient to Scoped to match ApplicationDbContext lifetime
services.AddScoped<IDatabaseCleanerService, DatabaseCleanerService>();
// NEW: Add user session management service
services.AddScoped<IUserSessionService, UserSessionService>();

services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new PersianDateModelBinderProvider());
});

// Register background services
services.AddHostedService<DatabaseCleanupBackgroundService>();

// Admin first-run setup state cache
services.AddSingleton<AdminSetupState>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseMiddleware<AdminSetupMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
// app.MapRazorPages(); // intentionally left commented to match previous Startup configuration

// IMPORTANT: Seed database and fix ConcurrencyStamp issues
try
{
    await DatabaseSeeder.SeedRolesAsync(app.Services);
}
catch (Exception ex)
{
    Log.Error(ex, "Error occurred during database seeding");
}

try
{
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
