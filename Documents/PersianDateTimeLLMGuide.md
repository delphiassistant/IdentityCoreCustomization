# LLM Guide: Persian Date/Time End-to-End Integration (AnyGPT Pattern)

## Purpose

This guide documents **all Persian datetime-related parts** used in this project and how to reproduce them in a destination project with the same behavior.

It covers:

- Runtime culture and calendar behavior
- MVC model binding for Persian date strings
- Admin UI datepicker integration (Persian + RTL)
- View/controller patterns for filters and datetime input
- Optional logging enrichment with Persian timestamps

---

## 1) Current Persian DateTime Architecture in This Project

### 1.1 Runtime culture (global)
- File: `Program.cs`
- Behavior: Sets default thread culture to Persian (`fa-IR`) using the custom culture builder.

```csharp
CultureInfo.DefaultThreadCurrentCulture
                = CultureInfo.DefaultThreadCurrentUICulture = PersianDateExtensionMethods.GetPersianCulture();
```

### 1.2 Persian culture + calendar helpers
- File: `Classes/Extensions/PersianDateExtensionMethods.cs`
- Provides:
  - `GetPersianCulture()`
  - `ToPersianDateString(...)`
  - `ToPersianFullDateTimeString(...)`
  - `IranTimeZone`
  - Persian month boundary helpers:
    - `PersianMonthStart(...)`
    - `PersianMonthEnd(...)`
    - `CurrentPersianMonthStartString(...)`
    - `CurrentPersianMonthEndString(...)`

### 1.3 MVC model binder for Persian date inputs
- Files:
  - `Classes/ModelBinding/PersianDateModelBinder.cs`
  - `Classes/ModelBinding/PersianDateModelBinderProvider.cs`
- Registration in: `Program.cs`

```csharp
services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new PersianDateModelBinderProvider());
});
```

This lets action parameters like `DateTime? from, DateTime? to` bind from strings like:

- `yyyy/MM/dd`
- `yyyy/MM/dd HH:mm`

### 1.4 Persian datepicker in Admin layout
- Library location:
  - `wwwroot/lib/multi-calendar-datepicker/lib/multi-calendar-datepicker.js`
  - `wwwroot/lib/multi-calendar-datepicker/lib/multi-calendar-datepicker.css`
  - `wwwroot/lib/multi-calendar-datepicker/lib/moment-bundled.js`
- References in: `Areas/Admin/Views/Shared/_Layout.cshtml`

```html
<link rel="stylesheet" href="~/lib/multi-calendar-datepicker/lib/multi-calendar-datepicker.css" />
...
<script src="~/lib/multi-calendar-datepicker/lib/moment-bundled.js"></script>
<script src="~/lib/multi-calendar-datepicker/lib/multi-calendar-datepicker.js"></script>
```

### 1.5 Logging enrichment (optional but implemented)
- File: `Classes/Logging/PersianTimestampEnricher.cs`
- Registered in `Program.cs` (bootstrap + runtime Serilog pipeline)
- Adds:
  - `PersianTimestamp` for message templates
  - `PersianDate` for file naming (ASCII digits safe for file names)

---

## 2) Files to Add in a Destination Project

Add these files (or equivalent) if destination project does not already have them:

| Type | File | Required | Why |
|---|---|---|---|
| C# class | `Classes/Extensions/PersianDateExtensionMethods.cs` | Yes | Central Persian culture/timezone/date formatting helpers |
| C# class | `Classes/ModelBinding/PersianDateModelBinder.cs` | Yes | Parses Persian date/datetime strings from request |
| C# class | `Classes/ModelBinding/PersianDateModelBinderProvider.cs` | Yes | Hooks binder into MVC |
| Static assets | `wwwroot/lib/multi-calendar-datepicker/lib/*` | Yes (UI projects) | Persian calendar datepicker support |
| C# class | `Classes/Logging/PersianTimestampEnricher.cs` | Optional | Persian timestamps in logs |
| Guide doc | `Documents/PersianDateTimeLLMGuide.md` | Recommended | Implementation contract for future LLM/dev work |

> If your destination project is API-only (no server-rendered UI), the datepicker assets are not required.

---

## 3) Files to Edit in a Destination Project

