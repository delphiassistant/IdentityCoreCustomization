# IdentityCoreCusomization

Goal
- Demonstrate a production-oriented customization of ASP.NET Core Identity on .NET 8 with MVC controllers (replacing Razor Pages Identity), focusing on localization, SMS-based authentication, admin tooling, and scalable session management.

Features
- .NET 8 minimal hosting with all setup consolidated in `Program.cs`.
- MVC controllers in Identity area replacing default Razor Pages Identity.
- Identity with `int` keys: custom `ApplicationUser`, `ApplicationRole`, and related entities.
- Custom EF Core schema mapping: renamed tables/columns in `ApplicationDbContext` (e.g., `Users`, `Roles`, `UserID`).
- Persian `IdentityErrorDescriber` for localized Identity error messages.
- Custom `UserStore` implementation enabling login by username, email, or phone number.
- Passwordless SMS login flow using `UserLoginWithSms` and `SmsService`.
- SMS-based pre-registration (`UserPreRegistration`) gated by `Identity:PreRegistrationEnabled` configuration.
- Server-side cookie session storage via `ITicketStore` implementation `DatabaseTicketStore` and `AuthenticationTicket` model.
- Online user session management dashboard (`/UserSessions`) with Bootstrap confirmations and admin actions (force logout, cleanup expired, clear all).
- In-memory `MemoryCacheTicketStore` utility for session ticket management (optional).
- Hangfire integration with SQL Server storage, dashboard, and recurring job `DatabaseCleanerService` for cleanup.
- Admin area to manage users and roles: create users, assign roles, reset passwords.
- Role-based UI visibility with `RolesTagHelper` using `visible-to-roles` attribute in Razor.
- `ClaimsPrincipal` helpers in `IdentityExtensions` for user id/name/email access.
- Cookie path configuration and relaxed Identity options (e.g., `RequireUniqueEmail=false`).


## Quick Start

- Set `DefaultConnection` in `appsettings.json` to your SQL Server.
- Apply migrations (e.g., `dotnet ef database update`).
- Run the app (`dotnet run`) and browse to `/Identity/Account/Login` or `/Admin`.

## Identity Customizations Overview

This section details all custom features implemented in this project compared to stock ASP.NET Core Identity, with code samples and explanations.

**Target**: .NET 8, MVC controllers (no Razor Pages Identity)

---

### 1) Custom Identity types with int keys and schema mapping

**What**: `ApplicationUser : IdentityUser<int>`, `ApplicationRole : IdentityRole<int>` and custom claim/login/role/token entities. The `ApplicationDbContext` changes default table and column names.

**Code (ApplicationDbContext)**:

```csharp
public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser,
    ApplicationRole,
    int,
    ApplicationUserClaim,
    ApplicationUserRole,
    ApplicationUserLogin,
    ApplicationRoleClaim,
    ApplicationUserToken>
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
            b.Property(e => e.Id).HasColumnName("UserID");
            b.Property(e => e.UserName).HasColumnName("Username");
            b.Property(e => e.NormalizedUserName).HasColumnName("NormalizedUsername");
        });

        modelBuilder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("Roles");
            b.Property(e => e.Id).HasColumnName("RoleID");
            b.Property(e => e.Name).HasColumnName("RoleName");
            b.Property(e => e.NormalizedName).HasColumnName("RoleNormalizedName");
        });
    }
}
```

**Effect**: compatibility with existing DB schemas; simpler joining with numeric keys.

---

### 2) Persian IdentityErrorDescriber (localized errors)

**What**: Localized error messages for Identity via a custom `IdentityErrorDescriber`.

**Code**:

```csharp
public class PersianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateUserName(string userName) => new()
    {
        Code = nameof(DuplicateUserName),
        Description = $"نام کاربری '{userName}' به کاربر دیگری اختصاص یافته است."
    };
}
```

**Registration**:

```csharp
services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<PersianIdentityErrorDescriber>();
```

**Effect**: consistent localized errors across UI and APIs.

---

### 3) Multi-identifier login (username/email/phone)

**What**: Custom `UserStore.FindByNameAsync` allows locating user by `UserName`, `PhoneNumber`, or `Email`.

**Code**:

```csharp
public Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken ct)
{
    var dbUser = db.Users.FirstOrDefault(u =>
        u.UserName == normalizedUserName ||
        u.PhoneNumber == normalizedUserName ||
        u.Email == normalizedUserName);
    return Task.FromResult(dbUser);
}
```

**Effect**: users can sign in using any of the three identifiers. Consider normalizing inputs consistently.

---

