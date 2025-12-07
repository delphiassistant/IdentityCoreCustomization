# Have I Been Pwned (HIBP) Password Validation Integration

## Overview

This project integrates **Have I Been Pwned** password breach detection to prevent users from choosing compromised passwords. The integration uses the `PwnedPasswords.Validator` library which implements a privacy-preserving k-anonymity model.

---

## 🔐 What is Have I Been Pwned?

Have I Been Pwned is a service created by security researcher Troy Hunt that allows you to check if your password has appeared in known data breaches. The database contains over **613 million** pwned passwords from various breaches.

**Website**: https://haveibeenpwned.com/Passwords

---

## 🛡️ Security & Privacy

### K-Anonymity Model

The implementation uses **k-anonymity** to protect user privacy:

1. **Local Hashing**: Password is hashed locally using SHA-1
2. **Partial Hash Sent**: Only the first 5 characters of the hash are sent to the API
3. **Range Response**: API returns all hash suffixes matching that prefix
4. **Local Matching**: Your application checks if the full hash exists in the returned set

**Example:**
```
Password: "password123"
SHA-1 Hash: 482C811DA5D5B4BC6D497FFA98491E38B9A4C6
First 5 chars sent: 482C8
Response: Hundreds of matching suffixes
Local check: Does "11DA5D5B4BC6D497FFA98491E38B9A4C6" exist in response?
```

### What Data is Transmitted?

- ✅ **Sent to API**: First 5 characters of password hash (e.g., `482C8`)
- ❌ **NOT sent**: Full password
- ❌ **NOT sent**: Full hash
- ❌ **NOT sent**: Username
- ❌ **NOT sent**: Email

**Your actual password never leaves your server.**

---

## 📦 Implementation Details

### Package Installed

```xml
<PackageReference Include="PwnedPasswords.Validator" Version="1.2.0" />
```

**GitHub**: https://github.com/andrewlock/PwnedPasswords

### Code Changes

#### 1. Program.cs - Service Registration

```csharp
using PwnedPasswords.Validator;

// Register HTTP client for HIBP API
services.AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned: 1);

// Add validator to Identity
services.AddIdentity<ApplicationUser, ApplicationRole>(options => { ... })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<PersianIdentityErrorDescriber>()
    .AddPwnedPasswordValidator<ApplicationUser>(options =>
    {
        options.ErrorMessage = "هشدار: این رمز عبور در لیست رمزهای نشت‌شده دیده شده است. لطفاً یک رمز عبور قوی‌تر و منحصربه‌فرد انتخاب کنید.";
    });
```

#### Parameters Explained

- **`minimumFrequencyToConsiderPwned: 1`**: 
  - Reject password if seen even **once** in breaches
  - You can increase this (e.g., 5, 10) to allow commonly used but breached passwords
  - **Recommendation**: Keep at 1 for maximum security

- **`ErrorMessage`**: 
  - Custom Persian error message
  - Displayed to users when they try to use a pwned password

---

## ✅ Where It Works

The password breach check is automatically enforced in:

### 1. User Registration
- **Route**: `/Identity/Account/Register`
- **Trigger**: When a new user signs up
- **Behavior**: Registration fails if password is pwned

### 2. Password Change (User)
- **Route**: `/Identity/Manage/ChangePassword`
- **Trigger**: When user changes their password
- **Behavior**: Password change fails if new password is pwned

### 3. Password Reset
- **Route**: `/Identity/Account/ResetPassword`
- **Trigger**: After user requests password reset
- **Behavior**: Reset fails if new password is pwned

### 4. Admin User Creation
- **Route**: `/Admin/Users/Create`
- **Trigger**: When admin creates a new user
- **Behavior**: User creation fails if password is pwned

### 5. Admin Password Change
- **Route**: `/Admin/Users/ChangePassword`
- **Trigger**: When admin resets a user's password
- **Behavior**: Password change fails if new password is pwned

---

## 🧪 Testing the Integration

### Test 1: Known Pwned Password

Try to register or change password to a commonly breached password:

**Test Passwords** (all pwned):
- `password`
- `password123`
- `123456`
- `qwerty`
- `letmein`

**Expected Result**:
```
هشدار: این رمز عبور در لیست رمزهای نشت‌شده دیده شده است. 
لطفاً یک رمز عبور قوی‌تر و منحصربه‌فرد انتخاب کنید.
```

### Test 2: Strong Unique Password

Try to register with a strong, random password:

**Test Passwords** (safe):
- `Tr!ck$yP@nda2024!`
- `Xk9#mQ2$vL8!pR3`
- Generate using: https://passwordsgenerator.net/

**Expected Result**:
- ✅ Password accepted
- ✅ User registered/password changed successfully

---

## 📊 API Rate Limits

**Have I Been Pwned API Rate Limits**:
- **Free tier**: Unlimited requests
- **No authentication required** for password range API
- **Recommended**: Add caching for production (built into the library)

The `PwnedPasswords.Validator` library includes:
- ✅ Automatic retry logic
- ✅ Response caching (reduces API calls)
- ✅ Configurable timeouts

---

## 🎛️ Configuration Options

### Adjust Frequency Threshold

