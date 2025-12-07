# Version History

This document tracks all changes, improvements, and fixes made to the IdentityCoreCustomization project.

> **📊 For comprehensive project analysis, see [Project_Analysis.md](Project_Analysis.md)**

---

## 2024-12-02 15:30 - Version 1.6

### Fixed

#### NuGet Package Dependency Warnings (NU1608)
- **Location**: `IdentityCoreCustomization.csproj`
- **Issue**: Multiple NU1608 warnings about Microsoft.CodeAnalysis package version conflicts
  - `Microsoft.VisualStudio.Web.CodeGeneration.Design 9.0.0` depends on `Microsoft.CodeAnalysis.*` version 4.8.0
  - .NET 10 requires `Microsoft.CodeAnalysis.*` version 4.14.0
  - NuGet resolver upgraded to 4.14.0, violating scaffolding tool's exact version constraint
- **Fix**: Added explicit package references to override transitive dependencies:
  ```xml
  <PackageReference Include="Microsoft.CodeAnalysis.Common" Version="4.14.0" />
  <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" />
  <PackageReference Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="4.14.0" />
  <PackageReference Include="Microsoft.CodeAnalysis.Workspaces.Common" Version="4.14.0" />
  ```
- **Impact**: 
  - Build now completes with zero warnings
  - Scaffolding tools remain functional
  - Project compatible with .NET 10 CodeAnalysis requirements
  - No breaking changes to existing functionality

### Technical Details

#### Packages Affected
- `Microsoft.CodeAnalysis.Common` - Explicitly set to 4.14.0
- `Microsoft.CodeAnalysis.CSharp` - Explicitly set to 4.14.0  
- `Microsoft.CodeAnalysis.CSharp.Workspaces` - Explicitly set to 4.14.0
- `Microsoft.CodeAnalysis.Workspaces.Common` - Explicitly set to 4.14.0

#### Why This Works
- Explicit package references take precedence over transitive dependencies
- NuGet resolver uses the highest compatible version specified
- Scaffolding tools (9.0.0) work with CodeAnalysis 4.14.0 despite requesting 4.8.0
- No version 10.0.0 stable of scaffolding tools exists yet (only RC versions)

#### Alternative Approaches Considered
1. ❌ **Upgrade to scaffolding 10.0.0** - Not available (only RC versions)
2. ❌ **Downgrade .NET 10 to .NET 8** - Loses .NET 10 features
3. ✅ **Explicit version overrides** - Clean solution, no trade-offs

---

## 2024-12-02 12:53 - Version 1.5

### Added

#### Password Breach Detection - Have I Been Pwned Integration
- **Location**: `Program.cs` and `IdentityCoreCustomization.csproj`
- **Package**: `PwnedPasswords.Validator` version 1.2.0
- **Changes**:
  - Integrated Have I Been Pwned (HIBP) API for password breach detection
  - Uses k-anonymity model for secure password checking (only first 5 characters of SHA-1 hash sent)
  - Automatic validation during user registration and password changes
  - Applied to both end-user registration (`/Identity/Account/Register`) and admin user management (`/Admin/Users/Create`, `/Admin/Users/ChangePassword`)
  - Custom Persian error message: "هشدار: این رمز عبور در لیست رمزهای نشت‌شده دیده شده است. لطفاً یک رمز عبور قوی‌تر و منحصربه‌فرد انتخاب کنید."
- **Security Impact**: 
  - Prevents users from choosing passwords that have appeared in known data breaches
  - No privacy concerns - uses k-anonymity (partial hash matching)
  - Checks against 613+ million pwned passwords in HIBP database
  - Works seamlessly with ASP.NET Core Identity validation pipeline
- **Documentation**: See `HIBP-Integration-Guide.md`

#### Admin User Management - Create User Functionality
- **Location**: `/Areas/Admin/Views/Users/Create.cshtml`
- **Changes**:
  - Added `PhoneNumber` input field to user creation form
  - Field includes proper validation matching the model's regex pattern (`^(\+98|0)?9\d{9}$`)
  - Integrated with Bootstrap form styling for consistency

#### Admin User Management - Edit User Functionality  
- **Location**: `/Areas/Admin/Views/Users/Edit.cshtml`
- **Changes**:
  - Added `Email` input field with email validation
  - Added `PhoneNumber` input field with validation
  - Added `EmailConfirmed` checkbox to allow admins to manually confirm user emails
  - Added `PhoneNumberConfirmed` checkbox to allow admins to manually confirm phone numbers
  - Added `LockoutEnabled` checkbox to control whether user accounts can be locked out
  - Added `TwoFactorEnabled` checkbox to enable/disable two-factor authentication for users
  - All fields properly labeled with Persian translations matching the model Display attributes

### Fixed

#### UsersController - Create Action
- **Location**: `/Areas/Admin/Controllers/UsersController.cs` - `Create` POST method
- **Issue**: User creation was not properly setting all properties from the model
- **Fix**: Updated user object initialization to include:
  - `PhoneNumber` property (was completely missing)
  - `EmailConfirmed` property (now respects model value instead of hardcoded logic)
  - `PhoneNumberConfirmed` property
  - `LockoutEnabled` property
  - `TwoFactorEnabled` property
- **Impact**: Users can now be created with complete profile information and proper security settings

#### UsersController - Edit GET Action
- **Location**: `/Areas/Admin/Controllers/UsersController.cs` - `Edit` GET method
- **Issue**: Edit form was not loading existing user data for email, phone, and security settings
- **Fix**: Updated model population to include all user properties:
  - `Email`
  - `PhoneNumber`
  - `EmailConfirmed`
  - `PhoneNumberConfirmed`
  - `LockoutEnabled`
  - `TwoFactorEnabled`
