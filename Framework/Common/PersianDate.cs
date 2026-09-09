using System.Globalization;

namespace Framework.Common
{
    public static class PersianDate
    {
        private static readonly PersianCalendar Calendar = new();

        public static bool TryParse(string? jalali, out DateTime gregorian)
        {
            gregorian = default;
            if (string.IsNullOrWhiteSpace(jalali))
                return false;

            var parts = jalali.Trim().Replace('-', '/').Split('/');
            if (parts.Length != 3)
                return false;
            if (!int.TryParse(parts[0], out var year) ||
                !int.TryParse(parts[1], out var month) ||
                !int.TryParse(parts[2], out var day))
                return false;

            try
            {
                gregorian = Calendar.ToDateTime(year, month, day, 0, 0, 0, 0);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public static DateTime? ParseOrNull(string? jalali)
        {
            return TryParse(jalali, out var value) ? value : null;
        }

        public static string ToJalali(DateTime date)
        {
            return $"{Calendar.GetYear(date):0000}/{Calendar.GetMonth(date):00}/{Calendar.GetDayOfMonth(date):00}";
        }

        public static int GetYear(DateTime date) => Calendar.GetYear(date);

        public static int GetMonth(DateTime date) => Calendar.GetMonth(date);
    }
}
