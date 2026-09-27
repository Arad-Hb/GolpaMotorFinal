namespace DomainModel.ViewModels.Warranty
{
    public class RegisterCardsResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? RetryAfterSeconds { get; set; }
        public List<string> FailedLines { get; set; } = new();
        public int SavedCount { get; set; }
    }
}
