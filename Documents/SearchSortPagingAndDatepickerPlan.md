# Implementation Plan: Search/Sort/Filter/Paging + Multi-Calendar Datepicker

## Overview

This document defines the architecture and per-view implementation plan for two cross-cutting improvements:

1. **Standardized search, filtering, sorting, and paging** on every admin table view, with full parent-state restoration when navigating back from a child page.
2. **Replacing all native date/datetime browser inputs** with the `multi-calendar-datepicker` jQuery plugin (Persian calendar by default) across the entire project.

---

## Part 1 — Search / Filter / Sort / Paging

### 1.1 Architecture Decisions

#### PagedResult\<T\> Model
A generic shared model class to be placed at `Models/System/PagedResult.cs`:

```csharp
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
```

Default `PageSize` = **20** unless the table is expected to have very few rows (e.g., AiModels, Roles → use 50).

#### Query Parameter Conventions
All table views accept these standard query-string parameters:

| Parameter | Type | Description |
|-----------|------|-------------|
| `q` | `string?` | Full-text search keyword |
| `sortBy` | `string?` | Column name to sort by (camelCase property name) |
| `sortDir` | `string?` | `"asc"` or `"desc"` (default: `"asc"`) |
| `page` | `int` | Current page number (default: 1) |
| `pageSize` | `int` | Records per page (default: 20) |
| (view-specific filter params) | varies | e.g., `userId`, `from`, `to`, `operationType` |

#### Sortable Column Header Pattern
Column headers that support sorting become anchor links:

```html
@{
    var newDir = (ViewBag.SortBy == "userName" && ViewBag.SortDir == "asc") ? "desc" : "asc";
    var icon = (ViewBag.SortBy == "userName") ? (ViewBag.SortDir == "asc" ? "▲" : "▼") : "";
}
<th>
    <a asp-action="Index" asp-all-route-data="sortRouteData" asp-route-sortBy="userName" asp-route-sortDir="@newDir">
        نام کاربری @icon
    </a>
</th>
```

A shared tag helper or HTML helper method should be created in `Helpers/SortLinkHelper.cs` to avoid repeating this logic per column.

#### Pagination Partial
A shared partial `Areas/Admin/Views/Shared/_Pagination.cshtml` renders the Bootstrap pagination component.  
It reads from a `PagedResult<T>` (passed via `ViewBag.Paged` or as part of the view model) and builds links that preserve all current query-string parameters via `asp-all-route-data`.

---

### 1.2 Parent-State Preservation — `returnUrl` Pattern

**The Problem:** When a user is on page 3 of the Users table, sorted by FullName descending, with search "احمد", clicking "Details" currently drops all that state. The back button goes to plain `Index` (page 1, no sort, no search).

**The Solution:** Encode the full parent URL (including all query-string state) into a `returnUrl` query parameter passed to every child page action link.

#### In the Parent View (Index):
```html
<!-- Build the current page's full URL as returnUrl -->
@{
    var returnUrl = Context.Request.GetEncodedPathAndQuery();
}
<a asp-action="Details" asp-route-id="@item.UserID"
   asp-route-returnUrl="@returnUrl">جزئیات</a>
```

#### In the Child Controller:
```csharp
public IActionResult Details(int id, string? returnUrl = null)
{
    ViewBag.ReturnUrl = returnUrl;
    // ... load data
}
```

#### In the Child View (Back Button):
```html
<a href="@(ViewBag.ReturnUrl ?? Url.Action("Index"))" class="btn btn-secondary">
    <i class="fa fa-arrow-right me-1"></i> بازگشت
</a>
```

**Security Note:** `returnUrl` must be validated to be a local URL before use:
```csharp
// In controller or a helper:
if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
    return Redirect(returnUrl);
return RedirectToAction("Index");
```

---

### 1.3 Shared Infrastructure to Create

| Artifact | Location | Purpose |
|----------|----------|---------|
| `PagedResult<T>` | `Models/System/PagedResult.cs` | Generic paged list model |
| `_Pagination.cshtml` | `Areas/Admin/Views/Shared/_Pagination.cshtml` | Bootstrap pagination partial |
| `SortLinkHelper` | `Helpers/SortLinkHelper.cs` (or Tag Helper) | DRY sortable column headers |
| `ControllerExtensions` | `Extensions/ControllerExtensions.cs` | `GetReturnUrl` helper |

