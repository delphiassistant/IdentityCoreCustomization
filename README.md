# IdentityCoreCustomization

## Goal
Demonstrate a production-oriented customization of ASP.NET Core Identity on .NET 10 with MVC controllers (replacing Razor Pages Identity), focusing on localization, SMS-based authentication, admin tooling, scalable session management, and modern UI.

## ✨ Recent Major Enhancements

### 🎨 Modern UI Overhaul (January 2026)
- **Complete authentication pages redesign** with purple gradient backgrounds
- **Password strength indicators** with real-time validation
- **Responsive sidebar navigation** for manage area
- **Card-based layouts** throughout admin and user areas
- **Shared layouts**: `_AuthLayout.cshtml` for auth pages, modern sidebar for manage area
- **Mobile-first responsive design** with RTL (Persian) support

### 🎯 Enhancement Status
- ✅ **Admin Area**: 100% complete - Modern dashboard, user management, role management
- ✅ **Account Views**: 60% complete (12/20) - Login, register, 2FA, password reset
- ✅ **Manage Views**: 23% complete (3/13) - Profile, email, password change
- ⏳ **Remaining**: 21 views to enhance (SMS login, phone verification, 2FA management)

### 🎨 CSS Framework
- `auth.css` - Authentication pages styling (400+ lines)
- `admin.css` - Admin dashboard styling
- `manage.css` - User profile management styling

## Features
- .NET 10 with all setup consolidated in `Program.cs`
- MVC controllers in `Users` area replacing default Razor Pages Identity
- Identity with `int` keys: custom `ApplicationUser`, `ApplicationRole`, and related entities
- Custom EF Core schema mapping: renamed tables/columns in `ApplicationDbContext`
- Persian `IdentityErrorDescriber` for localized Identity error messages
- Custom `UserStore` implementation enabling login by username, email, or phone number
- Passwordless SMS login flow using `UserLoginWithSms` and `SmsService`
- SMS-based pre-registration gated by `Identity:PreRegistrationEnabled` configuration
- **Complete Two-Factor Authentication (2FA)** with Authenticator Apps (TOTP) and SMS
- **Password Breach Detection** via Have I Been Pwned API (k-anonymity model)
- Server-side cookie session storage via `ITicketStore` (`DatabaseTicketStore`)
- Online user session management dashboard with admin actions
- Background cleanup service for expired sessions and tokens
- Admin area for comprehensive user and role management
- Role-based UI visibility with `RolesTagHelper`

## 🔐 Two-Factor Authentication (2FA)

Complete 2FA implementation supporting both **Authenticator Apps (TOTP)** and **SMS-based verification**.

### Key 2FA Features
✅ QR code generation for authenticator apps  
✅ 10 single-use recovery codes  
✅ SMS-based 2FA fallback  
✅ Device remembering (30-day cookies)  
✅ Comprehensive management dashboard  
✅ Persian/Farsi localization with RTL support  

### 2FA Routes
- `/Users/Manage/TwoFactorAuthentication` - 2FA dashboard
- `/Users/Manage/EnableAuthenticator` - Setup authenticator with QR code
- `/Users/Manage/ShowRecoveryCodes` - View/download recovery codes
- `/Users/Account/LoginWith2fa` - Login with TOTP code
- `/Users/Account/LoginWithRecoveryCode` - Login with recovery code

**📖 Detailed Guide**: See [Documents/2FA-Implementation-Guide.md](Documents/2FA-Implementation-Guide.md)

## 🛡️ Password Breach Detection

Integration with **Have I Been Pwned (HIBP) API** using `PwnedPasswords.Validator` package.

### Features
- ✅ Checks against 613+ million breached passwords
- ✅ k-anonymity model (only first 5 SHA-1 hash characters sent)
- ✅ Applied to registration and password changes
- ✅ Custom Persian error message
- ✅ Works with ASP.NET Core Identity validation pipeline

**📖 Detailed Guide**: See [Documents/HIBP-Integration-Guide.md](Documents/HIBP-Integration-Guide.md)

## Quick Start

1. Set `DefaultConnection` in `appsettings.json` to your SQL Server
2. Apply migrations: `dotnet ef database update`
3. Run the app: `dotnet run`
4. Browse to `/Users/Account/Login` or `/Admin`

## 📐 Design System

### Authentication Pages
- **Layout**: Uses shared `_AuthLayout.cshtml` for all Account views
- **Background**: Purple gradient (`#667eea` → `#764ba2`)
- **Components**: Modern cards, smooth animations, icon integration
- **Features**: Password strength, auto-submit, loading states
- **Mobile**: Fully responsive with RTL support
- **Structure**: Views focus on content only; layout handles HTML/head/scripts

### Manage Area
- **Layout**: Sidebar navigation with modern design
- **Components**: Card-based sections, status badges, action buttons
- **Navigation**: Collapsible sidebar for mobile
- **User Info**: Avatar with username in sidebar footer

