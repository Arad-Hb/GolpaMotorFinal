namespace GolpaMotorFinal.Models.ViewModels.Account
{
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
    }
}
