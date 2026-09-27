namespace DomainModel.ViewModels.Warranty
{
    public class RegisterCardsRequest
    {
        public string CustomerPhoneNumber { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? CustomerTypeId { get; set; }
        public List<string> Codes { get; set; } = new();
        public string RateLimitKey { get; set; } = string.Empty;
    }
}
