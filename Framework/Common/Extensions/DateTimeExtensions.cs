using System.Globalization;

namespace Framework.Common.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly PersianCalendar Calendar = new();
        private static readonly TimeZoneInfo IranTimeZone = ResolveIranTimeZone();

        private static readonly string[] WeekDays =
        {
            "یکشنبه",
            "دوشنبه",
            "سه‌شنبه",
            "چهارشنبه",
            "پنجشنبه",
            "جمعه",
            "شنبه"
        };

        private static readonly string[] Months =
        {
            "همه ماه‌ها",
            "فروردین",
            "اردیبهشت",
            "خرداد",
            "تیر",
            "مرداد",
            "شهریور",
            "مهر",
            "آبان",
            "آذر",
            "دی",
            "بهمن",
            "اسفند"
        };

        public static IReadOnlyList<string> PersianMonths => Months;

        public static string PersianMonthName(int month)
            => month >= 0 && month < Months.Length ? Months[month] : string.Empty;

        public static string ToPersianDate(this DateTime date)
        {
            return $"{Calendar.GetYear(date):0000}/{Calendar.GetMonth(date):00}/{Calendar.GetDayOfMonth(date):00}";
        }

        public static string ToPersianDate(this DateTime? date)
            => date.HasValue ? date.Value.ToPersianDate() : string.Empty;

        public static string ToPersianDateTime(this DateTime date)
        {
            return $"{date.ToPersianDate()} {date:HH:mm}";
        }

        public static string ToPersianDateTime(this DateTime? date)
            => date.HasValue ? date.Value.ToPersianDateTime() : string.Empty;

        public static string ToPersianLongDate(this DateTime date)
        {
            var year = Calendar.GetYear(date);
            var month = Calendar.GetMonth(date);
            var day = Calendar.GetDayOfMonth(date);
            var dayName = WeekDays[(int)date.DayOfWeek];

            return $"{dayName} {day} {Months[month]} {year}";
        }

        public static DateTime? ToGregorianDate(this string? jalali)
        {
            if (string.IsNullOrWhiteSpace(jalali))
                return null;

            var parts = jalali.Trim().Replace('-', '/').Split('/');
            if (parts.Length != 3)
                return null;
            if (!int.TryParse(parts[0], out var year) ||
                !int.TryParse(parts[1], out var month) ||
                !int.TryParse(parts[2], out var day))
                return null;

            try
            {
                return Calendar.ToDateTime(year, month, day, 0, 0, 0, 0);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        public static int GetPersianYear(this DateTime date) => Calendar.GetYear(date);

        public static int GetPersianMonth(this DateTime date) => Calendar.GetMonth(date);

        public static DateTime? JalaliStartOfDayUtc(this string? jalali)
        {
            var local = jalali.ToGregorianDate();
            return local.HasValue
                ? TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(local.Value.Date, DateTimeKind.Unspecified),
                    IranTimeZone)
                : null;
        }

        public static DateTime? JalaliEndExclusiveUtc(this string? jalali)
        {
            var local = jalali.ToGregorianDate();
            return local.HasValue
                ? TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(local.Value.Date.AddDays(1), DateTimeKind.Unspecified),
                    IranTimeZone)
                : null;
        }

        public static DateTime ToIranTime(this DateTime utc)
            => TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(utc, DateTimeKind.Utc),
                IranTimeZone);

        private static TimeZoneInfo ResolveIranTimeZone()
        {
            foreach (var id in new[] { "Iran Standard Time", "Asia/Tehran" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
            }

            return TimeZoneInfo.Utc;
        }
    }
}