---

### 1.4 Per-View Implementation Scope

#### Views That Need Search + Sort + Paging

| View | Current State | Search Param | Sortable Columns | Notes |
|------|---------------|-------------|-----------------|-------|
| `Users/Index` | Search (q), no sort/page | `q` | UserName, FullName, Email | `Take(∞)` → paged |
| `UserChats/Index` | Search (search), no sort/page | `q` | Username, SessionCount, TotalCredit | Query rebuilt with paging |
| `UserCredits/Index` | Search (search), no sort/page | `q` | Username, FullName, Balance | |
| `AiModels/Index` | None | `q` (Name) | ModelId, DisplayName | Small table (pageSize=50) |
| `AiModelPricings/Index` | None | `q` (model name) | ModelName, CreatedAt | |
| `ChatFeedback/Index` | Filter (onlyNegative), no sort/page | — | UserName, CreatedAt, Rating | Add paging; no free-text search |
| `ConversionRateSnapshots/Index` | None | — | SnapshotDate, Rate | Sorted by date desc by default |
| `CreditConsumption/Index` | Filters (userId, from, to, type), Take(500) | — | Amount, CreatedAt, OperationType | Replace Take(500) with paging |
| `DocumentAiPrompts/Index` | None | `q` (PromptKey) | PromptKey, ModelName | |
| `Roles/Index` | None | `q` (Name) | Name | Small table (pageSize=50) |

#### Views That Do NOT Need Paging (Small/Static Data)
| View | Reason |
|------|--------|
| `UserSessions/Index` | Real-time auto-refresh; paging would break live updates |
| `WelcomeGiftCredits/Index` | Single-record form, not a table |
| `ProfitMarginSettings/Index` | Append-only log; relatively small; add paging only if needed |
| `Notifications/Index` | Already has paging via ViewBag — migrate to PagedResult<T> pattern |

#### Parent → Child Navigation (returnUrl needed)

| Parent View | Child Views | State to Preserve |
|-------------|-------------|-------------------|
| `Users/Index` | Details, Edit, Delete, ChangePassword | q, sortBy, sortDir, page |
| `UserChats/Index` | Details (chat sessions) | q, sortBy, sortDir, page |
| `UserCredits/Index` | Details, Adjust | q, sortBy, sortDir, page |
| `AiModelPricings/Index` | History (per model) | sortBy, sortDir, page |
| `AiModelPricings/History` | Create (new pricing for that model) | page (of History) |

---

### 1.5 Controller Changes (Pattern per Controller)

```csharp
// Standard Index action signature (example: UsersController)
public async Task<IActionResult> Index(
    string? q = null,
    string? sortBy = "userName",
    string? sortDir = "asc",
    int page = 1,
    int pageSize = 20)
{
    var query = _dbContext.Users.AsQueryable();

    // 1. Filter
    if (!string.IsNullOrWhiteSpace(q))
        query = query.Where(u => u.UserName.Contains(q) || u.FullName.Contains(q));

    // 2. Sort
    query = sortBy switch
    {
        "fullName" => sortDir == "desc" ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
        _ => sortDir == "desc" ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
    };

    // 3. Count + Page
    var total = await query.CountAsync();
    var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

    // 4. Pass state to view
    ViewBag.Q = q;
    ViewBag.SortBy = sortBy;
    ViewBag.SortDir = sortDir;
    ViewBag.Paged = new PagedResult<ApplicationUser>
    {
        Items = items, Page = page, PageSize = pageSize, TotalCount = total
    };

    return View(items);
}
```

---

### 1.6 Filter Bar — FK Dropdown Rule

Whenever a table row has a foreign-key column (e.g. a plugin, a model, a user, an operation type), the filter bar **must** include a `<select>` dropdown to filter by that FK.

#### Pattern
- Load the FK list in the controller and expose it via `ViewBag` (e.g. `ViewBag.Plugins`, `ViewBag.AiModels`).
- Include a blank first option ("همه ..." — meaning *all*) so the filter is optional.
- Use `int? fkId = null` as the controller parameter; pass it through all sort/page links so it is preserved.
- Pre-select the active value by comparing the option value to `ViewBag.FkId` in the Razor loop.

