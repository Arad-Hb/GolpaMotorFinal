namespace DomainModel.ViewModels.Reports
{
    public class DashboardWarrantyAlertItem
    {
        public long WarrantyCardID { get; set; }
        public string UserID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string ScratchedCode { get; set; } = string.Empty;
        public DateTime RegisteredAtUtc { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int RemainingDays { get; set; }
        public string RemainingText { get; set; } = string.Empty;
    }
}
