# Admin Area Design Guidelines

**Purpose**: Comprehensive design and development guide for creating new controllers and views in the Admin area.  
**Target Framework**: .NET 10  
**Last Updated**: January 2026  
**Status**: Production Ready

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Design System](#design-system)
3. [Layout Structure](#layout-structure)
4. [View Patterns](#view-patterns)
5. [Component Library](#component-library)
6. [JavaScript Patterns](#javascript-patterns)
7. [Controller Patterns](#controller-patterns)
8. [Best Practices](#best-practices)
9. [Examples](#examples)

---

## 1. Overview

### Design Philosophy

The admin area follows these core principles:

- **Modern & Professional**: Gradient backgrounds, smooth animations, card-based layouts
- **Consistent**: Reusable components, standard patterns across all views
- **User-Friendly**: Clear feedback, helpful messages, intuitive navigation
- **Responsive**: Mobile-first design with collapsible sidebar
- **RTL Support**: Full Persian/Farsi text support
- **Accessible**: Proper ARIA labels, keyboard navigation, screen reader support

### Technology Stack

- **CSS Framework**: Bootstrap 5.3 RTL
- **Icons**: FontAwesome 6
- **Fonts**: Vazirmatn (Persian)
- **JavaScript**: Vanilla JS with Bootstrap 5 components
- **Backend**: ASP.NET Core MVC (.NET 10)

---

## 2. Design System

### Color Palette

```css
:root {
  --admin-primary: #4f46e5;        /* Indigo */
  --admin-primary-dark: #4338ca;   /* Dark Indigo */
  --admin-secondary: #7c3aed;      /* Purple */
  --admin-success: #10b981;        /* Green */
  --admin-danger: #ef4444;         /* Red */
  --admin-warning: #f59e0b;        /* Amber */
  --admin-info: #3b82f6;           /* Blue */
}
```

### Typography

- **Headers**: Vazirmatn font, bold weights
- **Body**: Vazirmatn font, regular weight
- **Code**: Monospace font for IDs and technical values

### Spacing

- **Card margins**: `1.5rem` bottom
- **Section padding**: `1.5rem`
- **Button padding**: `0.5rem 1rem` (regular), `0.35rem 0.75rem` (small)
- **Form control padding**: `0.625rem 0.875rem`

---

## 3. Layout Structure

### Required Layout File

All admin views must use the admin layout:

```razor
@{
    Layout = "~/Areas/Admin/Views/Shared/_Layout.cshtml";
}
```

### Layout Components

The admin layout provides:

1. **Fixed Sidebar** (right side, 260px wide)
   - Navigation menu
   - Collapsible on mobile
   - Gradient background
   - Active state indicators

2. **Top Navbar** (56px height)
   - Gradient background matching auth pages
   - Breadcrumb navigation
   - User info

3. **Content Area**
   - Main content with padding
   - Responsive grid system
   - Footer at bottom

### Content Structure Template

```razor
@{
    ViewData["Title"] = "Page Title";
    Layout = "~/Areas/Admin/Views/Shared/_Layout.cshtml";
}

@section Styles {
    <!-- Page-specific styles -->
}

<!-- Page Header -->
<div class="page-header">
    <h1>
        <i class="fa-solid fa-icon-name"></i> @ViewData["Title"]
    </h1>
    <p class="mb-0 mt-2 opacity-75">Brief description</p>
</div>

<!-- Alert Messages -->
@if (TempData["SuccessMessage"] != null) { /* Success alert */ }
@if (TempData["ErrorMessage"] != null) { /* Error alert */ }

<!-- Toolbars / Actions -->
<div class="search-toolbar">
    <!-- Search, filters, action buttons -->
</div>

<!-- Main Content -->
<div class="card">
    <div class="card-body">
        <!-- Content here -->
    </div>
</div>

@section Scripts {
    <!-- Page-specific scripts -->
}
```

---

## 4. View Patterns

### 4.1 Index/List View Pattern

**Purpose**: Display list of records with search, filter, and actions.

**Key Components**:
- Gradient page header
- Search toolbar with filters
- Data table with sortable columns
- Action buttons (view, edit, delete)
- Status badges
- Pagination (if needed)
- Confirmation modals

**Example Structure**:

```razor
<!-- Page Header with Gradient -->
<div class="page-header">
    <h1 class="mb-0">
        <i class="fa-solid fa-users"></i> مدیریت کاربران
    </h1>
    <p class="mb-0 mt-2 opacity-75">مدیریت و کنترل کامل کاربران سیستم</p>
</div>

<!-- Search Toolbar -->
<div class="search-toolbar">
    <div class="row align-items-center g-3">
        <div class="col-md-4">
            <a asp-action="Create" class="btn btn-success">
                <i class="fa-solid fa-plus"></i> ایجاد جدید
            </a>
        </div>
        <div class="col-md-5">
            <form method="get" asp-action="Index">
                <input type="text" name="q" class="form-control" placeholder="جستجو..." />
                <button type="submit" class="btn btn-primary">
                    <i class="fa-solid fa-magnifying-glass"></i> جستجو
                </button>
            </form>
        </div>
        <div class="col-md-3 text-end">
            <span class="badge bg-info fs-6">
                <i class="fa-solid fa-list-check"></i> تعداد: @Model.Count()
            </span>
        </div>
    </div>
</div>

<!-- Data Table -->
<div class="card">
    <div class="card-body p-0">
        <div class="table-responsive">
            <table class="table table-striped table-hover mb-0">
                <thead class="table-dark">
                    <tr>
                        <th>شناسه</th>
                        <th>نام</th>
                        <th>وضعیت</th>
                        <th class="text-center">دستورات</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in Model)
                    {
                        <tr>
                            <td><code>@item.Id</code></td>
                            <td>@item.Name</td>
                            <td>
                                <span class="badge bg-success">فعال</span>
                            </td>
                            <td>
                                <div class="btn-group" role="group">
                                    <a asp-action="Details" asp-route-id="@item.Id" 
                                       class="btn btn-info btn-sm">
                                        <i class="fa-solid fa-eye"></i>
                                    </a>
                                    <a asp-action="Edit" asp-route-id="@item.Id" 
                                       class="btn btn-primary btn-sm">
                                        <i class="fa-solid fa-pen-to-square"></i>
                                    </a>
                                    <a asp-action="Delete" asp-route-id="@item.Id" 
                                       class="btn btn-danger btn-sm">
                                        <i class="fa-solid fa-trash"></i>
                                    </a>
                                </div>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>
</div>

<!-- Empty State -->
@if (!Model.Any())
{
    <div class="alert alert-info text-center">
        <i class="fa-solid fa-info-circle fs-3 d-block mb-2"></i>
        <strong>هیچ رکوردی یافت نشد.</strong>
        <p class="mb-0 mt-2">برای شروع، یک مورد جدید ایجاد کنید.</p>
    </div>
}
```

### 4.2 Create/Edit Form Pattern

**Purpose**: Forms for creating or editing records.

**Key Components**:
- Page header with icon
- Form card with sections
- Validation messages
- Submit and cancel buttons
- Help text for fields

**Example Structure**:

```razor
<div class="page-header">
    <h1>
        <i class="fa-solid fa-plus"></i> ایجاد جدید
    </h1>
</div>

<div class="row">
    <div class="col-lg-8">
        <div class="card">
            <div class="card-header">
                <h5 class="mb-0">
                    <i class="fa-solid fa-pen"></i> اطلاعات پایه
                </h5>
            </div>
            <div class="card-body">
                <form asp-action="Create" method="post">
                    @Html.AntiForgeryToken()
                    
                    <!-- Validation Summary -->
                    <div asp-validation-summary="ModelOnly" class="alert alert-danger"></div>
                    
                    <!-- Form Fields -->
                    <div class="mb-3">
                        <label asp-for="Name" class="form-label">
                            <i class="fa-solid fa-tag text-primary"></i>
                            @Html.DisplayNameFor(m => m.Name)
                        </label>
                        <input asp-for="Name" class="form-control" />
                        <span asp-validation-for="Name" class="text-danger small"></span>
                        <small class="form-text text-muted">
                            <i class="fa-solid fa-info-circle"></i> راهنما: متن کمکی
                        </small>
                    </div>
                    
                    <!-- Action Buttons -->
                    <div class="d-flex gap-2">
                        <button type="submit" class="btn btn-success">
                            <i class="fa-solid fa-check"></i> ذخیره
                        </button>
                        <a asp-action="Index" class="btn btn-secondary">
                            <i class="fa-solid fa-times"></i> انصراف
                        </a>
                    </div>
                </form>
            </div>
        </div>
    </div>
    
    <!-- Sidebar with Tips -->
    <div class="col-lg-4">
        <div class="card">
            <div class="card-header">
                <h5 class="mb-0">
                    <i class="fa-solid fa-lightbulb text-warning"></i> راهنمایی
                </h5>
            </div>
            <div class="card-body">
                <ul class="small mb-0">
                    <li>نکته اول</li>
                    <li>نکته دوم</li>
                </ul>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### 4.3 Details View Pattern

**Purpose**: Display detailed information about a single record.

**Example Structure**:

```razor
<div class="page-header">
    <h1>
        <i class="fa-solid fa-info-circle"></i> جزئیات
    </h1>
</div>

<div class="card">
    <div class="card-header">
        <h5 class="mb-0">اطلاعات کامل</h5>
    </div>
    <div class="card-body">
        <dl class="row">
            <dt class="col-sm-3">
                <i class="fa-solid fa-tag text-primary"></i>
                @Html.DisplayNameFor(model => model.Name)
            </dt>
            <dd class="col-sm-9">@Model.Name</dd>
            
            <!-- More fields -->
        </dl>
        
        <div class="d-flex gap-2 mt-3">
            <a asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-primary">
                <i class="fa-solid fa-pen-to-square"></i> ویرایش
            </a>
            <a asp-action="Index" class="btn btn-secondary">
                <i class="fa-solid fa-arrow-right"></i> بازگشت
            </a>
        </div>
    </div>
</div>
```

### 4.4 Delete Confirmation Pattern

**Purpose**: Confirm deletion with details about what will be deleted.

**Example Structure**:

```razor
<div class="page-header">
    <h1 class="text-danger">
        <i class="fa-solid fa-trash"></i> حذف
    </h1>
</div>

<div class="alert alert-danger">
    <i class="fa-solid fa-triangle-exclamation"></i>
    <strong>هشدار:</strong> این عمل قابل بازگشت نیست!
</div>

<div class="card">
    <div class="card-header bg-danger text-white">
        <h5 class="mb-0">آیا مطمئن هستید؟</h5>
    </div>
    <div class="card-body">
        <p>موارد زیر حذف خواهند شد:</p>
        
        <dl class="row">
            <dt class="col-sm-3">شناسه</dt>
            <dd class="col-sm-9"><code>@Model.Id</code></dd>
            
            <dt class="col-sm-3">نام</dt>
            <dd class="col-sm-9">@Model.Name</dd>
        </dl>
        
        <form asp-action="Delete" method="post">
            @Html.AntiForgeryToken()
            <input type="hidden" asp-for="Id" />
            
            <div class="d-flex gap-2">
                <button type="submit" class="btn btn-danger">
                    <i class="fa-solid fa-trash"></i> بله، حذف شود
                </button>
                <a asp-action="Index" class="btn btn-secondary">
                    <i class="fa-solid fa-times"></i> انصراف
                </a>
            </div>
        </form>
    </div>
</div>
```

---

## 5. Component Library

### 5.1 Page Header (Gradient Style)

**Usage**: Top of every page with title and description.

```razor
<div class="page-header">
    <h1 class="mb-0">
        <i class="fa-solid fa-icon-name"></i> عنوان صفحه
    </h1>
    <p class="mb-0 mt-2 opacity-75">توضیح مختصر</p>
</div>
```

**CSS** (add to page via `@section Styles`):

```css
.page-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 2rem;
    border-radius: 1rem;
    margin-bottom: 2rem;
    box-shadow: 0 8px 16px rgba(102, 126, 234, 0.3);
}

.page-header h1 {
    color: white;
    border: none;
    margin: 0;
    padding: 0;
}

.page-header h1::after {
    display: none;
}
```

### 5.2 Search Toolbar

**Usage**: Action buttons, search box, and filters.

```razor
<div class="search-toolbar">
    <div class="row align-items-center g-3">
        <div class="col-md-4">
            <a asp-action="Create" class="btn btn-success">
                <i class="fa-solid fa-plus"></i> ایجاد جدید
            </a>
        </div>
        <div class="col-md-5">
            <form method="get" asp-action="Index" class="d-flex">
                <input type="text" name="q" class="form-control me-2" placeholder="جستجو..." />
                <button type="submit" class="btn btn-primary">
                    <i class="fa-solid fa-magnifying-glass"></i>
                </button>
            </form>
        </div>
        <div class="col-md-3 text-end">
            <span class="badge bg-info fs-6">
                <i class="fa-solid fa-list-check"></i> تعداد: @Model.Count()
            </span>
        </div>
    </div>
</div>
```

**CSS**:

```css
.search-toolbar {
    background: white;
    padding: 1.5rem;
    border-radius: 0.75rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
    margin-bottom: 1.5rem;
}
```

### 5.3 Alert Messages

**Usage**: Success, error, warning, info messages from TempData.

```razor
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success alert-dismissible fade show">
        <i class="fa-solid fa-circle-check"></i> @TempData["SuccessMessage"]
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>
}

@if (TempData["ErrorMessage"] != null)
{
    <div class="alert alert-danger alert-dismissible fade show">
        <i class="fa-solid fa-circle-exclamation"></i> @TempData["ErrorMessage"]
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>
}
```

**Auto-dismiss JavaScript** (add to `@section Scripts`):

```javascript
setTimeout(function() {
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function(alert) {
        const bsAlert = new bootstrap.Alert(alert);
        bsAlert.close();
    });
}, 5000);
```

### 5.4 Status Badges

**Usage**: Display status with color-coded badges.

```razor
<!-- Success/Active -->
<span class="badge bg-success">
    <i class="fa-solid fa-circle-check"></i> فعال
</span>

<!-- Danger/Locked -->
<span class="badge bg-danger">
    <i class="fa-solid fa-lock"></i> قفل
</span>

<!-- Warning -->
<span class="badge bg-warning text-dark">
    <i class="fa-solid fa-triangle-exclamation"></i> هشدار
</span>

<!-- Info -->
<span class="badge bg-info">
    <i class="fa-solid fa-shield-halved"></i> 2FA
</span>

<!-- Secondary/No Role -->
<span class="badge bg-secondary">
    <i class="fa-solid fa-ban"></i> بدون نقش
</span>
```

### 5.5 Action Button Groups

**Usage**: View, edit, delete buttons in tables.

```razor
<div class="btn-group" role="group">
    <a asp-action="Details" asp-route-id="@item.Id" 
       class="btn btn-info btn-sm" title="مشاهده">
        <i class="fa-solid fa-eye"></i>
    </a>
    
    <a asp-action="Edit" asp-route-id="@item.Id" 
       class="btn btn-primary btn-sm" title="ویرایش">
        <i class="fa-solid fa-pen-to-square"></i>
    </a>
    
    <a asp-action="Delete" asp-route-id="@item.Id" 
       class="btn btn-danger btn-sm" title="حذف">
        <i class="fa-solid fa-trash"></i>
    </a>
</div>
```

### 5.6 Confirmation Modals

**Usage**: Confirm dangerous actions (delete, lock, etc.).

```razor
<!-- Modal HTML -->
<div class="modal fade" id="confirmModal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header bg-danger text-white">
                <h5 class="modal-title">
                    <i class="fa-solid fa-trash"></i> تأیید حذف
                </h5>
                <button type="button" class="btn-close btn-close-white" 
                        data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <div class="alert alert-danger">
                    <i class="fa-solid fa-exclamation-triangle"></i>
                    آیا مطمئن هستید؟
                </div>
                <p id="confirmMessage">این عمل قابل بازگشت نیست.</p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                    <i class="fa-solid fa-times"></i> انصراف
                </button>
                <form id="confirmForm" method="post" style="display: inline;">
                    @Html.AntiForgeryToken()
                    <button type="submit" class="btn btn-danger">
                        <i class="fa-solid fa-trash"></i> تأیید
                    </button>
                </form>
            </div>
        </div>
    </div>
</div>
```

### 5.7 Empty State

**Usage**: When no records exist.

```razor
@if (!Model.Any())
{
    <div class="alert alert-info text-center">
        <i class="fa-solid fa-info-circle fs-3 d-block mb-2"></i>
        <strong>هیچ رکوردی یافت نشد.</strong>
        <p class="mb-0 mt-2">برای شروع، یک مورد جدید ایجاد کنید.</p>
    </div>
}
```

---

## 6. JavaScript Patterns

### 6.1 Modal Confirmation Pattern

**Purpose**: Show Bootstrap modal for confirmations.

```javascript
// Initialize modal on button click
document.querySelectorAll('.action-button').forEach(function(button) {
    button.addEventListener('click', function(e) {
        e.preventDefault();
        
        const action = this.getAttribute('data-action');
        const itemId = this.getAttribute('data-item-id');
        const itemName = this.getAttribute('data-item-name');
        
        showConfirmModal(action, itemId, itemName);
    });
});

function showConfirmModal(action, itemId, itemName) {
    // Set modal content
    document.getElementById('confirmMessage').textContent = 
        `آیا مطمئن هستید که می‌خواهید ${itemName} را ${action} کنید؟`;
    
    // Set form action
    const form = document.getElementById('confirmForm');
    form.action = `/Admin/Controller/Action/${itemId}`;
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('confirmModal'));
    modal.show();
}
```

### 6.2 Form Loading State

**Purpose**: Show loading state during form submission.

```javascript
document.querySelector('form').addEventListener('submit', function(e) {
    const submitBtn = this.querySelector('button[type="submit"]');
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> در حال پردازش...';
    }
});
```

### 6.3 Auto-dismiss Alerts

**Purpose**: Automatically hide alerts after 5 seconds.

```javascript
setTimeout(function() {
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function(alert) {
        const bsAlert = new bootstrap.Alert(alert);
        try {
            bsAlert.close();
        } catch (e) {
            console.log('Alert already closed');
        }
    });
}, 5000);
```

### 6.4 Tooltip Initialization

**Purpose**: Initialize Bootstrap tooltips on elements with `title` attribute.

```javascript
document.addEventListener('DOMContentLoaded', function() {
    const tooltipTriggerList = [].slice.call(
        document.querySelectorAll('[title]')
    );
    
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
});
```

---

## 7. Controller Patterns

### 7.1 Standard CRUD Controller

```csharp
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ExampleController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public ExampleController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // GET: Index - List all
    public async Task<IActionResult> Index(string q)
    {
        var items = _context.Items.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(q))
        {
            items = items.Where(i => i.Name.Contains(q));
            ViewBag.Search = q;
        }
        
        return View(await items.ToListAsync());
    }
    
    // GET: Details
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        
        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();
        
        return View(item);
    }
    
    // GET: Create
    public IActionResult Create()
    {
        return View();
    }
    
    // POST: Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExampleModel model)
    {
        if (ModelState.IsValid)
        {
            _context.Add(model);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "مورد با موفقیت ایجاد شد.";
            return RedirectToAction(nameof(Index));
        }
        
        return View(model);
    }
    
    // GET: Edit
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        
        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();
        
        return View(item);
    }
    
    // POST: Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExampleModel model)
    {
        if (id != model.Id) return NotFound();
        
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "تغییرات با موفقیت ذخیره شد.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(model.Id))
                    return NotFound();
                else
                    throw;
            }
            
            return RedirectToAction(nameof(Index));
        }
        
        return View(model);
    }
    
    // GET: Delete
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        
        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();
        
        return View(item);
    }
    
    // POST: Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Items.FindAsync(id);
        
        if (item != null)
        {
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "مورد با موفقیت حذف شد.";
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    private bool ItemExists(int id)
    {
        return _context.Items.Any(e => e.Id == id);
    }
}
```

### 7.2 TempData Messages Pattern

```csharp
// Success message
TempData["SuccessMessage"] = "عملیات با موفقیت انجام شد.";