### 4) Passwordless SMS login flow

**What**: Issue one-time code to phone, verify, then sign-in.

**Entities and flow**:

```csharp
// Model
public class UserLoginWithSms
{
    [Key] public int LoginWithSmsID { get; set; }
    public string PhoneNumber { get; set; }
    public string AuthenticationCode { get; set; }
    public string AuthenticationKey { get; set; }
    public DateTime ExpireDate { get; set; }
    public int UserID { get; set; }
    public ApplicationUser User { get; set; }
    public void Initialize() { /* set code/key */ }
}
```

**Controller highlights**:

```csharp
[HttpPost]
[AllowAnonymous]
public async Task<IActionResult> LoginWithSms(LoginWithSmsModel model)
{
    var user = await _userManager.FindByNameAsync(model.PhoneNumber);
    var loginWithSms = new UserLoginWithSms
    {
        PhoneNumber = user.PhoneNumber,
        UserID = user.Id,
        ExpireDate = DateTime.Now.AddMinutes(5)
    };
    loginWithSms.Initialize();
    db.UserLoginWithSms.Add(loginWithSms);
    await db.SaveChangesAsync();
    await smsService.SendSms($"کد امنیتی شما: {loginWithSms.AuthenticationCode}", new() { loginWithSms.PhoneNumber });
    return RedirectToAction("LoginWithSmsResponse", new { Key = loginWithSms.AuthenticationKey });
}

[HttpPost]
[AllowAnonymous]
public async Task<IActionResult> LoginWithSmsResponse(LoginWithSmsResponseModel model)
{
    var row = db.UserLoginWithSms.Include(pr => pr.User)
        .FirstOrDefault(pr => pr.AuthenticationKey == model.AuthenticationKey);
    if (row != null && row.AuthenticationCode == model.AuthenticationCode)
    {
        await _signInManager.SignInAsync(row.User, true);
        return RedirectToAction("Index", "Manage", new { area = "Identity" });
    }
    // handle errors
}
```

**Effect**: passwordless UX; security depends on code entropy, expiry, rate-limiting, and delivery channel.

---

### 5) SMS-based pre-registration gate

**What**: Optional pre-registration step toggled by `Identity:PreRegistrationEnabled`. User verifies phone via SMS before registration continues.

**Code (controller excerpts)**:

```csharp
[AllowAnonymous]
public async Task<IActionResult> PreRegister(UserPreRegistration model)
{
    model.Initialize();
    model.ExpireTime = DateTime.Now.AddMinutes(5);
    db.PreRegistrations.Add(model);
    await db.SaveChangesAsync();
    await smsService.SendSms($"کد امنیتی: {model.AuthenticationCode}", new() { model.PhoneNumber });
    return RedirectToAction("PreRegisterConfirm", new { Key = model.AuthenticationKey });
}

[AllowAnonymous]
public IActionResult Register(string returnUrl = null, string Key = null)
{
    bool enabled = Configuration.GetValue<bool>("Identity:PreRegistrationEnabled");
    if (enabled)
    {
        var ok = db.PreRegistrations.Any(pr => pr.Confirmed && pr.AuthenticationKey == Key && pr.ExpireTime > DateTime.Now);
        if (!ok) return RedirectToAction("PreRegister");
    }
    return View();
}
```

**Effect**: mitigates fake registrations; ensures phone verification prior to account creation.

---

### 6) Admin area for users and roles

**What**: `Areas/Admin` controllers to manage roles and users, including assigning roles and resetting user passwords.

**Code (assign roles by IDs)**:

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateUserModel model, List<int> selectedRoles)
{
    var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
    var result = await userManager.CreateAsync(user, model.Password);
    if (result.Succeeded && selectedRoles.Any())
    {
        var userRoles = selectedRoles.Select(sr => new ApplicationUserRole { UserId = user.Id, RoleId = sr }).ToList();
        await context.UserRoles.AddRangeAsync(userRoles);
        await context.SaveChangesAsync();
    }
    // ...
}
```

**Effect**: quick management UI; writes directly via EF for roles assignment.

---

### 7) Persian display metadata and int-based entities

**What**: `ApplicationUser`/`ApplicationRole` have Persian `Display` attributes; int keys and navigation properties.

**Code**:

```csharp
public class ApplicationUser : IdentityUser<int>
{
    [Display(Name= "نام کاربر")]
    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
    public override string UserName { get; set; }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    // ...
}
```

**Effect**: localized UI labels; richer navigation for queries.

---

### 8) Server-side cookie session store (ITicketStore => database)

**What**: Replaces default cookie-only storage with DB-backed `ITicketStore` (`DatabaseTicketStore`) for the Identity cookie.

**Updated registration (DI + options)**:

```csharp
// Required for DatabaseTicketStore
services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
services.AddSingleton<DatabaseTicketStore>();
services.AddSingleton<ITicketStore>(sp => sp.GetRequiredService<DatabaseTicketStore>());