#### Controller (example)
```csharp
// Load FK list for dropdown
ViewBag.Plugins = await _db.PluginDefinitions
    .Where(p => p.PluginType == PluginType.ConversionRate)
    .OrderBy(p => p.DisplayName)
    .ToListAsync();

ViewBag.PluginDefinitionId = pluginDefinitionId;
```

#### View (example)
```html
<select name="pluginDefinitionId" class="form-select form-select-sm">
    <option value="">همه منابع</option>
    @foreach (var p in (List<PluginDefinition>)ViewBag.Plugins)
    {
        if ((int?)ViewBag.PluginDefinitionId == p.PluginDefinitionID)
        {
            <option value="@p.PluginDefinitionID" selected>@p.DisplayName</option>
        }
        else
        {
            <option value="@p.PluginDefinitionID">@p.DisplayName</option>
        }
    }
</select>
```

> **RZ1031 note:** Never use inline C# expressions in `<option>` attribute position (e.g. `@(condition ? "selected" : "")`). Always use an `if/else` block to emit the `selected` attribute, as shown above.

---

## Part 2 — Multi-Calendar Datepicker Integration

### 2.1 Library Location
```
wwwroot/lib/multi-calendar-datepicker/lib/
├── multi-calendar-datepicker.js
├── multi-calendar-datepicker.css
└── moment-bundled.js
```

### 2.2 Standard Initialization Snippet

#### Persian Date-Only (default for all date fields in this project)
```javascript
$('#myDateInput').multiCalendarDatePicker({
    calendar: 'persian',
    locale: 'fa',
    format: 'YYYY/MM/DD',
    rtl: true,
    theme: 'light'  // or 'dark' based on user theme
});
```

#### Persian DateTime (for fields that need time — e.g., EffectiveFrom)
```javascript
$('#myDateTimeInput').multiCalendarDatePicker({
    calendar: 'persian',
    locale: 'fa',
    format: 'YYYY/MM/DD HH:mm',
    rtl: true,
    timePicker: true,
    timeFormat: 'HH:mm',
    use24Hour: true
});
```

### 2.3 Layout / Partial for Scripts
Add a `_DatePickerScripts.cshtml` partial at `Areas/Admin/Views/Shared/_DatePickerScripts.cshtml`:

```html
<!-- Include once in layout or per-view via @section Scripts -->
<link rel="stylesheet" href="~/lib/multi-calendar-datepicker/lib/multi-calendar-datepicker.css" />
<script src="~/lib/multi-calendar-datepicker/lib/moment-bundled.js"></script>
<script src="~/lib/multi-calendar-datepicker/lib/multi-calendar-datepicker.js"></script>
```

These three lines should be added to the Admin layout (`_Layout.cshtml`) so every Admin view gets the datepicker without per-view script references.

### 2.4 Value Format & Controller Binding

**`PersianDateModelBinder` is registered globally** in `Program.cs` via `options.ModelBinderProviders.Insert(0, new PersianDateModelBinderProvider())`.  
It parses Persian date strings (`yyyy/MM/dd`, `yyyy/MM/dd HH:mm`) from `fa-IR` culture into `DateTime` automatically.

This means **no hidden Gregorian field is needed**. Give the datepicker input `name="from"` / `name="to"` directly:

```html
<input type="text" id="fromDisplay" name="from"
       class="form-control mcdp-rtl-input"
       value="@fromDisplay" autocomplete="off" />
```

The controller simply declares `DateTime? from` / `DateTime? to` as action parameters — the binder handles conversion. No hidden fields, no client-side Moment.js conversion.

```csharp
public async Task<IActionResult> Index(DateTime? from = null, DateTime? to = null, ...)
```

```javascript
// Only initialization needed — no mcdp:change handler required
$('#fromDisplay').multiCalendarDatePicker({ calendar: 'persian', locale: 'fa', format: 'YYYY/MM/DD', rtl: true });
```

