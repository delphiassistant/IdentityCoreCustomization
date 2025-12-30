# Two-Factor Authentication (2FA) Implementation Guide

## Overview

This document outlines the complete Two-Factor Authentication (2FA) implementation in the IdentityCoreCustomization project. The system supports both **SMS-based 2FA** and **Authenticator App (TOTP) 2FA** with comprehensive recovery options.

---

## Table of Contents

1. [Features Overview](#features-overview)
2. [Authenticator App (TOTP) 2FA](#authenticator-app-totp-2fa)
3. [SMS-Based 2FA](#sms-based-2fa)
4. [Recovery Codes](#recovery-codes)
5. [Technical Implementation](#technical-implementation)
6. [User Workflows](#user-workflows)
7. [Security Considerations](#security-considerations)
8. [Dependencies](#dependencies)

---

## Features Overview

### ✅ Implemented Features

| Feature | Description | Status |
|---------|-------------|--------|
| **Authenticator App Setup** | QR code and manual key entry for TOTP apps | ✅ Complete |
| **TOTP Login** | Login with 6-digit time-based codes | ✅ Complete |
| **SMS-Based 2FA** | Send verification codes via SMS | ✅ Complete |
| **Recovery Codes** | 10 single-use backup codes | ✅ Complete |
| **Recovery Code Login** | Login using recovery codes | ✅ Complete |
| **Smart Login Routing** | Auto-detect authenticator vs SMS | ✅ Complete |
| **Device Remembering** | Remember trusted devices for 30 days | ✅ Complete |
| **2FA Management Dashboard** | Enable/disable, reset, regenerate codes | ✅ Complete |
| **Copy to Clipboard** | One-click copy for shared keys | ✅ Complete |
| **Download Recovery Codes** | Export codes as text file | ✅ Complete |
| **Persian (Farsi) UI** | Full RTL support with Persian labels | ✅ Complete |

---

## Authenticator App (TOTP) 2FA

### Setup Process

#### 1. Enable Authenticator App
**Route:** `/Users/Manage/EnableAuthenticator`

**Features:**
- **QR Code Generation:** Server-side QR code using QRCoder library (200x200px)
- **Manual Key Entry:** Formatted shared key displayed in Bootstrap alert
- **Copy to Clipboard:** One-click copy button with visual feedback
- **Verification:** Enter 6-digit TOTP code to confirm setup
- **Automatic Recovery Codes:** 10 codes generated immediately after setup

#### 2. View/Controller Implementation

**Controller:** `ManageController.EnableAuthenticator()`
```csharp
- GET: Display QR code and shared key
- POST: Verify TOTP code and enable 2FA
- Generates recovery codes automatically
- Uses TempData to pass codes to ShowRecoveryCodes
```

**Key Methods:**
- `LoadSharedKeyAndQrCodeUriAsync()` - Generate authenticator key and URI
- `FormatKey()` - Format key with spaces (e.g., "abcd efgh ijkl")
- `GenerateQrCodeUri()` - Create otpauth:// URI for QR code

### Login with Authenticator

**Route:** `/Users/Account/LoginWith2fa`

**Features:**
- Enter 6-digit TOTP code from authenticator app
- "Remember this device" checkbox (30-day cookie)
- Link to recovery code login as fallback
- Auto-lockout after failed attempts

**Flow:**
1. User enters username/password
2. System detects authenticator is configured
3. Redirects to TOTP code entry
4. Validates code using `TwoFactorAuthenticatorSignInAsync()`
5. Signs in or shows error

### Reset Authenticator

**Route:** `/Users/Manage/ResetAuthenticatorWarning`

**Features:**
- Warning page with consequences
- Disables 2FA temporarily
- Generates new authenticator key
- Requires re-setup in authenticator app
- Recovery codes remain valid

---

## SMS-Based 2FA

### Setup Process

**Route:** `/Users/Manage/AddPhoneNumber`

**Features:**
- Phone number verification via SMS
- 6-digit verification code
- 5-minute expiration
- Stored in `UserPhoneTokens` table

### Login with SMS

**Route:** `/Users/Account/LoginWith2faSms`

**Features:**
- SMS code sent to registered phone number
- 5-minute validity period
- Link to recovery code login
- Auto-generated on login if authenticator not configured

---

## Recovery Codes

### Generation

**Automatic Generation:**
- 10 codes created after enabling authenticator
- Stored securely in `UserTokens` table
- Each code usable only once

**Manual Regeneration:**
**Route:** `/Users/Manage/GenerateRecoveryCodes` (POST)

### Display and Download

**Route:** `/Users/Manage/ShowRecoveryCodes`

**Features:**
- ⚠️ **Security Warning:** Displayed prominently
- **LTR Display:** Codes shown in monospace font, left-to-right
- **Download Button:** Export as timestamped text file
- **Format:** Clean, numbered list with instructions

**Downloaded File Format:**
```
کدهای بازیابی احراز هویت دو مرحله ای
==========================================

تاریخ ایجاد: [timestamp]

هشدار: این کدها را در مکانی امن نگه دارید.
هر کد فقط یک بار قابل استفاده است.

کدهای بازیابی:
==========================================
1. ABCD-EFGH-IJKL
2. MNOP-QRST-UVWX
...
10. YZ12-3456-7890

==========================================
این کدها را حذف نکنید!
```

### Using Recovery Codes

**Route:** `/Users/Account/LoginWithRecoveryCode`

**Features:**
- Enter recovery code (spaces/hyphens ignored)
- Each code works only once
- Accessible from both TOTP and SMS login pages
- Auto-lockout after multiple failed attempts

---

## Technical Implementation

### NuGet Packages Added

```xml
<PackageReference Include="QRCoder" Version="1.6.0" />
```

### Models Created

| Model | Purpose | Location |
|-------|---------|----------|
| `EnableAuthenticatorModel` | QR code setup | Areas/Users/Models/ |
| `ShowRecoveryCodesModel` | Display recovery codes | Areas/Users/Models/ |
| `ResetAuthenticatorWarningModel` | Reset confirmation | Areas/Users/Models/ |
| `LoginWith2faModel` | TOTP login | Areas/Users/Models/ |
| `LoginWithRecoveryCodeModel` | Recovery code login | Areas/Users/Models/ |
| `TwoFactorAuthenticationModel` | 2FA dashboard (updated) | Areas/Users/Models/ |

### Controller Actions

#### ManageController
```csharp
// Setup & Configuration
- EnableAuthenticator (GET/POST)
- ShowRecoveryCodes (GET)
- ResetAuthenticatorWarning (GET)
- ResetAuthenticatorKey (POST)

// Recovery Codes
- GenerateRecoveryCodes (POST)
- ResetRecoveryCodes (POST)

// 2FA Management
- TwoFactorAuthentication (GET)
- SetTwoFactorAuthentication (POST)
- Disable2fa (POST)
- ForgetBrowser (POST)
```

#### AccountController
```csharp
// Login
- Login (GET/POST) - Smart routing to TOTP or SMS
- LoginWith2fa (GET/POST) - TOTP login
- LoginWith2faSms (GET/POST) - SMS login
- LoginWithRecoveryCode (GET/POST) - Recovery code login
```

### Database Storage

**UserTokens Table:**
```
LoginProvider: [AspNetUserStore]
Name: AuthenticatorKey | RecoveryCodes
UserId: [User ID]
Value: [Encrypted key/codes]
```

**UserPhoneTokens Table:**
```
PhoneNumber: User's phone
AuthenticationCode: 6-digit code
AuthenticationKey: GUID for verification
ExpireTime: 5-minute validity
Confirmed: Verification status
```

### JavaScript Features

**Copy to Clipboard:**
```javascript
- Uses modern Clipboard API (navigator.clipboard)
- Fallback to document.execCommand for older browsers
- Visual feedback (button turns green, shows checkmark)
- Removes spaces from key before copying
- RTL support for Persian text
```

**Download Recovery Codes:**
```javascript
- Creates Blob with text content
- Generates timestamped filename
- UTF-8 encoding with Persian characters
- Clean numbered format
```

---

## User Workflows

### First-Time Setup (Authenticator App)

1. Navigate to `/Users/Manage/TwoFactorAuthentication`
2. Click "افزودن برنامه احراز هویت" (Add Authenticator App)
3. Install authenticator app (Google/Microsoft Authenticator)
4. Scan QR code OR copy manual key
5. Enter 6-digit verification code
6. View and download 10 recovery codes
7. Save codes securely
8. Click "تایید" (Confirm)

### Login with 2FA Enabled

**Scenario 1: Authenticator App Available**
1. Enter username/password
2. System detects authenticator configured
3. Redirected to `/Users/Account/LoginWith2fa`
4. Enter 6-digit TOTP code from app
5. Optionally check "Remember this device"
6. Successfully logged in

**Scenario 2: Authenticator Not Available**
1. Click recovery code link
2. Redirected to `/Users/Account/LoginWithRecoveryCode`
3. Enter one recovery code
4. Successfully logged in (code consumed)

**Scenario 3: SMS-Based 2FA**
1. Enter username/password (no authenticator)
2. System sends SMS code
3. Redirected to `/Users/Account/LoginWith2faSms`
4. Enter SMS code
5. Successfully logged in

### Managing 2FA Settings

**Dashboard:** `/Users/Manage/TwoFactorAuthentication`

Available Actions:
- ✅ View authenticator status
- ✅ View recovery codes count
- ✅ Add/Reset authenticator app
- ✅ Generate new recovery codes
- ✅ Disable 2FA completely
- ✅ Forget remembered devices
- ✅ Enable/Disable SMS 2FA

---

## Security Considerations

### Best Practices Implemented

✅ **Authenticator Key Security**
- Keys generated using ASP.NET Core Identity's secure random generator
- Stored encrypted in database
- Never logged or exposed in URLs

✅ **Recovery Codes**
- 10 codes (recommended minimum)
- Single-use only (consumed after use)
- Stored hashed in database
- Only displayed once during generation

✅ **Time-Based Codes (TOTP)**
- 30-second validity window
- Clock drift tolerance built into Identity
- SHA-1 hashing with base32 encoding

✅ **Rate Limiting**
- Account lockout after failed attempts
- Configurable in `IdentityOptions`

✅ **Device Remembering**
- 30-day cookie expiration (default)
- Revocable via "Forget Browser" action

✅ **SMS Security**
- 5-minute code expiration
- One-time use codes
- Stored with expiration timestamp

### Recommendations for Production

⚠️ **Additional Security Measures:**

1. **Implement Rate Limiting**
   - Add throttling for SMS sending (prevent abuse)
   - Limit TOTP verification attempts per minute

2. **Monitor 2FA Usage**
   - Log 2FA setup/disable events
   - Alert on suspicious recovery code usage

3. **Backup Communication**
   - Support multiple phone numbers
   - Email notifications for 2FA changes

4. **Session Management**
   - Require 2FA for sensitive actions
   - Force re-authentication after timeout

5. **Audit Logging**
   - Track all 2FA-related events
   - Alert on repeated failures

---

## Dependencies

### Required NuGet Packages

```xml
<!-- QR Code Generation -->
<PackageReference Include="QRCoder" Version="1.6.0" />

<!-- ASP.NET Core Identity (already included) -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.20" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="8.0.20" />
```

### JavaScript Libraries (CDN)

- **Font Awesome** - Icons for UI elements
- **Bootstrap 5** - UI framework (RTL version)

### External Services

- **SMS Provider:** PARSGREEN API (configured in `appsettings.json`)
- **Email Provider:** SMTP configuration for notifications

---

## Configuration

### appsettings.json

```json
{
  "Identity": {
    "PreRegistrationEnabled": true,
    "Password": {
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true,
      "RequireNonAlphanumeric": true,
      "RequiredLength": 6
    },
    "Lockout": {
      "DefaultLockoutTimeSpan": "00:05:00",
      "MaxFailedAccessAttempts": 5,
      "AllowedForNewUsers": true
    },
    "SignIn": {
      "RequireConfirmedAccount": false,
      "RequireConfirmedEmail": false,
      "RequireConfirmedPhoneNumber": false
    }
  }
}
```

---

## Testing Checklist

### Authenticator App Setup
- [ ] QR code displays correctly (200x200px)
- [ ] Manual key displays in LTR format with spaces
- [ ] Copy button works and shows success feedback
- [ ] TOTP verification accepts valid codes
- [ ] Recovery codes generated after successful setup
- [ ] Download button creates valid text file

### TOTP Login
- [ ] System routes to TOTP login when authenticator configured
- [ ] Valid codes grant access
- [ ] Invalid codes show error message
- [ ] "Remember device" checkbox works
- [ ] Recovery code link appears and works
- [ ] Account locks after multiple failures

### SMS Login
- [ ] System routes to SMS login when no authenticator
- [ ] SMS codes sent successfully
- [ ] 5-minute expiration enforced
- [ ] Recovery code link appears and works

### Recovery Codes
- [ ] Codes work only once
- [ ] Codes display in proper format
- [ ] Download creates timestamped file
- [ ] Regenerate creates new set of 10 codes
- [ ] Old codes invalidated after regeneration

### 2FA Management
- [ ] Dashboard shows correct status
- [ ] Reset authenticator works
- [ ] Disable 2FA removes requirement
- [ ] Forget browser clears remembered device

---

## Troubleshooting

### Common Issues

**Issue:** QR code too large
- **Solution:** Adjust `GetGraphic()` parameter in EnableAuthenticator.cshtml (currently set to 10)

**Issue:** Recovery codes not displaying
- **Solution:** Check TempData configuration and session state

**Issue:** TOTP codes not working
- **Solution:** Verify server time synchronization (TOTP requires accurate time)

**Issue:** SMS not sending
- **Solution:** Check PARSGREEN API credentials in configuration

**Issue:** Persian text displays incorrectly
- **Solution:** Ensure `dir="rtl"` attribute on elements with Persian text

---

## Future Enhancements

### Potential Improvements

- [ ] **Backup Codes via Email:** Send recovery codes to user's email
- [ ] **Multiple Authenticators:** Support multiple TOTP devices
- [ ] **WebAuthn/FIDO2:** Hardware key support
- [ ] **Biometric Authentication:** Face ID, Touch ID integration
- [ ] **2FA Analytics Dashboard:** Track usage and security metrics
- [ ] **QR Code Customization:** Add logo/branding to QR codes
- [ ] **Push Notifications:** Approve login from mobile app
- [ ] **Geo-fencing:** Location-based 2FA requirements

---

## Support & Documentation

### Related Files

- **Controllers:** `Areas/Users/Controllers/ManageController.cs`, `AccountController.cs`
- **Views:** `Areas/Users/Views/Manage/`, `Areas/Users/Views/Account/`
- **Models:** `Areas/Users/Models/`
- **Services:** `Services/SmsService.cs`

### Contact

For issues or questions about this implementation, please refer to the project repository:
- **GitHub:** https://github.com/delphiassistant/IdentityCoreCustomization

---

## Changelog

### Version 1.0 (2024)
- ✅ Initial implementation of TOTP authenticator app support
- ✅ QR code generation with QRCoder library
- ✅ Recovery codes system (10 single-use codes)
- ✅ Smart login routing (TOTP vs SMS)
- ✅ Device remembering functionality
- ✅ Copy to clipboard feature
- ✅ Download recovery codes as text file
- ✅ Complete Persian (Farsi) localization
- ✅ RTL support throughout UI

---

## License

This implementation follows the license of the main IdentityCoreCustomization project.

---

**Last Updated:** December 2024  
**Version:** 1.0  
**Target Framework:** .NET 8.0
