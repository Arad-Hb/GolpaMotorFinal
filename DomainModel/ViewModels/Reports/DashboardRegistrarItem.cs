namespace DomainModel.ViewModels.Reports
{
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
}
