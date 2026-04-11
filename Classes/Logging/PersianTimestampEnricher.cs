using IdentityCoreCustomization.Classes.Extensions;

using Serilog.Core;
using Serilog.Events;
using System.Globalization;

namespace IdentityCoreCustomization.Classes.Logging;

/// <summary>
/// Enriches every log event with two Persian (Shamsi) calendar properties:
/// <list type="bullet">
///   <item><c>PersianTimestamp</c> — full date-time for use in output templates.</item>
///   <item><c>PersianDate</c>      — date-only with ASCII digits (e.g. <c>14050105</c>), safe
///     for use in log file names via <c>WriteTo.Map</c>.</item>
/// </list>
/// Serilog's built-in {Timestamp} token always formats with
/// CultureInfo.InvariantCulture regardless of DefaultThreadCurrentCulture,
/// so the only reliable way to get Persian-calendar timestamps is to add them
/// as enriched properties.
/// </summary>
public sealed class PersianTimestampEnricher : ILogEventEnricher
{
    public const string TimestampPropertyName = "PersianTimestamp";

    /// <summary>
    /// Date-only property used as the key in <c>WriteTo.Map</c> to route log events to
    /// Persian-date-named files (e.g. <c>log-14050105.txt</c>).
    /// Always contains ASCII digits regardless of the OS locale setting.
    /// </summary>
    public const string DatePropertyName = "PersianDate";

    // PersianCalendar is thread-safe for reads; reuse the single instance.
    private static readonly PersianCalendar _calendar = new();

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var iranTime = TimeZoneInfo.ConvertTime(logEvent.Timestamp, PersianDateExtensionMethods.IranTimeZone);
        var dt = iranTime.DateTime;

        // Full timestamp for log content (yyyy/MM/dd HH:mm:ss.fff in Persian calendar).
        var timestamp = dt.ToString("yyyy/MM/dd HH:mm:ss.fff", PersianDateExtensionMethods.GetPersianCulture());
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(TimestampPropertyName, timestamp));

        // Date-only using PersianCalendar directly to guarantee ASCII numerals (0-9)
        // regardless of CultureInfo.DigitSubstitution or OS locale settings.
        // Result example: "14050105" (yyyyMMdd in Shamsi calendar).
        var year  = _calendar.GetYear(dt);
        var month = _calendar.GetMonth(dt);
        var day   = _calendar.GetDayOfMonth(dt);
        var date  = $"{year:D4}{month:D2}{day:D2}";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(DatePropertyName, date));
    }
}