// Error message
TempData["ErrorMessage"] = "خطا در انجام عملیات.";

// Warning message
TempData["WarningMessage"] = "هشدار: این عمل ممکن است...";

// Info message
TempData["InfoMessage"] = "توجه: لطفاً...";
```

---

## 8. Best Practices

### 8.1 Naming Conventions

- **Controllers**: `ExampleController` (singular)
- **Views**: `Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, etc.
- **Models**: `ExampleModel`, `CreateExampleModel`, `EditExampleModel`
- **CSS Classes**: kebab-case (e.g., `page-header`, `search-toolbar`)
- **JavaScript Functions**: camelCase (e.g., `showModal`, `handleSubmit`)

### 8.2 Security

- ✅ Always use `[ValidateAntiForgeryToken]` on POST actions
- ✅ Apply `[Authorize(Roles = "Admin")]` at controller level
- ✅ Validate all inputs
- ✅ Use parameterized queries (EF Core does this automatically)
- ✅ Sanitize user input before displaying
- ✅ Check for null values before operations

### 8.3 Performance

- ✅ Use async/await for database operations
- ✅ Include related entities only when needed
- ✅ Use pagination for large datasets
- ✅ Index frequently searched columns
- ✅ Minimize database queries in loops

### 8.4 User Experience

- ✅ Always provide feedback (success/error messages)
- ✅ Use loading states during operations
- ✅ Confirm dangerous actions with modals
- ✅ Show helpful error messages
- ✅ Provide clear instructions
- ✅ Use tooltips for icons
- ✅ Make forms keyboard-accessible