| File | Required Change |
|---|---|
| `Program.cs` | Set default thread culture to Persian using `PersianDateExtensionMethods.GetPersianCulture()` |
| `Program.cs` | Register `PersianDateModelBinderProvider` via `AddControllersWithViews(options => ...)` |
| `Areas/Admin/Views/Shared/_Layout.cshtml` (or equivalent shared layout) | Add datepicker CSS/JS references once globally |
| Date filter views (`.cshtml`) | Replace native `type="date"` / `type="datetime-local"` with datepicker-enabled text inputs |
| Related controllers | Accept `DateTime?` params directly, rely on model binder, and normalize to UTC for DB filtering |

---

## 4) Required View Pattern (Persian Date/DateTime Inputs)

### 4.1 Do not use native date input types
Avoid:

- `type="date"`
- `type="datetime-local"`

Use `type="text"` with datepicker initialization instead.

### 4.2 Date-only initialization
```javascript
$('#from').multiCalendarDatePicker({
    calendar: 'persian',
    locale: 'fa',
    format: 'YYYY/MM/DD',
    rtl: true
});
```

### 4.3 DateTime initialization
```javascript
$('#effectiveFrom').multiCalendarDatePicker({
    calendar: 'persian',
    locale: 'fa',
    format: 'YYYY/MM/DD HH:mm',
    rtl: true,
    timePicker: true,
    timeFormat: 'HH:mm',
    use24Hour: true
});
```

### 4.4 Binding rule
- Input `name` must match action parameter (`from`, `to`, `effectiveFrom`, ...).
- No hidden Gregorian field is required.
- Use `ToPersianDateString()` to pre-fill displayed values.

---

## 5) Required Controller Pattern

### 5.1 Action signatures
Use nullable `DateTime?` parameters directly:

```csharp
public async Task<IActionResult> Index(DateTime? from, DateTime? to)
```

### 5.2 Normalize for UTC-stored database values
Typical filtering pattern:

```csharp
if (from.HasValue)
    query = query.Where(x => x.CreatedAt >= from.Value.ToUniversalTime());

if (to.HasValue)
    query = query.Where(x => x.CreatedAt <= to.Value.ToUniversalTime().AddDays(1));
```

### 5.3 Populate back to view as Persian string
- For date-only controls: `from?.ToPersianDateString()`
- For datetime controls: `value?.ToPersianDateString("yyyy/MM/dd HH:mm")`

---

## 6) Concrete Destination-Project Change Checklist

Use this exact checklist when porting to another MVC project:

1. Copy/add `PersianDateExtensionMethods`.
2. Copy/add `PersianDateModelBinder` and `PersianDateModelBinderProvider`.
3. Edit `Program.cs`:
   - set default thread culture with `GetPersianCulture()`
   - register binder provider at index `0`
4. Copy datepicker library under `wwwroot/lib/multi-calendar-datepicker/lib/`.
5. Add datepicker references in shared Admin layout.
6. Replace all date inputs in admin views with text + datepicker init.
7. Ensure date input names map directly to controller parameters.
8. Convert filtering boundaries to UTC in controller queries.
9. Use `ToPersianDateString(...)` for pre-populated display values.
10. (Optional) add `PersianTimestampEnricher` and wire Serilog to Persian log timestamps.

---

## 7) Validation Checklist (Must Pass)

- Persian date (`1404/01/15`) binds to `DateTime?` action parameter.
- Persian datetime (`1404/01/15 14:30`) binds correctly.
- Form submit round-trip keeps Persian value format in input.
- DB query results match expected UTC range.
- No native `type="date"`/`type="datetime-local"` remains in migrated admin views.
- Datepicker assets load once from shared layout (no duplicate includes per view).
- RTL rendering is correct in admin pages.

---

## 8) Project-Specific Notes for AnyGPT

- Existing shared plan file: `Documents/SearchSortPagingAndDatepickerPlan.md`
- Existing implementation files already present:
  - `Classes/Extensions/PersianDateExtensionMethods.cs`
  - `Classes/ModelBinding/PersianDateModelBinder.cs`
  - `Classes/ModelBinding/PersianDateModelBinderProvider.cs`
  - `Areas/Admin/Views/Shared/_Layout.cshtml`
  - `wwwroot/lib/multi-calendar-datepicker/lib/*`
  - `Classes/Logging/PersianTimestampEnricher.cs`

When using this repo as the source pattern, keep those files and replicate the same integration points in the destination project.
