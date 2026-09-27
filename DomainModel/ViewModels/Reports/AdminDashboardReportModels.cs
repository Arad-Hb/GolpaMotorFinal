namespace DomainModel.ViewModels.Reports
{
    public class AdminDashboardSummary
    {
        public int TotalEarnedPoints { get; set; }
        public int TotalSettledPoints { get; set; }
        public int TotalRemainedPoints { get; set; }
        public int TotalRewardRequests { get; set; }
        public int SettledRewardRequests { get; set; }
        public int PendingRewardRequests { get; set; }
    }

    public class DashboardRegistrarItem
    {
        public string UserID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? JobTitle { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; }
        public int RegistrationCount { get; set; }
    }

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