- **Impact**: Admins can now see and modify all important user properties during edit operations

### Improved

#### User Management Completeness
- **Before**: Admin could only edit username and roles; contact info and security settings were ignored
- **After**: Complete user management with ability to:
  - Manage contact information (email, phone number)
  - Control verification status (email confirmed, phone confirmed)
  - Configure security settings (lockout enabled, two-factor enabled)
  - Manage user roles
  - Update username

#### Data Integrity
- All model properties now properly mapped in both Create and Edit operations
- Form validation matches backend model validation rules
- No data loss when creating or editing users

### Technical Details

#### Models Affected
- `CreateUserModel` (`/Areas/Admin/Models/CreateUser.cs`)
  - All properties now properly utilized in Create view and controller
- `EditUserModel` (`/Areas/Admin/Models/EditUserModel.cs`)
  - All properties now properly utilized in Edit view and controller

#### Validation Rules Maintained
- Username: Required, max 256 chars, regex `^[a-zA-Z0-9_@.-]+$`
- Email: Optional, email format, max 256 chars
- PhoneNumber: Optional, phone format, regex `^(\+98|0)?9\d{9}$` (Iranian mobile format)
- Password (Create only): Required, 6-100 chars
- **NEW**: Password breach check via Have I Been Pwned API

#### Security Considerations
- EmailConfirmed/PhoneNumberConfirmed flags allow admins to manually verify contact info
- LockoutEnabled controls whether account lockout policies apply to the user
- TwoFactorEnabled can be controlled by admin for immediate effect
- Role changes trigger security stamp update and force user re-authentication
- **NEW**: Breached password detection prevents use of compromised credentials

---

## Project Information

**Project**: IdentityCoreCustomization  
**Version**: 1.6  
**Target Framework**: .NET 10  
**Project Type**: ASP.NET Core MVC with Identity  
**Language**: C# 13.0  

### Key Technologies
- ASP.NET Core Identity
- Entity Framework Core 10.0
- Razor Views
- Bootstrap 5
- Font Awesome icons
- Hangfire (background jobs)
- MailKit (email)
- ParsGreen (SMS)
- QRCoder (2FA QR codes)
- CheckBoxList.Core (UI component)
- **PwnedPasswords.Validator** (password breach detection)

### NuGet Packages
```xml
<PackageReference Include="CheckBoxList.Core" Version="1.1.0" />
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.22" />
<PackageReference Include="Hangfire.SqlServer" Version="1.8.22" />
<PackageReference Include="MailKit" Version="4.14.1" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Microsoft.CodeAnalysis.Common" Version="4.14.0" />
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.14.0" />
<PackageReference Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="4.14.0" />
<PackageReference Include="Microsoft.CodeAnalysis.Workspaces.Common" Version="4.14.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
<PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="9.0.0" />
<PackageReference Include="PwnedPasswords.Validator" Version="1.2.0" />
<PackageReference Include="QRCoder" Version="1.7.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.0.1" />
```

### Database
- SQL Server (LocalDB for development)
- Connection String: `Server=localhost;Database=IdentityCoreCustomization;Trusted_Connection=True`
- Custom table/column naming (Users, Roles, UserID, RoleID, etc.)

---

## Quick Links

- 📊 **[Comprehensive Project Analysis](Project_Analysis.md)** - Features, gaps, and recommendations
- 🔐 **[HIBP Integration Guide](HIBP-Integration-Guide.md)** - Password breach detection
- 🔑 **[2FA Implementation Guide](2FA-Implementation-Guide.md)** - Two-factor authentication
- 📖 **[README](README.md)** - Project overview

---

## Notes

This version history follows [Semantic Versioning](https://semver.org/) principles and [Keep a Changelog](https://keepachangelog.com/) format.

### Categories Used
- **Added**: New features
- **Changed**: Changes in existing functionality
- **Deprecated**: Soon-to-be removed features
- **Removed**: Removed features
- **Fixed**: Bug fixes
- **Security**: Security improvements
- **Improved**: General improvements and enhancements

---

## Next Steps - Recommended Action Items

> **📊 For detailed analysis and prioritized recommendations, see [Project_Analysis.md](Project_Analysis.md)**

### Immediate (This Sprint)
1. ✅ **COMPLETED**: Fix Admin Create/Edit user forms
2. ✅ **COMPLETED**: Implement password breach detection (Have I Been Pwned)
3. ✅ **COMPLETED**: Resolve NuGet dependency warnings (NU1608)
4. Implement rate limiting on sensitive endpoints
5. Add CAPTCHA to public forms
6. Review and enhance error logging (include HIBP events)

### Short Term (Next 2-4 Weeks)
7. Create unit test project and basic test coverage (including HIBP tests)
8. Implement audit logging for admin actions and security events
9. Add user-facing session management page
10. Configure and test email confirmation workflow
11. Add logging for pwned password detections

### Long Term (Next Quarter)
12. Comprehensive integration testing
13. Performance testing and optimization
14. Complete documentation (deployment, troubleshooting)
15. Consider API implementation if needed
16. Implement device/browser tracking
17. Add geographic login tracking (IP-based)

### Maintenance
- Regular dependency updates (including PwnedPasswords.Validator)
- Security vulnerability scanning
- Database backup strategy
- Log retention policy
- Session cleanup monitoring
- Monitor HIBP API availability and rate limits
- **Monitor for scaffolding tools 10.0.0 stable release**
