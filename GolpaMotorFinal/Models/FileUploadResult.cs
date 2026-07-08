namespace GolpaMotorFinal.Models
{
    public class FileUploadResult
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";

        public string? FileName { get; set; }

        public string? FileUrl { get; set; }

        public string? ThumbnailUrl { get; set; }
    }
}