### 8.5 Accessibility

- ✅ Use semantic HTML
- ✅ Add ARIA labels where needed
- ✅ Ensure keyboard navigation works
- ✅ Provide alt text for images
- ✅ Use proper heading hierarchy
- ✅ Test with screen readers

### 8.6 Code Quality

- ✅ Keep controllers thin (business logic in services)
- ✅ DRY principle (Don't Repeat Yourself)
- ✅ Single Responsibility Principle
- ✅ Meaningful variable names
- ✅ Comment complex logic
- ✅ Handle exceptions properly

---

## 9. Examples

### Example: Complete CRUD for "Categories"

#### 9.1 Model

```csharp
public class Category
{
    public int Id { get; set; }
    
    [Display(Name = "نام دسته")]
    [Required(ErrorMessage = "لطفاً {0} را وارد کنید")]
    [StringLength(100, ErrorMessage = "{0} نمی‌تواند بیشتر از {1} کاراکتر باشد")]
    public string Name { get; set; }
    
    [Display(Name = "توضیحات")]
    [StringLength(500)]
    public string Description { get; set; }
    
    [Display(Name = "فعال")]
    public bool IsActive { get; set; } = true;
    
    [Display(Name = "تاریخ ایجاد")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
```

#### 9.2 Controller

```csharp
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IActionResult> Index(string q)
    {
        var categories = _context.Categories.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(q))
        {
            categories = categories.Where(c => c.Name.Contains(q) || c.Description.Contains(q));
            ViewBag.Search = q;
        }
        
        return View(await categories.OrderBy(c => c.Name).ToListAsync());
    }
    
    public IActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "دسته با موفقیت ایجاد شد.";
            return RedirectToAction(nameof(Index));
        }
        
        return View(category);
    }
    
    // Add Edit, Delete, Details actions...
}
```

#### 9.3 Index View

```razor
@model IEnumerable<Category>
@{
    ViewData["Title"] = "مدیریت دسته‌ها";
    Layout = "~/Areas/Admin/Views/Shared/_Layout.cshtml";
}

@section Styles {
    <style>
        .page-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 2rem;
            border-radius: 1rem;
            margin-bottom: 2rem;
            box-shadow: 0 8px 16px rgba(102, 126, 234, 0.3);
        }
        .page-header h1 { color: white; border: none; margin: 0; }
        .page-header h1::after { display: none; }
        .search-toolbar {
            background: white;
            padding: 1.5rem;
            border-radius: 0.75rem;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
            margin-bottom: 1.5rem;
        }
    </style>
}

<div class="page-header">
    <h1 class="mb-0">
        <i class="fa-solid fa-layer-group"></i> @ViewData["Title"]
    </h1>
    <p class="mb-0 mt-2 opacity-75">مدیریت و سازماندهی دسته‌بندی‌ها</p>
</div>

@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success alert-dismissible fade show">
        <i class="fa-solid fa-circle-check"></i> @TempData["SuccessMessage"]
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>
}

<div class="search-toolbar">
    <div class="row align-items-center g-3">
        <div class="col-md-4">
            <a asp-action="Create" class="btn btn-success">
                <i class="fa-solid fa-plus"></i> ایجاد دسته جدید
            </a>
        </div>
        <div class="col-md-5">
            <form method="get" asp-action="Index" class="d-flex">
                <input type="text" name="q" value="@ViewBag.Search" 
                       class="form-control me-2" placeholder="جستجو..." />
                <button type="submit" class="btn btn-primary me-2">
                    <i class="fa-solid fa-magnifying-glass"></i>
                </button>
                @if (!string.IsNullOrWhiteSpace(ViewBag.Search))
                {
                    <a asp-action="Index" class="btn btn-outline-secondary">
                        <i class="fa-solid fa-xmark"></i>
                    </a>
                }
            </form>
        </div>
        <div class="col-md-3 text-end">
            <span class="badge bg-info fs-6">
                <i class="fa-solid fa-list-check"></i> تعداد: @Model.Count()
            </span>
        </div>
    </div>
</div>

<div class="card">
    <div class="card-body p-0">
        <div class="table-responsive">
            <table class="table table-striped table-hover mb-0">
                <thead class="table-dark">
                    <tr>
                        <th>شناسه</th>
                        <th>نام دسته</th>
                        <th>توضیحات</th>
                        <th>وضعیت</th>
                        <th>تاریخ ایجاد</th>
                        <th class="text-center">دستورات</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in Model)
                    {
                        <tr>
                            <td><code>@item.Id</code></td>
                            <td><strong>@item.Name</strong></td>
                            <td>
                                @if (!string.IsNullOrEmpty(item.Description))
                                {
                                    <span>@item.Description.Substring(0, Math.Min(50, item.Description.Length))...</span>
                                }
                                else
                                {
                                    <span class="text-muted">-</span>
                                }
                            </td>
                            <td>
                                @if (item.IsActive)
                                {
                                    <span class="badge bg-success">
                                        <i class="fa-solid fa-circle-check"></i> فعال
                                    </span>
                                }
                                else
                                {
                                    <span class="badge bg-secondary">
                                        <i class="fa-solid fa-circle-xmark"></i> غیرفعال
                                    </span>
                                }
                            </td>
                            <td class="ltr">@item.CreatedAt.ToString("yyyy/MM/dd")</td>
                            <td>
                                <div class="btn-group">
                                    <a asp-action="Edit" asp-route-id="@item.Id" 
                                       class="btn btn-primary btn-sm">
                                        <i class="fa-solid fa-pen-to-square"></i>
                                    </a>
                                    <a asp-action="Delete" asp-route-id="@item.Id" 
                                       class="btn btn-danger btn-sm">
                                        <i class="fa-solid fa-trash"></i>
                                    </a>
                                </div>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>
</div>

@if (!Model.Any())
{
    <div class="alert alert-info text-center">
        <i class="fa-solid fa-info-circle fs-3 d-block mb-2"></i>
        <strong>هیچ دسته‌ای ثبت نشده است.</strong>
        <p class="mb-0 mt-2">برای شروع، یک دسته جدید ایجاد کنید.</p>
    </div>
}

@section Scripts {
    <script>
        // Auto-dismiss alerts
        setTimeout(function() {
            const alerts = document.querySelectorAll('.alert-dismissible');
            alerts.forEach(function(alert) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            });
        }, 5000);
    </script>
}
```

---

## 10. Checklist for New Controller/Views

When creating a new admin controller and views, use this checklist:

### Controller
- [ ] Add `[Area("Admin")]` attribute
- [ ] Add `[Authorize(Roles = "Admin")]` attribute
- [ ] Inject required dependencies (DbContext, etc.)
- [ ] Implement Index action with search
- [ ] Implement Create (GET and POST)
- [ ] Implement Edit (GET and POST)
- [ ] Implement Delete (GET and POST)
- [ ] Add `[ValidateAntiForgeryToken]` to all POST actions
- [ ] Use TempData for success/error messages
- [ ] Handle exceptions properly

### Views
- [ ] Set correct Layout
- [ ] Add gradient page header with icon
- [ ] Include alert message display
- [ ] Add search toolbar (for Index)
- [ ] Use proper table styling (for Index)
- [ ] Include action buttons with icons
- [ ] Add empty state message
- [ ] Include validation scripts (for forms)
- [ ] Add loading states to buttons
- [ ] Test responsiveness

### Testing
- [ ] Test all CRUD operations
- [ ] Verify validation works
- [ ] Check authorization (non-admin can't access)
- [ ] Test search functionality
- [ ] Verify messages display correctly
- [ ] Test on mobile devices
- [ ] Check RTL layout
- [ ] Verify icons display

---

## 11. Resources

### CSS Files
- `wwwroot/css/admin.css` - Main admin area styles
- `wwwroot/css/admin-users.css` - User management specific styles

### JavaScript Libraries
- Bootstrap 5.3 (included in layout)
- FontAwesome 6 (included in layout)
- jQuery (for validation, included in layout)

### Documentation
- [Bootstrap 5 Docs](https://getbootstrap.com/docs/5.3/)
- [FontAwesome Icons](https://fontawesome.com/icons)
- [ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/)

---

## 12. Support & Questions

For questions or issues with admin area development:

1. Check this guide first
2. Review existing admin controllers (Users, Roles, Sessions)
3. Check `Version-History.md` for recent changes
4. Refer to Bootstrap 5 and FontAwesome documentation

---

**Last Updated**: January 2026  
**Version**: 1.0  
**Maintained By**: Development Team

---

**Remember**: Consistency is key! Follow these patterns for all new admin controllers to maintain a professional, cohesive admin interface.
