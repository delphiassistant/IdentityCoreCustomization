using System.Globalization;
using System.Reflection;

namespace IdentityCoreCustomization.Classes.Extensions
{
    public static class PersianDateExtensionMethods
    {
        // Thread-safe singleton using Lazy<T> with ExecutionAndPublication mode.
        // CultureInfo.ReadOnly() prevents any caller from mutating the shared instance.
        private static readonly Lazy<CultureInfo> _lazyCulture =
            new(() => CultureInfo.ReadOnly(BuildPersianCulture()), LazyThreadSafetyMode.ExecutionAndPublication);

        public static CultureInfo GetPersianCulture() => _lazyCulture.Value;

        private static CultureInfo BuildPersianCulture()
        {
            CultureInfo culture = new("fa-IR");
            DateTimeFormatInfo formatInfo = culture.DateTimeFormat;
            formatInfo.AbbreviatedDayNames = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
            formatInfo.DayNames = new[] { "یکشنبه", "دوشنبه", "سه شنبه", "چهار شنبه", "پنجشنبه", "جمعه", "شنبه" };
            string[] monthNames = new[]
            {
                "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن",
                "اسفند",
                ""
            };
            formatInfo.AbbreviatedMonthNames =
                formatInfo.MonthNames =
                formatInfo.MonthGenitiveNames = formatInfo.AbbreviatedMonthGenitiveNames = monthNames;
            formatInfo.AMDesignator = "صبح";
            formatInfo.PMDesignator = "عصر";
            formatInfo.ShortDatePattern = "yyyy/MM/dd";
            formatInfo.LongDatePattern = "dddd dd MMMM yyyy";
            formatInfo.FirstDayOfWeek = DayOfWeek.Saturday;
            formatInfo.FullDateTimePattern = "dddd d MMMM yyyy ساعت HH:mm:ss";
            formatInfo.YearMonthPattern = "MMMM yyyy";
            Calendar cal = new PersianCalendar();

            FieldInfo fieldInfo = culture.GetType().GetField("calendar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(culture, cal);
            }

            FieldInfo info = formatInfo.GetType().GetField("calendar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
            {
                info.SetValue(formatInfo, cal);
            }

            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.DigitSubstitution = DigitShapes.NativeNational;
            culture.NumberFormat.NumberNegativePattern = 0;
            culture.NumberFormat.CurrencySymbol = "ریال";
            culture.NumberFormat.NaNSymbol = "تعریف نشده";
            culture.NumberFormat.PercentDecimalSeparator = "/";
            culture.NumberFormat.PercentSymbol = "%";

            return culture;
        }

        public static string ToPersianDateString(this DateTime date, string format = "yyyy/MM/dd")
        {
            return date.ToString(format, GetPersianCulture());
        }

        /// <summary>Gets the Iran Standard Time zone (UTC+3:30).</summary>
        public static TimeZoneInfo IranTimeZone { get; } = GetIranTimeZone();

        private static TimeZoneInfo GetIranTimeZone()
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById("Iran Standard Time", out var tz))
            {
                return tz;
            }

            if (TimeZoneInfo.TryFindSystemTimeZoneById("Asia/Tehran", out tz))
            {
                return tz;
            }

            return TimeZoneInfo.CreateCustomTimeZone("Iran", TimeSpan.FromHours(3.5), "ایران", "ایران");
        }

        /// <summary>
        /// Converts a UTC DateTime to Iran Standard Time and returns a formatted Persian full date-time string,
        /// e.g. "شنبه ۱ فروردین ۱۴۰۴ ساعت ۱۲:۳۰".
        /// </summary>
        public static string ToPersianFullDateTimeString(this DateTime utcDateTime)
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, IranTimeZone);
            return local.ToString("dddd d MMMM yyyy ساعت HH:mm", GetPersianCulture());
        }

        /// <summary>
        /// Returns the first day of the Persian month that contains <paramref name="date"/>,
        /// expressed as a Gregorian <see cref="DateTime"/> at midnight.
        /// </summary>
        public static DateTime PersianMonthStart(this DateTime date)
        {
            var cal = new PersianCalendar();
            int year  = cal.GetYear(date);
            int month = cal.GetMonth(date);
            return cal.ToDateTime(year, month, 1, 0, 0, 0, 0);
        }

        /// <summary>
        /// Returns the last day of the Persian month that contains <paramref name="date"/>,
        /// expressed as a Gregorian <see cref="DateTime"/> at midnight.
        /// </summary>
        public static DateTime PersianMonthEnd(this DateTime date)
        {
            var cal = new PersianCalendar();
            int year      = cal.GetYear(date);
            int month     = cal.GetMonth(date);
            int daysInMonth = cal.GetDaysInMonth(year, month);
            return cal.ToDateTime(year, month, daysInMonth, 0, 0, 0, 0);
        }

        /// <summary>
        /// Returns the Persian date string for the first day of the current Persian month,
        /// formatted as <c>yyyy/MM/dd</c> (e.g. "۱۴۰۴/۰۱/۰۱").
        /// Useful for populating datepicker placeholders.
        /// </summary>
        public static string CurrentPersianMonthStartString(string format = "yyyy/MM/dd")
            => DateTime.Today.PersianMonthStart().ToPersianDateString(format);

        /// <summary>
        /// Returns the Persian date string for the last day of the current Persian month,
        /// formatted as <c>yyyy/MM/dd</c> (e.g. "۱۴۰۴/۰۱/۳۱").
        /// Useful for populating datepicker placeholders.
        /// </summary>
        public static string CurrentPersianMonthEndString(string format = "yyyy/MM/dd")
            => DateTime.Today.PersianMonthEnd().ToPersianDateString(format);
    }
}
