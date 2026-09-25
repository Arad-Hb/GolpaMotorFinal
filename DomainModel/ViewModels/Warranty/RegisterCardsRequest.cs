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

    public class RegisterCardsResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? RetryAfterSeconds { get; set; }
        public List<string> FailedLines { get; set; } = new();
        public int SavedCount { get; set; }
    }
}
