# Identity Customizations Overview

This document summarizes all custom features implemented in this project compared to stock ASP.NET Core Identity, with short code samples and the effect of each customization.

- Target: .NET 8, Razor Pages (with MVC controllers)

---

## 1) Custom Identity types with int keys and schema mapping

- What: `ApplicationUser : IdentityUser<int>`, `ApplicationRole : IdentityRole<int>` and custom claim/login/role/token entities. The `ApplicationDbContext` changes default table and column names.

Code (ApplicationDbContext):

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

        modelBuilder.Entity<ApplicationRole>().HasData(
            new ApplicationRole { Id = 1, Name = "Admin", NormalizedName = "مدیران سایت" }
        );
    }
}
```

Effect: compatibility with existing DB schemas; simpler joining with numeric keys; seeded default role.

---

## 2) Persian IdentityErrorDescriber (localized errors)

- What: Localized error messages for Identity via a custom `IdentityErrorDescriber`.

Code:

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

Registration:

```csharp
services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<PersianIdentityErrorDescriber>();
```

Effect: consistent localized errors across UI and APIs.

---

## 3) Multi-identifier login (username/email/phone)

- What: Custom `UserStore.FindByNameAsync` allows locating user by `UserName`, `PhoneNumber`, or `Email`.

Code:

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

Effect: users can sign in using any of the three identifiers. Consider normalizing inputs consistently.

---

## 4) Passwordless SMS login flow

- What: Issue one-time code to phone, verify, then sign-in.

Entities and flow:

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

Controller highlights:

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

Effect: passwordless UX; security depends on code entropy, expiry, rate-limiting, and delivery channel.

---

## 5) SMS-based pre-registration gate

- What: Optional pre-registration step toggled by `Identity:PreRegistrationEnabled`. User verifies phone via SMS before registration continues.

Code (controller excerpts):

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

Effect: mitigates fake registrations; ensures phone verification prior to account creation.

---

## 6) Admin area for users and roles

- What: `Areas/Admin` controllers to manage roles and users, including assigning roles and resetting user passwords.

Code (assign roles by IDs):

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

Effect: quick management UI; writes directly via EF for roles assignment.

---

## 7) Persian display metadata and int-based entities

- What: `ApplicationUser`/`ApplicationRole` have Persian `Display` attributes; int keys and navigation properties.

Code:

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

Effect: localized UI labels; richer navigation for queries.

---

## 8) Server-side cookie session store (ITicketStore => database)

- What: Replaces default cookie-only storage with DB-backed `ITicketStore` (`DatabaseTicketStore`) for the Identity cookie.

Registration:

```csharp
services.AddSingleton<ITicketStore, DatabaseTicketStore>();
services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
    .Configure<ITicketStore>((options, store) => { options.SessionStore = store; });
```

Store (excerpt):

```csharp
public async Task<string> StoreAsync(AuthenticationTicket ticket)
{
    var userId = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)ی.Value;
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

Effect: centralized session management, easier invalidation across servers, and visibility of active sessions. Requires cleanup to avoid DB growth.

---

## 9) Role-based visibility TagHelper

- What: Hide elements unless the signed-in user holds any of the specified roles via `visible-to-roles`.

Usage in Razor:

```html
<a href="/Admin/Users" visible-to-roles="Admin,Manager">User Admin</a>
```

Implementation (core idea):

```csharp
public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
{
    if (!httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
    {
        output.SuppressOutput();
        return;
    }
    var allowed = Roles.Split(',').ToList();
    var user = await userManager.FindByIdAsync(UserID);
    var userRoles = await userManager.GetRolesAsync(user);
    if (!allowed.Intersect(userRoles).Any()) output.SuppressOutput();
}
```

Effect: keeps view logic clean; avoids duplicating role checks.

---

## 10) Background cleanup hosted service

- What: Hosted service using `BackgroundService` + `PeriodicTimer` to run `IDatabaseCleanerService.CleanDatabaseAsync` every 20 seconds.
- Purpose: purge expired SMS logins, phone tokens, and authentication tickets to keep the database lean.

Registration:

```csharp
services.AddScoped<IDatabaseCleanerService, DatabaseCleanerService>();
services.AddHostedService<DatabaseCleanupBackgroundService>();
```

Cleaner service remains responsible for the deletion logic.

---

## 11) ClaimsPrincipal helper extensions

- What: Extension methods to get logged-in user id/name/email from claims.

```csharp
public static T GetLoggedInUserId<T>(this ClaimsPrincipal principal) { /* ... */ }
public static string GetLoggedInUserName(this ClaimsPrincipal principal) { /* ... */ }
public static string GetLoggedInUserEmail(this ClaimsPrincipal principal) { /* ... */ }
```

Effect: reduces boilerplate in controllers/views.

---

## 12) Cookie paths and Identity options

- What: Custom cookie paths and relaxed Identity options.

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

Effect: routes align with Identity area; registration less strict (duplicate emails allowed).

---

## 13) Minimal hosting with endpoints and middleware

- What: `Program.cs` contains all service registrations and pipeline, replacing classic Startup.

```csharp
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
```

Effect: aligns with ASP.NET Core 8 minimal hosting model.

---

## Notes and considerations

- Normalize inputs consistently for multi-identifier login; consider enforcing unique phone/email if required.
- Add throttling/rate-limits and audit logs for SMS flows.
- Periodically purge `AuthenticationTickets` to avoid DB growth (handled by the hosted cleanup service).
- Consider using production-grade email/SMS providers and robust error handling.