### Admin Area  
- **Layout**: Full admin dashboard with navigation
- **Tables**: Sortable, filterable data tables
- **Actions**: Role assignment, password reset, session management
- **UI**: Bootstrap-based with FontAwesome icons

## Known Issues & Fixes

### ✅ Fixed Issues
- **White space at bottom** - Resolved with body background gradient
- **Content cutoff on tall pages** - Fixed with `justify-content: flex-start` and scrolling
- **Labels not visible** - Changed to dark gray (`#374151`)
- **OAuth redirect issues** - Fixed hardcoded area names after Identity→Users rename

### Current Limitations
- Email confirmation workflow requires configuration
- SMS service needs production provider setup
- Some views still pending UI enhancement (see status above)

## Identity Customizations Overview

### 1) Custom Identity Types with Int Keys
`ApplicationUser : IdentityUser<int>`, `ApplicationRole : IdentityRole<int>` with custom schema mapping.

```csharp
modelBuilder.Entity<ApplicationUser>(b => {
    b.ToTable("Users");
    b.Property(e => e.Id).HasColumnName("UserID");
    b.Property(e => e.UserName).HasColumnName("Username");
});
```

### 2) Persian Localization
```csharp
public class PersianIdentityErrorDescriber : IdentityErrorDescriber {
    public override IdentityError DuplicateUserName(string userName) => new() {
        Code = nameof(DuplicateUserName),
        Description = $"نام کاربری '{userName}' به کاربر دیگری اختصاص یافته است."
    };
}
```

### 3) Multi-Identifier Login
Custom `UserStore.FindByNameAsync` allows login by username, email, or phone number.

### 4) Passwordless SMS Login
One-time code flow with `UserLoginWithSms` entity and expiration handling.

### 5) SMS Pre-Registration Gate
Optional phone verification before registration via `Identity:PreRegistrationEnabled`.

### 6) Admin Area
Full user/role management with assignment, password reset, and session control.

### 7) Server-Side Session Store
`DatabaseTicketStore` implementation for centralized session management.

### 8) Session Management Dashboard
View active sessions, force logout, cleanup expired, clear all sessions.

### 9) Background Cleanup Service
`BackgroundService` with `PeriodicTimer` to purge expired data every 20 seconds.

## Technical Stack

### Backend
- **.NET 10** - C# 14
- **ASP.NET Core Identity** - Authentication & authorization
- **Entity Framework Core 10** - Data access
- **SQL Server** - Database

### Frontend
- **Bootstrap 5.3 RTL** - UI framework
- **FontAwesome 6** - Icons
- **Vazirmatn Font** - Persian typography
- **Custom CSS** - auth.css, admin.css, manage.css
- **Vanilla JavaScript** - Form interactions
- **jQuery Validation** - Client-side validation

### NuGet Packages
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.0
- `Microsoft.EntityFrameworkCore.SqlServer` 10.0.0
- `MailKit` 4.14.1
- `QRCoder` 1.7.0
- `PwnedPasswords.Validator` 1.2.0
- `CheckBoxList.Core` 1.1.0

## Project Structure

```
IdentityCoreCustomization/
├── Areas/
│   ├── Admin/                 # Admin dashboard
│   │   ├── Controllers/
│   │   ├── Models/
│   │   └── Views/
│   └── Users/                 # User authentication & management
│       ├── Controllers/
│       ├── Models/
│       └── Views/
│           ├── Account/       # Login, register, 2FA
│           ├── Manage/        # Profile management
│           └── Shared/
│               └── _AuthLayout.cshtml
├── Data/                      # DbContext & migrations
├── Models/                    # Domain models
├── Services/                  # Email, SMS, cleanup services
├── wwwroot/css/
│   ├── auth.css              # Authentication styling
│   ├── admin.css             # Admin dashboard styling
│   └── manage.css            # Profile management styling
└── Documents/
    ├── 2FA-Implementation-Guide.md
    ├── HIBP-Integration-Guide.md
    └── Project_Analysis.md
```

## Configuration

### Database
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IdentityCoreCustomization;Trusted_Connection=True"
  }
}
```

### Identity Options
```csharp
services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<PersianIdentityErrorDescriber>()
.AddPasswordValidator<PwnedPasswordsValidator<ApplicationUser>>();
```

## Documentation

- **[Version History](Version-History.md)** - Complete change log with version tracking
- **[2FA Implementation Guide](Documents/2FA-Implementation-Guide.md)** - Two-factor authentication setup
- **[HIBP Integration Guide](Documents/HIBP-Integration-Guide.md)** - Password breach detection
- **[Project Analysis](Documents/Project_Analysis.md)** - Comprehensive feature analysis & recommendations

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Ensure build succeeds: `dotnet build`
5. Submit a pull request

## License

This project is for demonstration purposes. Modify and use as needed for your projects.

## Support

For issues or questions:
- Check documentation in `Documents/` folder
- Review `Version-History.md` for recent changes
- Open an issue on GitHub

---

**Version**: 1.10  
**Last Updated**: January 2026  
**Target Framework**: .NET 10  
**Status**: Active Development ⚡