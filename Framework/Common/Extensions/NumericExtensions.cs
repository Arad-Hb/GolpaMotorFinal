using System.Globalization;

namespace Framework.Common.Extensions
{
    public static class NumericExtensions
    {
        public static string ToEnglishDigits(this string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var result = new System.Text.StringBuilder(value.Length);

            foreach (char character in value)
            {
                if (character is >= '۰' and <= '۹')
                {
                    result.Append((char)('0' + character - '۰'));
                }
                else if (character is >= '٠' and <= '٩')
                {
                    result.Append((char)('0' + character - '٠'));
                }
                else
                {
                    result.Append(character);
                }
            }

            return result.ToString();
        }

        public static string ToGroupedNumber(this int value)
            => value.ToString("N0", CultureInfo.InvariantCulture);

        public static string ToGroupedNumber(this long value)
            => value.ToString("N0", CultureInfo.InvariantCulture);

        public static string ToGroupedNumber(this decimal value)
            => value.ToString("N0", CultureInfo.InvariantCulture);

        public static string ToGroupedNumber(this int? value, string emptyText = "—")
            => value.HasValue ? value.Value.ToGroupedNumber() : emptyText;

        public static string ToGroupedNumber(this decimal? value, string emptyText = "—")
            => value.HasValue ? value.Value.ToGroupedNumber() : emptyText;

        public static string FormatGroupedNumber(object? value, string emptyText = "—")
        {
            if (value == null)
                return emptyText;

            return value switch
            {
                byte number => number.ToString("N0", CultureInfo.InvariantCulture),
                short number => number.ToString("N0", CultureInfo.InvariantCulture),
                int number => number.ToGroupedNumber(),
                long number => number.ToGroupedNumber(),
                float number => number.ToString("N0", CultureInfo.InvariantCulture),
                double number => number.ToString("N0", CultureInfo.InvariantCulture),
                decimal number => number.ToGroupedNumber(),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? emptyText
            };
        }
    }
}
