using Hangfire;
using Hangfire.SqlServer;

using IdentityCoreCustomization;
using IdentityCoreCustomization.Classes.Identity;
using IdentityCoreCustomization.Data;
using IdentityCoreCustomization.Models.Identity;
using IdentityCoreCustomization.Services;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PwnedPasswords.Validator;

using System;
using System.Globalization;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appSettings.Development.json", optional: true, reloadOnChange: true) // loaded later → higher precedence
    .AddEnvironmentVariables();

CultureInfo.DefaultThreadCurrentCulture
    = CultureInfo.DefaultThreadCurrentUICulture = PersianDateExtensionMethods.GetPersianCulture();


var services = builder.Services;
var configuration = builder.Configuration;

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddDatabaseDeveloperPageExceptionFilter();

// Register Pwned Passwords HTTP Client (uses k-anonymity for privacy)
services.AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned: 1);

services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<PersianIdentityErrorDescriber>()
    .AddPwnedPasswordValidator<ApplicationUser>(options =>
    {
        options.ErrorMessage = "هشدار: این رمز عبور در لیست رمزهای نشت‌شده دیده شده است. لطفاً یک رمز عبور قوی‌تر و منحصربه‌فرد انتخاب کنید.";
    });
//var memoryCacheTicketStore = new MemoryCacheTicketStore();
//services.AddSingleton(memoryCacheTicketStore);

services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Register DatabaseTicketStore as both interface and concrete type
services.AddSingleton<DatabaseTicketStore>();
services.AddSingleton<ITicketStore>(provider => provider.GetRequiredService<DatabaseTicketStore>());

services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
    .Configure<ITicketStore>((options, store) => { options.SessionStore = store; });

services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/Identity/Account/Login");
    options.LogoutPath = new PathString("/Identity/Account/Logout");
    options.AccessDeniedPath = new PathString("/Identity/Account/AccessDenied");
});

// Password hasher and alternatives (left commented to preserve original intent)
// services.Configure<PasswordHasherOptions>(options => options.IterationCount = 310000);
// services.AddScoped<IPasswordHasher<ApplicationUser>, PlainTextPasswordHasher<ApplicationUser>>();
// services.AddScoped<IPasswordHasher<ApplicationUser>, Argon2PasswordHasher<ApplicationUser>>();
// services.Configure<Argon2PasswordHasherOptions>(options => { options.Strength = Argon2HashStrength.Medium; });

services.AddTransient<IUserStore<ApplicationUser>, UserStore>();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
services.AddTransient<IEmailSender, EmailSender>();

services.AddScoped<ISmsService, SmsService>();
// FIXED: Changed from Transient to Scoped to match ApplicationDbContext lifetime
services.AddScoped<IDatabaseCleanerService, DatabaseCleanerService>();
// NEW: Add user session management service
services.AddScoped<IUserSessionService, UserSessionService>();

services.AddControllersWithViews();

// Hangfire
services.AddHangfire(hf => hf
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

services.AddHangfireServer();

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

app.MapHangfireDashboard();

// FIXED: Use Hangfire.Extensions.DependencyInjection pattern for scoped services
// Register the job to run every 20 seconds
RecurringJob.AddOrUpdate<IDatabaseCleanerService>("CleanDatabaseJob", 
    service => service.CleanDatabaseAsync(), 
    "*/20 * * * * *");

// IMPORTANT: Seed database and fix ConcurrencyStamp issues
try
{
    await DatabaseSeeder.SeedRolesAsync(app.Services);
}
catch (Exception ex)
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("Program");
    logger.LogError(ex, "Error occurred during database seeding");
}

app.Run();
