using System.Linq;

namespace Framework.Common
{
    public static class IranianMobileNumber
    {
        public static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("0098", StringComparison.Ordinal))
                digits = digits[4..];
            else if (digits.StartsWith("98", StringComparison.Ordinal) && digits.Length >= 12)
                digits = digits[2..];

            if (digits.Length == 10 && digits.StartsWith("9", StringComparison.Ordinal))
                digits = "0" + digits;

            if (digits.Length == 11 && digits.StartsWith("09", StringComparison.Ordinal))
                return digits;

            return null;
        }

        public static List<string> LookupCandidates(string? value)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            var trimmed = value?.Trim();
            if (!string.IsNullOrEmpty(trimmed))
                set.Add(trimmed);

            var normalized = Normalize(value);
            if (normalized != null)
            {
                var national = normalized[1..];
                set.Add(normalized);
                set.Add(national);
                set.Add("98" + national);
                set.Add("+98" + national);
                set.Add("0098" + national);
            }

            return set.ToList();
        }
    }
}
