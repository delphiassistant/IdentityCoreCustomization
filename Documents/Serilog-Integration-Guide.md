# Serilog Integration Guide for ASP.NET Core

This document describes the exact Serilog setup used in this project and provides
complete, copy-paste-ready instructions for applying the same pattern to any other
ASP.NET Core project. It is intentionally written so that an LLM can follow it
without additional context.

---

## Overview

| Concern | Solution |
|---|---|
| Coloured console output | `Serilog.Sinks.Console` with `AnsiConsoleTheme.Code` |
| Persistent file logs | `Serilog.Sinks.File` — daily rolling, 7-day retention |
| Startup crash capture | Bootstrap logger before host build |
| Framework noise | Source-level `Override` rules in `appsettings.json` |
| SQL / DB query suppression | `Microsoft.EntityFrameworkCore` → `Warning` in all environments |
| Request logging | `app.UseSerilogRequestLogging()` replaces the default middleware logs |
| Config-driven | All sink and level settings live in `appsettings.json` |

---

## Step 1 — Install Packages

```bash
dotnet add package Serilog.AspNetCore --version 10.0.0
dotnet add package Serilog.Sinks.Console --version 6.1.1
dotnet add package Serilog.Sinks.File    --version 7.0.0
```

> **Version note for LLMs**: the versions above target .NET 10. For other TFMs check
> `Serilog.AspNetCore` on NuGet and use the latest stable version; `Serilog.Sinks.Console`
> and `Serilog.Sinks.File` are independent of the host TFM — always use their own latest stable.

---

## Step 2 — `Program.cs`

### 2a. Replace the `Microsoft.Extensions.Logging` using with Serilog

```csharp
// Remove this:
using Microsoft.Extensions.Logging;

// Add this:
using Serilog;
```

> `ILogger<T>` throughout the app continues to work unchanged — Serilog registers itself
> as the backend of the `Microsoft.Extensions.Logging` abstraction via `UseSerilog()`.

### 2b. Add the bootstrap logger before `WebApplication.CreateBuilder`

Place this at the very top of `Program.cs`, before any other code:

```csharp
// Bootstrap logger — captures failures before host/DI is available.
// Replaced by the full logger once UseSerilog() is called.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
```

### 2c. Configure `UseSerilog` on the host

Add immediately after the `builder.Configuration` block (so configuration sources are
already set up before Serilog reads them):

```csharp
builder.Host.UseSerilog((context, services, config) =>
    config
        .ReadFrom.Configuration(context.Configuration) // reads appsettings.json Serilog section
        .ReadFrom.Services(services)                   // DI-aware enrichers/destructurers
        .Enrich.FromLogContext());                      // attaches scoped properties
```

### 2d. Add request logging middleware

Add after `app.UseStaticFiles()` (placing it here excludes static-file requests from
the request log, which reduces noise):

```csharp
app.UseStaticFiles();
app.UseSerilogRequestLogging(); // ← here
app.UseRouting();
```

### 2e. Wrap `app.Run()` with clean shutdown

Replace the bare `app.Run();` at the end of the file:

```csharp
try
{
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
```

> `HostAbortedException` is filtered out because .NET 8+ throws it on graceful shutdown;
> treating it as a fatal error would produce false-positive log entries.

### 2f. Replace any direct `ILoggerFactory` usage in `Program.cs`

If the file uses `ILoggerFactory` to log startup errors (e.g., a database seeder
catch block), replace it with the Serilog static API:

```csharp
// Before
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("Program");
logger.LogError(ex, "Seeding failed");

// After
Log.Error(ex, "Seeding failed");
```

---

## Step 3 — `appsettings.json`

Remove (or leave — it is ignored) the existing `Logging` section and add the `Serilog`
section:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "WriteTo": [
    {
      "Name": "Console",
      "Args": {
        "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
        "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}"
      }
    },
    {
      "Name": "File",
      "Args": {
        "path": "logs/log-.txt",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 7,
        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
      }
    }
  ],
  "Enrich": [ "FromLogContext" ]
},
```

---

## Step 4 — `appsettings.Development.json`

Add only the `MinimumLevel` override block — the sink configuration is inherited from
the base file and does **not** need to be repeated:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Debug",
    "Override": {
      "Microsoft": "Information",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
},
```

> `Microsoft.EntityFrameworkCore` stays at `Warning` even in development to prevent
> SQL query logs from flooding the output.

