namespace Framework.Common
{
    public static class WarrantyValidity
    {
        public static int? RemainingDays(DateTime? registeredAt, int validityMonths, DateTime today)
        {
            if (!registeredAt.HasValue)
                return null;

            var expiry = registeredAt.Value.Date.AddMonths(validityMonths);
            return (expiry - today.Date).Days;
        }

        public static string Format(int? remainingDays)
        {
            if (!remainingDays.HasValue)
                return "شروع‌نشده";
            if (remainingDays.Value < 0)
                return "منقضی";

            var months = remainingDays.Value / 30;
            var days = remainingDays.Value % 30;
            if (months > 0 && days > 0)
                return $"{months} ماه و {days} روز";
            if (months > 0)
                return $"{months} ماه";
            return $"{remainingDays.Value} روز";
        }
    }
}
