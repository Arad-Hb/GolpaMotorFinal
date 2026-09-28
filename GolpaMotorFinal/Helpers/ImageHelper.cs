namespace GolpaMotorFinal.Helpers
{
    public static class ImageHelper
    {
        public static string Fix(string? fileName)
        {
            // اگر خالی بود
            if (string.IsNullOrWhiteSpace(fileName) ||fileName== "/images/pics/noimage.jpg")
                return "/images/pics/noimage.jpg";

            // فقط اسم فایل (امنیت + جلوگیری از path injection)
            var safeFileName = Path.GetFileName(fileName);

            // مسیر نهایی ثابت
            return "/images/imageProducts/uploads/" + safeFileName;
        }

        public static string ToThumbnail(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || imageUrl == "/images/pics/noimage.jpg")
                return "/images/pics/noimage.jpg";

            return imageUrl.Replace("/uploads/", "/thumbnails/");
        }
    }
}