---

## Step 5 — `.gitignore`

Append to `.gitignore` so rolling log files are never committed:

```
# Serilog rolling log files
logs/
```

---

## Configuration Reference

### Console Themes

| Theme string | Appearance |
|---|---|
| `AnsiConsoleTheme::Code` | Code-editor palette — recommended for modern terminals |
| `AnsiConsoleTheme::Literate` | Classic Serilog default colours |
| `AnsiConsoleTheme::Grayscale` | No colour — useful for CI pipelines that strip ANSI |
| `SystemConsoleTheme::Literate` | Uses `System.Console` colours — fallback for old terminals |

Full type path for JSON config:
`Serilog.Sinks.SystemConsole.Themes.<ThemeName>::<Field>, Serilog.Sinks.Console`

### Output Template Tokens

| Token | Description |
|---|---|
| `{Timestamp:HH:mm:ss}` | Time only — compact, good for console |
| `{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}` | Full date + ms + timezone — good for files |
| `{Level:u3}` | Three-letter uppercase level: `INF`, `WRN`, `ERR` |
| `{Level:u4}` | Four-letter: `INFO`, `WARN`, `EROR` |
| `{SourceContext}` | Logger name (usually `Namespace.ClassName`) |
| `{Message:lj}` | Message with literal strings unquoted — recommended |
| `{Exception}` | Full exception including stack trace |
| `{NewLine}` | Platform line break |
| `{Properties:j}` | All extra structured properties as JSON |

### File Sink — `rollingInterval` Values

`Infinite` `Year` `Month` `Day` `Hour` `Minute`

### Source Override — Common Values

```json
"Override": {
  "Microsoft":                                   "Warning",
  "Microsoft.Hosting.Lifetime":                  "Information",
  "Microsoft.EntityFrameworkCore":               "Warning",
  "Microsoft.AspNetCore.Routing":                "Warning",
  "Microsoft.AspNetCore.Mvc":                    "Warning",
  "Microsoft.AspNetCore.StaticFiles":            "Warning",
  "System.Net.Http.HttpClient":                  "Warning"
}
```

---

## Porting Checklist

Use this checklist when applying the pattern to a new project:

- [ ] Install `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`
- [ ] Remove (or keep as dead config) the `Logging` section in `appsettings.json`
- [ ] Add `Serilog` section to `appsettings.json` (Step 3 above)
- [ ] Add `Serilog` MinimumLevel override to `appsettings.Development.json` (Step 4)
- [ ] Replace `using Microsoft.Extensions.Logging` with `using Serilog` in `Program.cs`
- [ ] Add bootstrap logger before `WebApplication.CreateBuilder` (Step 2b)
- [ ] Add `builder.Host.UseSerilog(...)` after configuration sources (Step 2c)
- [ ] Add `app.UseSerilogRequestLogging()` after `UseStaticFiles()` (Step 2d)
- [ ] Wrap `app.Run()` in try/catch/finally with `Log.CloseAndFlushAsync()` (Step 2e)
- [ ] Replace any `ILoggerFactory` usage in `Program.cs` with `Log.*` static calls (Step 2f)
- [ ] Add `logs/` to `.gitignore` (Step 5)
- [ ] Build and run — verify coloured output in terminal and `logs/log-<date>.txt` is created

---

## LLM Prompt Template

Use the following prompt to ask an LLM to apply this pattern to another project:

```
Apply the Serilog integration pattern from Documents/Serilog-Integration-Guide.md
to this project. Follow every step in the porting checklist exactly.
Target framework: <net10.0 / net9.0 / ...>
The project's Program.cs uses top-level statements.
Do not change any existing service registrations or middleware order other than
inserting UseSerilog, UseSerilogRequestLogging, and the shutdown wrapper.
```

---

## Notes

- `ReadFrom.Services(services)` is optional but recommended — it allows enrichers that
  depend on DI (e.g., `IHttpContextAccessor`-based enrichers) to be registered normally.
- Serilog's `ILogger<T>` integration means **no changes are needed in any service or
  controller** — they continue to inject `ILogger<T>` as usual; Serilog is the backend.
- The `logs/` folder is created automatically by the File sink on first write.
- Log file path in `appSettings.json` is relative to the application's working directory
  (typically the project root in development, the publish folder in production).
