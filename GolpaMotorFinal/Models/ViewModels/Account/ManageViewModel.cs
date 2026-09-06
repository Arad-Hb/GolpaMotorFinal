using DomainModel.Models;

namespace GolpaMotorFinal.Models.ViewModels.Account
{
    public class CustomerCardItem
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Points { get; set; }
    }

    public class ManageViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName {  get; set; }
        public string LastName { get; set; }
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber {  get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? City { get; set; }
        public string? Province { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public int RemainedPoints { get; set; }
        public int TotalRegisteredCards { get; set; }
        public List<CustomerCardItem> Cards { get; set; } = new();
    }
}