> **Pre-populating the datepicker:**
> - For existing filter values: set `value="@currentFrom.Value.ToPersianDateString()"`.
> - For **placeholder** (hint text when empty): use the static helpers from `PersianDateExtensionMethods` so the hint always reflects the current Persian month rather than a hardcoded year:
>   ```html
>   placeholder="مثال: @PersianDateExtensionMethods.CurrentPersianMonthStartString()"
>   placeholder="مثال: @PersianDateExtensionMethods.CurrentPersianMonthEndString()"
>   ```
>   See `Classes/Extensions/PersianDateExtensionMethods.cs` for `CurrentPersianMonthStartString()`, `CurrentPersianMonthEndString()`, `PersianMonthStart(this DateTime)`, `PersianMonthEnd(this DateTime)`.
> - Full Persian DateTime guide: `Documents/PersianDateTimeGuide.md`.

### 2.5 Views Requiring Datepicker Changes

| View | Input(s) | Current Type | Replacement | Notes |
|------|----------|-------------|-------------|-------|
| `CreditConsumption/Index.cshtml` | `from`, `to` | `type="date"` | Persian date-only | Filter dates; use hidden Gregorian fields |
| `CreditConsumption/Report.cshtml` | `from`, `to` | `type="date"` | Persian date-only | Same as above |
| `AiModelPricings/Create.cshtml` | `EffectiveFrom` | `type="datetime-local"` | Persian datetime | timePicker: true |

**Future rule:** Every new form input requiring a date or date+time **must** use the datepicker (see instructions update below). No native `type="date"` or `type="datetime-local"` inputs are allowed.

---

## Part 3 — Implementation Order

### Phase 1 — Shared Infrastructure
1. Create `Models/System/PagedResult.cs`
2. Create `Areas/Admin/Views/Shared/_Pagination.cshtml`
3. Create `Helpers/SortLinkHelper.cs` (or Tag Helper)
4. Add datepicker CSS/JS references to `Areas/Admin/Views/Shared/_Layout.cshtml`

### Phase 2 — High-Traffic Views (Users, UserChats, UserCredits)
5. Update `UsersController.Index` + `Users/Index.cshtml` (search + sort + paging + returnUrl on child links)
6. Update `UserChatsController.Index` + `UserChats/Index.cshtml`
7. Update `UserCreditsController.Index` + `UserCredits/Index.cshtml`
8. Update `Users/Details.cshtml`, `UserChats/Details.cshtml`, `UserCredits/Details.cshtml` back buttons

### Phase 3 — Data/Finance Views
9. Update `CreditConsumptionController.Index` + `CreditConsumption/Index.cshtml` (paging + datepicker)
10. Update `CreditConsumption/Report.cshtml` (datepicker)
11. Update `AiModelPricings/Index.cshtml` + History + Create (paging + returnUrl + datepicker)

### Phase 4 — Remaining Admin Views
12. Update `AiModels/Index.cshtml` + controller
13. Update `ChatFeedback/Index.cshtml` + controller
14. Update `ConversionRateSnapshots/Index.cshtml` + controller
15. Update `DocumentAiPrompts/Index.cshtml` + controller
16. Update `Roles/Index.cshtml` + controller
17. Migrate `Notifications/Index` paging from ViewBag to `PagedResult<T>`

---

## Part 4 — Quality Checklist (Per View)

- [ ] Controller uses `PagedResult<T>` with correct `CountAsync` + `Skip/Take`
- [ ] All current filter/search params preserved in sortable column links
- [ ] All current filter/search params preserved in pagination links
- [ ] FK columns have a `<select>` dropdown filter; list loaded via `ViewBag`; FK value preserved in sort/page links
- [ ] FK dropdown options use `if/else` for `selected` — no inline `@(condition ? "selected" : "")` (RZ1031)
- [ ] Page size selector present (10 / 20 / 50 / 100); current value pre-selected; preserved in sort links
- [ ] Child action links include `asp-route-returnUrl` with encoded current URL
- [ ] Child page back button uses `ViewBag.ReturnUrl` with `Url.IsLocalUrl` guard
- [ ] No native `type="date"` or `type="datetime-local"` inputs remain
- [ ] Datepicker inputs carry `name=` directly; no hidden Gregorian field; `PersianDateModelBinder` handles conversion
- [ ] Datepicker initialized with Persian calendar + RTL; pre-populated via `date.ToPersianDateString()`
- [ ] Page resets to 1 when search/filter form is submitted