If you want to allow passwords that appear less than N times:

```csharp
services.AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned: 5);
```

**Examples**:
- `1` = Reject if seen even once (most secure) ⭐
- `5` = Allow if seen less than 5 times
- `100` = Allow if seen less than 100 times (less secure)

### Custom Error Messages

You can customize the error message per your needs:

```csharp
.AddPwnedPasswordValidator<ApplicationUser>(options =>
{
    // English
    options.ErrorMessage = "This password has been exposed in a data breach. Please choose a different password.";
    
    // Persian (current)
    options.ErrorMessage = "هشدار: این رمز عبور در لیست رمزهای نشت‌شده دیده شده است. لطفاً یک رمز عبور قوی‌تر و منحصربه‌فرد انتخاب کنید.";
});
```

### Disable for Testing

To temporarily disable (not recommended for production):

```csharp
// Comment out these lines in Program.cs:
// services.AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned: 1);
// .AddPwnedPasswordValidator<ApplicationUser>(...)
```

---

## 🚀 Production Considerations

### 1. Network Connectivity

**Requirement**: Application needs outbound HTTPS access to:
- `https://api.pwnedpasswords.com`

**Firewall Rules**: Ensure port 443 (HTTPS) is open for outbound connections.

### 2. Offline Fallback

If API is unreachable:
- ✅ Request times out after configured duration
- ✅ Password validation continues with other validators
- ⚠️ Password may be accepted even if pwned (security vs. availability tradeoff)

**To enforce strict checking**:
```csharp
services.AddPwnedPasswordHttpClient(
    minimumFrequencyToConsiderPwned: 1,
    failOnUnavailable: true // Fail validation if API is down
);
```

### 3. Performance

- **Typical response time**: 50-200ms per check
- **Caching**: Library caches responses for 24 hours by default
- **Impact**: Minimal - only checked during password operations (not login)

### 4. Logging

The library logs API calls at `Information` level:

```
[Information] PwnedPasswordValidator: Password hash prefix 482C8 found 3861493 times
```

Consider logging these events for security auditing.

---

## 📈 Monitoring

### Metrics to Track

1. **Pwned Password Detections**
   - How many users tried to use breached passwords?
   - Trend over time

2. **API Availability**
   - Success rate of API calls
   - Average response time

3. **User Friction**
   - Do users abandon registration after pwned password error?
   - Consider UX improvements

### Recommended Logging

Add custom logging in your application:

```csharp
// In a custom password validator or middleware
_logger.LogWarning(
    "User {UserId} attempted to set a pwned password (seen {Count} times)", 
    userId, 
    pwnedCount
);
```

---

## 🔄 Updating the Library

To update to the latest version:

```bash
dotnet add package PwnedPasswords.Validator
```

Check for updates: https://www.nuget.org/packages/PwnedPasswords.Validator

---

## 🆘 Troubleshooting

### Issue 1: Build Errors

**Error**: `'IServiceCollection' does not contain a definition for 'AddPwnedPasswordHttpClient'`

**Solution**: 
```csharp
// Add using directive:
using PwnedPasswords.Validator;
```

### Issue 2: Password Always Rejected

**Symptom**: Even strong passwords are rejected

**Possible Causes**:
1. API is down (check logs)
2. Network connectivity issues
3. Incorrect threshold configuration

**Debug**:
```csharp
// Temporarily increase logging
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
```

### Issue 3: Error Message Not in Persian

**Solution**: Ensure `options.ErrorMessage` is set in `Program.cs`:
```csharp
.AddPwnedPasswordValidator<ApplicationUser>(options =>
{
    options.ErrorMessage = "هشدار: این رمز عبور در لیست رمزهای نشت‌شده دیده شده است...";
});
```

---

## 📚 Additional Resources

1. **Have I Been Pwned Website**: https://haveibeenpwned.com/
2. **API Documentation**: https://haveibeenpwned.com/API/v3#PwnedPasswords
3. **Library GitHub**: https://github.com/andrewlock/PwnedPasswords
4. **Troy Hunt's Blog**: https://www.troyhunt.com/

---

## 🎓 Best Practices

### ✅ Do

- ✅ Keep `minimumFrequencyToConsiderPwned` at 1 for maximum security
- ✅ Use the default caching (reduces API load)
- ✅ Log pwned password attempts for security monitoring
- ✅ Provide clear error messages to users
- ✅ Test with known pwned passwords before going to production

### ❌ Don't

- ❌ Don't disable in production (defeats the purpose)
- ❌ Don't set threshold too high (e.g., > 100)
- ❌ Don't log actual passwords (only log that check occurred)
- ❌ Don't rely solely on this - keep other password policies (length, complexity)

---

## 📝 Summary

✅ **Privacy-Preserving**: K-anonymity ensures passwords never leave your server  
✅ **Automatic**: Works seamlessly with ASP.NET Core Identity  
✅ **Comprehensive**: Covers registration, password changes, and admin operations  
✅ **Localized**: Persian error messages for better UX  
✅ **Zero Configuration**: Works out of the box after package installation  

**Result**: Users are prevented from choosing passwords from 613+ million known breaches, significantly improving account security without compromising privacy.
