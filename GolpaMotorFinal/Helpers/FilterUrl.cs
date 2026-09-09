using Microsoft.AspNetCore.Routing;

namespace GolpaMotorFinal.Helpers
{
    public static class FilterUrl
    {
        public static string Combine(string path, object? values)
        {
            if (values == null)
                return path;

            var dict = new RouteValueDictionary(values);
            var pairs = dict
                .Where(x => x.Value != null && !string.IsNullOrWhiteSpace(Convert.ToString(x.Value)))
                .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(Convert.ToString(x.Value)!)}");

            var qs = string.Join("&", pairs);
            return string.IsNullOrEmpty(qs) ? path : path + "?" + qs;
        }
    }
}