services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
    .Configure<ITicketStore>((options, store) => { options.SessionStore = store; });
```

**Store (excerpt)**:

```csharp
public async Task<string> StoreAsync(AuthenticationTicket ticket)
{
    var userId = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
    var authenticationTicket = new Models.Identity.AuthenticationTicket
    {
        UserId = Convert.ToInt32(userId),
        LastActivity = DateTimeOffset.UtcNow,
        Value = TicketSerializer.Default.Serialize(ticket),
        Expires = ticket.Properties.ExpiresUtc
    };
    db.AuthenticationTickets.Add(authenticationTicket);
    await db.SaveChangesAsync();
    return authenticationTicket.UserId.ToString();
}
```

**Effect**: centralized session management, easier invalidation across servers, and visibility of active sessions.

---

### 9) Online user session management dashboard

**Route**: `/UserSessions`

**Features**:
- View currently online users with activity and session metadata
- Force logout a user (now supports logging out the current signed-in admin as well)
- Cleanup expired sessions
- Clear all sessions (nuclear option)
- Bootstrap modals for all confirmations (no `alert`/`confirm` JS)
- AJAX for force-logout to avoid menu/submit issues; other actions keep simple post after modal confirm

**Notes**:
- For self force-logout, the server signs out the current session and redirects to login
- Anti-forgery tokens and `X-Requested-With` header are used in AJAX posts

---

### 10) Hangfire integration and recurring jobs

**Registration and dashboard**:

```csharp
services.AddHangfire(hf => hf
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));

services.AddHangfireServer();
app.MapHangfireDashboard();
```

**Recurring job**:

```csharp
RecurringJob.AddOrUpdate<IDatabaseCleanerService>(
    "CleanDatabaseJob",
    service => service.CleanDatabaseAsync(),
    "*/20 * * * * *");
```

---

### 11) ClaimsPrincipal helper extensions

Helpers to get logged-in user id/name/email from claims.

---

### 12) Cookie paths and Identity options

```csharp
services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = false;
});
```

---

### 13) Minimal hosting with endpoints and middleware

```csharp
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHangfireDashboard();
```

---

## Additional Updates

- Added `DatabaseSeeder.SeedRolesAsync(app.Services)` at startup to ensure base roles exist.
- In views, replaced all `confirm()` prompts with Bootstrap modals for consistent UX.
- Force-logout uses AJAX with proper anti-forgery and shows Bootstrap confirmation before posting.

## Notes and Considerations

- Normalize inputs consistently for multi-identifier login; consider enforcing unique phone/email if required.
- Add throttling/rate-limits and audit logs for SMS flows.
- Secure Hangfire dashboard (e.g., authorization filter).
- Periodically purge `AuthenticationTickets` to avoid DB growth.
- Replace placeholder email/SMS with production providers.

## Getting Started

Follow these steps to implement these Identity customizations in a new ASP.NET Core 8 project by copying and adapting files from this repository.

### Key configuration and registrations (excerpt)

```csharp
// DbContext
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

services.AddDatabaseDeveloperPageExceptionFilter();

// Identity
services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<PersianIdentityErrorDescriber>();

// Session store and cookie options
services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
services.AddSingleton<DatabaseTicketStore>();
services.AddSingleton<ITicketStore>(sp => sp.GetRequiredService<DatabaseTicketStore>());
services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
    .Configure<ITicketStore>((options, store) => { options.SessionStore = store; });

// Services
services.AddTransient<IUserStore<ApplicationUser>, UserStore>();
services.AddTransient<IEmailSender, EmailSender>();
services.AddScoped<ISmsService, SmsService>();
services.AddScoped<IDatabaseCleanerService, DatabaseCleanerService>();

// Hangfire
services.AddHangfire(hf => hf
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));
services.AddHangfireServer();

// Controllers and endpoints
services.AddControllersWithViews();

var app = builder.Build();

// ... middleware ...

app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHangfireDashboard();

// Recurring job
RecurringJob.AddOrUpdate<IDatabaseCleanerService>(
    "CleanDatabaseJob",
    service => service.CleanDatabaseAsync(),
    "*/20 * * * * *");

// Seed roles
await DatabaseSeeder.SeedRolesAsync(app.Services);