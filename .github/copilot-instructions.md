# Copilot Instructions

## Documentation Structure
- Maintain a version-history.md at solution root folder and update it with each major change.
- All documentation should be kept in Documents/ folder except README.md

## Admin Area Development
- **When creating new admin controllers/views**: Follow the comprehensive design guidelines in [Documents/admin-design-guidelines.md](../Documents/admin-design-guidelines.md)
- This guide ensures consistency with existing admin area design patterns, components, and styling
- Includes: Layout structure, view patterns, component library, JavaScript patterns, controller patterns, and complete examples
- All admin views MUST use the shared layout and follow the established gradient header, card-based design system

## Critical Instructions

- Always use **MVC pattern**, not Razor Pages for UI.
- Do **not** create enums where entities could be created/used for easier data handling.
- Always use **Bootstrap modal** instead of `alert()` / `confirm()` for user interactions in the UI.
- **NEVER write, construct, or edit EF Core migration files by hand.** Migrations must always be created exclusively via EF tools (`dotnet ef migrations add <Name>` or `Add-Migration <Name>` in the Package Manager Console). After implementing model/`DbContext` changes, instruct the developer to run the EF tool command — do not produce migration `.cs` files yourself, even partially.

---

## Screenshot & Image Context

- Whenever a screenshot or image is provided, always look for **arrows, highlights, circles, or any marked areas** — those are the focus areas the user wants you to pay attention to. Treat them as explicit pointers to the part of the UI, code, or behavior being discussed or reported.

---

## General Guidelines

- You are a **Senior Software Developer**. Inspect every problem very deeply before generating a solution.
- All generated documents should be in **English**.
- When instructing the user to create a migration, always provide the **exact command** to run in the terminal (PMC), and specify the migration name.
- **Do not** create summary documents on each session — only create documents when explicitly requested.
- Whenever your response is long, **put it into a `.md` file** for review.
- Always put **architectural decisions in `.md` files** — do not put them in chat windows.
- When the prompt includes the word **"Question"**, just answer the question — do not write any code.
- When the result is a **technical evaluation**, provide pros and cons if applicable, and put long answers into `.md` files.
- **Do not remove existing code** or break existing functionality.
- All documentation should be kept in a `Documents/` folder, except `README.md`.
- Prefer **direct file-edit tools** instead of using PowerShell/shell for file content modifications when implementing changes.

---

## Code Style

- **Primary key naming:** All primary key property names must follow the pattern `[EntityName]ID` — e.g. `UserID`, `ProductID`, `OrderID`.
- **Route IDs over query strings:** Whenever a resource is identified by a single ID, always use a **route segment** (`/{id}`) instead of a query-string parameter (`?id=5`). Only use query-string parameters for optional/filter values (e.g. search text, pagination, booleans).
- **NEVER generate migration files.** After model changes are complete, tell the developer to run the EF tool command. Do not write, edit, or scaffold migration `.cs` files — this is the EF tooling's sole responsibility.
- **Do not modify the Model Snapshot directly**; it will be updated automatically when migrations are created.
- Only write **Controllers/Views** in MVC projects, not Razor Pages.
- Do **not** use migrations for seeding data; use a dedicated `DatabaseSeeder` class instead.
- Every new entity **must** have an explicit configuration block inside `OnModelCreating` in the `DbContext` (at minimum: `ToTable`, `HasKey`). Never rely on EF conventions alone. Add indexes and relationships there too.
- Always **add comments** in code to explain complex logic.
- Add comments when adding **public methods**.

---

## UI — Theme-Aware CSS

- Every custom CSS rule that sets `background-color`, `color`, or `border-color` **must** use theme-aware CSS variables — never hardcoded hex/rgba values. Use Bootstrap 5.3's built-in CSS custom properties (`--bs-body-bg`, `--bs-body-color`, `--bs-secondary-bg`, `--bs-tertiary-bg`, `--bs-border-color`) which automatically switch between light and dark values when `data-bs-theme` changes on the `<html>` element, so no separate `[data-bs-theme="dark"]` override is needed for colour properties.

- **NEVER use `background: white`, `background: #fff`, `background-color: #ffffff`, or any equivalent hardcoded light color on panel, card, toolbar, form-card, search-toolbar, or any container element.** These values break dark themes and cause bright white panels to appear on a dark background.

  **Wrong — never do this (breaks dark theme):**
  ```css
  .search-toolbar { background: white; }
  .form-card { background-color: #ffffff; }
  .action-toolbar { background: #fff; }
  ```
  **Correct — always use Bootstrap CSS variables:**
  ```css
  .search-toolbar { background: var(--bs-body-bg); }
  .form-card { background-color: var(--bs-body-bg); }
  .action-toolbar { background: var(--bs-secondary-bg); }
  ```
  **Exception:** `background: white` is acceptable only on elements that are intentionally always displayed against a dark background regardless of theme (e.g. circular avatar/logo containers inside a fixed-dark sidebar, or a white CTA button on a gradient hero banner). **Document these exceptions with a comment in the CSS.**

