# راهنمای پیاده‌سازی Background Queue (SMS & Email)

> **تاریخ ایجاد**: ژانویه 2026  
> **نسخه**: 2.0  
> **وضعیت**: ✅ Production-Ready

---

## 📋 فهرست مطالب

1. [مقدمه](#مقدمه)
2. [مشکل اولیه](#مشکل-اولیه)
3. [راه‌حل پیاده‌سازی شده](#راه-حل-پیاده-سازی-شده)
4. [معماری سیستم](#معماری-سیستم)
5. [فایل‌های ایجاد شده](#فایل-های-ایجاد-شده)
6. [تغییرات اعمال شده](#تغییرات-اعمال-شده)
7. [نحوه استفاده](#نحوه-استفاده)
8. [مزایا و معایب](#مزایا-و-معایب)
9. [تست و نگهداری](#تست-و-نگهداری)
10. [مشکلات احتمالی](#مشکلات-احتمالی)

---

## مقدمه

این سند توضیحات کاملی از پیاده‌سازی **Background Queue** برای SMS و Email در این پروژه ارائه می‌دهد. این قابلیت برای جلوگیری از بلاک شدن درخواست‌های کاربر هنگام ارسال پیامک و ایمیل طراحی شده است.

### اهداف اصلی

- ✅ جلوگیری از بلاک شدن HTTP Request هنگام ارسال SMS و Email
- ✅ بهبود تجربه کاربری (زمان پاسخ سریع‌تر)
- ✅ مدیریت بهتر خطاها در ارسال
- ✅ قابلیت retry و مدیریت صف
- ✅ جداسازی ارسال از منطق کنترلر
- ✅ یکپارچگی برای هر دو سرویس (SMS & Email)

---

## مشکل اولیه

### کد قبلی (Blocking)

```csharp
// ❌ مشکل: درخواست کاربر باید منتظر بماند
await _emailSender.SendEmailAsync(email, subject, body);
await smsService.SendSms(text, phone);
// ممکن است 2-10 ثانیه طول بکشد
return RedirectToAction("NextPage");
```

### مشکلات:

1. **⏱️ تأخیر در پاسخ**: کاربر باید منتظر بماند (2-10 ثانیه)
2. **🔴 نقطه شکست**: اگر سرویس خراب باشد، کل درخواست fail می‌شود
3. **📉 کاهش Performance**: thread‌ها منتظر می‌مانند
4. **❌ عدم مدیریت خطا**: خطاها مستقیماً به کاربر نشان داده می‌شود
5. **🚫 عدم امکان Retry**: اگر ارسال fail کند، دوباره تلاش نمی‌شود

---

## راه‌حل پیاده‌سازی شده

### معماری کلی (یکسان برای SMS و Email)

```
┌─────────────────┐
│  Controller     │
└────────┬────────┘
         │ Queue() (فوری - بدون await)
         ▼
┌─────────────────┐
│ Background      │
│ Queue (Channel) │
└────────┬────────┘
         │ DequeueAsync()
         ▼
┌─────────────────┐
│ Background      │
│ Service (Hosted)│
└────────┬────────┘
         │ با IServiceScopeFactory
         ▼
┌─────────────────┐
│  SMS / Email    │
│  Service        │
└─────────────────┘
```

---

## معماری سیستم

### 1. Background SMS Queue

#### Interface:
```csharp
public interface IBackgroundSmsQueue
{
    void QueueSms(string smsText, string receipentPhone);
    void QueueSms(string smsText, List<string> receipentPhones);
    Task<(string SmsText, List<string> Phones)> DequeueAsync(CancellationToken ct);
}
```

#### Implementation:
```csharp
public class BackgroundSmsQueue : IBackgroundSmsQueue
{
    private readonly Channel<(string SmsText, List<string> Phones)> _queue;

    public BackgroundSmsQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<(string, List<string>)>(options);
    }
}
```

### 2. Background Email Queue

#### Interface:
```csharp
public interface IBackgroundEmailQueue
{
    void QueueEmail(string email, string subject, string htmlMessage);
    Task<(string Email, string Subject, string HtmlMessage)> DequeueAsync(CancellationToken ct);
}
```

#### Implementation:
```csharp
public class BackgroundEmailQueue : IBackgroundEmailQueue
{
    private readonly Channel<(string Email, string Subject, string HtmlMessage)> _queue;

    public BackgroundEmailQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<(string, string, string)>(options);
    }
}
```

### 3. Background Services (با IServiceScopeFactory)

#### BackgroundSmsService:
```csharp
public class BackgroundSmsService : BackgroundService
{
    private readonly IBackgroundSmsQueue _smsQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var (smsText, phones) = await _smsQueue.DequeueAsync(stoppingToken);
            
            // ✅ ایجاد scope برای هر SMS
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();
                await smsService.SendSms(smsText, phones);
            }
        }
    }
}
```

#### BackgroundEmailService:
```csharp
public class BackgroundEmailService : BackgroundService
{
    private readonly IBackgroundEmailQueue _emailQueue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var (email, subject, htmlMessage) = await _emailQueue.DequeueAsync(stoppingToken);
            
            // ✅ ایجاد scope برای هر Email
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                await emailSender.SendEmailAsync(email, subject, htmlMessage);
            }
        }
    }
}
```

**🔑 نکته مهم**: استفاده از `IServiceScopeFactory` به دلیل اینکه `BackgroundService` یک **Singleton** است و نمی‌تواند مستقیماً سرویس‌های **Scoped** را inject کند.

---

## فایل‌های ایجاد شده

### 1. SMS Queue Files

#### `Services/BackgroundSmsQueue.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace YourApp.Services
{
    public interface IBackgroundSmsQueue
    {
        void QueueSms(string smsText, string receipentPhone);
        void QueueSms(string smsText, List<string> receipentPhones);
        Task<(string SmsText, List<string> Phones)> DequeueAsync(CancellationToken cancellationToken);
    }

    public class BackgroundSmsQueue : IBackgroundSmsQueue
    {
        private readonly Channel<(string SmsText, List<string> Phones)> _queue;

        public BackgroundSmsQueue(int capacity = 100)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<(string, List<string>)>(options);
        }

        public void QueueSms(string smsText, string receipentPhone)
        {
            ArgumentNullException.ThrowIfNull(smsText);
            ArgumentNullException.ThrowIfNull(receipentPhone);
            _queue.Writer.TryWrite((smsText, new List<string> { receipentPhone }));
        }

        public void QueueSms(string smsText, List<string> receipentPhones)
        {
            ArgumentNullException.ThrowIfNull(smsText);
            ArgumentNullException.ThrowIfNull(receipentPhones);
            _queue.Writer.TryWrite((smsText, receipentPhones));
        }

        public async Task<(string SmsText, List<string> Phones)> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
```

#### `Services/BackgroundSmsService.cs`
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YourApp.Services
{
    public class BackgroundSmsService : BackgroundService
    {
        private readonly IBackgroundSmsQueue _smsQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BackgroundSmsService> _logger;

        public BackgroundSmsService(
            IBackgroundSmsQueue smsQueue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<BackgroundSmsService> logger)
        {
            _smsQueue = smsQueue;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background SMS Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (smsText, phones) = await _smsQueue.DequeueAsync(stoppingToken);
                    _logger.LogInformation("Sending SMS to {Count} recipient(s)", phones.Count);

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();
                        await smsService.SendSms(smsText, phones);
                    }

                    _logger.LogInformation("SMS sent successfully to {Count} recipient(s)", phones.Count);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending SMS");
                }
            }

            _logger.LogInformation("Background SMS Service stopped.");
        }
    }
}
```

### 2. Email Queue Files

#### `Services/BackgroundEmailQueue.cs`
```csharp
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace YourApp.Services
{
    public interface IBackgroundEmailQueue
    {
        void QueueEmail(string email, string subject, string htmlMessage);
        Task<(string Email, string Subject, string HtmlMessage)> DequeueAsync(CancellationToken cancellationToken);
    }

    public class BackgroundEmailQueue : IBackgroundEmailQueue
    {
        private readonly Channel<(string Email, string Subject, string HtmlMessage)> _queue;

        public BackgroundEmailQueue(int capacity = 100)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<(string, string, string)>(options);
        }

        public void QueueEmail(string email, string subject, string htmlMessage)
        {
            ArgumentNullException.ThrowIfNull(email);
            ArgumentNullException.ThrowIfNull(subject);
            ArgumentNullException.ThrowIfNull(htmlMessage);
            _queue.Writer.TryWrite((email, subject, htmlMessage));
        }

        public async Task<(string Email, string Subject, string HtmlMessage)> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
```

#### `Services/BackgroundEmailService.cs`
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YourApp.Services
{
    public class BackgroundEmailService : BackgroundService
    {
        private readonly IBackgroundEmailQueue _emailQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BackgroundEmailService> _logger;

        public BackgroundEmailService(
            IBackgroundEmailQueue emailQueue,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<BackgroundEmailService> logger)
        {
            _emailQueue = emailQueue;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Email Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (email, subject, htmlMessage) = await _emailQueue.DequeueAsync(stoppingToken);
                    _logger.LogInformation("Sending email to {Email} with subject '{Subject}'", email, subject);

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                        await emailSender.SendEmailAsync(email, subject, htmlMessage);
                    }

                    _logger.LogInformation("Email sent successfully to {Email}", email);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending email");
                }
            }

            _logger.LogInformation("Background Email Service stopped.");
        }
    }
}
```

---

## تغییرات اعمال شده

### 1. تغییرات در `Program.cs`

```csharp
// Email Services
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
services.AddTransient<IEmailSender, EmailSender>();

// ✨ Background Email Queue
services.AddSingleton<IBackgroundEmailQueue, BackgroundEmailQueue>();
services.AddHostedService<BackgroundEmailService>();

// SMS Services
services.AddScoped<ISmsService, SmsService>();

// ✨ Background SMS Queue
services.AddSingleton<IBackgroundSmsQueue, BackgroundSmsQueue>();
services.AddHostedService<BackgroundSmsService>();
```

**Service Lifetimes:**
- `IBackgroundSmsQueue` & `IBackgroundEmailQueue`: **Singleton** (مشترک برای کل برنامه)
- `BackgroundSmsService` & `BackgroundEmailService`: **HostedService** (خودکار)
- `ISmsService`: **Scoped** (هر scope جدید)
- `IEmailSender`: **Transient** (هر request جدید)

### 2. تغییرات در `AccountController.cs`

#### Constructor:
```csharp
public AccountController(
    // ...
    IEmailSender emailSender,
    IBackgroundEmailQueue emailQueue,  // ✨ جدید
    IBackgroundSmsQueue smsQueue        // ✨ جدید
)
{
    _emailSender = emailSender;
    _emailQueue = emailQueue;    // ✨ جدید
    _smsQueue = smsQueue;         // ✨ جدید
}
```

#### استفاده در Action‌ها:

**ForgotPassword:**
```csharp
// ❌ قبل:
await _emailSender.SendEmailAsync(model.Email, "بازنشانی کلمه عبور", body);

// ✅ بعد:
_emailQueue.QueueEmail(model.Email, "بازنشانی کلمه عبور", body);
```

**Register:**
```csharp
// ❌ قبل:
await _emailSender.SendEmailAsync(model.Email, "Confirm your email", body);

// ✅ بعد:
_emailQueue.QueueEmail(model.Email, "Confirm your email", body);
```

**ResendEmailConfirmation:**
```csharp
// ❌ قبل:
await _emailSender.SendEmailAsync(model.Email, "تایید آدرس ایمیل شما", body);

// ✅ بعد:
_emailQueue.QueueEmail(model.Email, "تایید آدرس ایمیل شما", body);
```

**Login (2FA SMS):**
```csharp
// ❌ قبل:
await smsService.SendSms($"کد امنیتی شما: {token.AuthenticationCode}", user.PhoneNumber);

// ✅ بعد:
_smsQueue.QueueSms($"کد امنیتی شما: {token.AuthenticationCode}", user.PhoneNumber);
```

**LoginWithSms:**
```csharp
// ❌ قبل:
await smsService.SendSms($"کد امنیتی شما: {code}", phone);

// ✅ بعد:
_smsQueue.QueueSms($"کد امنیتی شما: {code}", phone);
```

**PreRegister:**
```csharp
// ❌ قبل:
await smsService.SendSms($"کد امنیتی شما: {model.AuthenticationCode}", model.PhoneNumber);

// ✅ بعد:
_smsQueue.QueueSms($"کد امنیتی شما: {model.AuthenticationCode}", model.PhoneNumber);
```

### 3. تغییرات در `ManageController.cs`

#### Constructor:
```csharp
public ManageController(
    // ...
    IEmailSender emailSender,
    IBackgroundEmailQueue emailQueue,  // ✨ جدید
    IBackgroundSmsQueue smsQueue       // ✨ جدید
)
{
    _emailSender = emailSender;
    _emailQueue = emailQueue;    // ✨ جدید
    _smsQueue = smsQueue;         // ✨ جدید
}
```

#### استفاده در Action‌ها:

**Email (Change Email):**
```csharp
// ❌ قبل:
await _emailSender.SendEmailAsync(model.NewEmail, "Confirm your email", body);

// ✅ بعد:
_emailQueue.QueueEmail(model.NewEmail, "Confirm your email", body);
```

**SendVerificationEmail:**
```csharp
// ❌ قبل:
await _emailSender.SendEmailAsync(email, "Confirm your email", body);

// ✅ بعد:
_emailQueue.QueueEmail(email, "Confirm your email", body);
```

**AddPhoneNumber:**
```csharp
// ❌ قبل:
await smsService.SendSms($"کد امنیتی شما: {phoneTokenModel.AuthenticationCode}", model.PhoneNumber);

// ✅ بعد:
_smsQueue.QueueSms($"کد امنیتی شما: {phoneTokenModel.AuthenticationCode}", model.PhoneNumber);
```

---

## نحوه استفاده

### 1. ارسال SMS

```csharp
// تک‌نفره
_smsQueue.QueueSms("متن پیامک", "09123456789");

// چندنفره
var phones = new List<string> { "09123456789", "09187654321" };
_smsQueue.QueueSms("متن پیامک", phones);
```

### 2. ارسال Email

```csharp
_emailQueue.QueueEmail(
    "user@example.com",
    "عنوان ایمیل",
    "<p>محتوای HTML ایمیل</p>"
);
```

### 3. ✅ بدون await

```csharp
// ❌ اشتباه - نباید await کرد
await _smsQueue.QueueSms(...);
await _emailQueue.QueueEmail(...);

// ✅ درست - فوری برمی‌گردد
_smsQueue.QueueSms(...);
_emailQueue.QueueEmail(...);
return RedirectToAction("NextPage");