- For Bootstrap utility classes that affect background/color, prefer theme-aware variants: use `bg-body-secondary` instead of `bg-light`, `text-body` instead of `text-dark`, `bg-body-tertiary` instead of `bg-white` on surfaces.

- Use **Bootstrap Switch** instead of regular checkboxes for boolean fields in forms.
- Any styles-related code should be placed in `wwwroot/css/` in a respective CSS file, not inline in Views or Layout files. Only put styles specific to a single view in that view's own CSS file if necessary; otherwise keep all styles in `site.css` for better maintainability.
- Same rule applies to JavaScript: store in `wwwroot/js/` in a respective JS file, not inline in Views or Layout files.

---

## UI — RTL Icon Spacing (Bootstrap 5 RTL)

When using `bootstrap.rtl.min.css`, spacing between an icon and its adjacent text **must be placed on the text element**, never on the icon.

**Correct:**
```html
<i class="fa-solid fa-envelope text-primary"></i>
<span class="ms-2">label text</span>
```

**Wrong — do not do this:**
```html
<i class="fa-solid fa-envelope text-primary ms-2"></i>
<span>label text</span>
```

- In RTL, `ms-*` (margin-start) is the physical **right** margin. On an icon it pushes away from the container edge, not toward the text.
- Never use `me-*` on an icon preceding text; use `ms-*` on the text element instead.
- This rule applies to all icon-text pairs: card headers, list items, nav links, info hints, table rows, etc.

---

## UI — RTL Flex Container Spacing (Bootstrap 5 RTL)

In `d-flex` layouts where a text/content block follows an icon block, the spacing on the text block **must use `ms-*`**, not `me-*`.

**Correct:**
```html
<div class="d-flex align-items-center">
    <div class="icon-block">
        <i class="fa-solid fa-users fa-2x"></i>
    </div>
    <div class="flex-grow-1 ms-3">
        label text
    </div>
</div>
```

**Wrong — do not do this:**
```html
<div class="flex-grow-1 me-3">...</div>
```

- In RTL, the icon block sits on the **right** (start) and the text block sits to its **left** (end).
- `me-3` pushes the text block the wrong direction. `ms-3` creates the gap correctly.
- Applies to any `d-flex` row where a fixed-size block (icon, avatar, badge) precedes a `flex-grow-1` text/content block.

---

## Table Views — Search / Filter / Sort / Paging

- **Every view that renders a table must support:** free-text search (`q`), column sorting (`sortBy` + `sortDir`), and pagination (`page`, `pageSize`) via query-string parameters. Default `pageSize` is 20.
- **Controller pattern:** filter → sort → `CountAsync` → `Skip/Take` → return a paged result model. Never return an unbounded list or use `Take(N)` as a substitute for real paging.
- Pass all active filter/sort/page params into pagination and sort-link URLs via `asp-all-route-data`.
- **Parent-state preservation (`returnUrl` pattern):** Whenever a table row has an action link that navigates to a child page (Details, Edit, History, etc.), the parent view **must** pass the current full URL as `returnUrl`:
  ```html
  @{ var returnUrl = Context.Request.GetEncodedPathAndQuery(); }
  <a asp-action="Details" asp-route-id="@item.SomeID" asp-route-returnUrl="@returnUrl">جزئیات</a>
  ```
  The child controller accepts `string? returnUrl = null` and sets `ViewBag.ReturnUrl = returnUrl`. The child view's back button uses:
  ```html
  <a href="@(ViewBag.ReturnUrl ?? Url.Action("Index"))" class="btn btn-secondary">Back</a>
  ```
  Always validate `returnUrl` with `Url.IsLocalUrl(returnUrl)` before redirecting server-side.
- **Page reset:** When the user submits a search or filter form, always reset `page` to 1.
- **Sort direction toggle:** Column header links toggle direction between `asc` and `desc`; show `▲` / `▼` icon next to the active sort column.
- **FK dropdown filters:** Whenever a table has a foreign-key column, the filter bar **must** include a `<select>` dropdown for that FK. Load the list in the controller via `ViewBag`, add a blank "All ..." first option, and preserve the selected FK value through all sort/page links. Use `if/else` blocks in Razor to set the `selected` attribute — never inline `@(condition ? "selected" : "")` on `<option>` elements (causes `RZ1031`).
