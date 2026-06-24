namespace GolpaMotorFinal.Helpers
{
    public static class ImageHelper
    {
        public static string Fix(string? fileName)
        {
            // اگر خالی بود
            if (string.IsNullOrWhiteSpace(fileName))
                return "/ImageProducts/noimage.jpg";

            // فقط اسم فایل (امنیت + جلوگیری از path injection)
            var safeFileName = Path.GetFileName(fileName);

            // مسیر نهایی ثابت
            return "/ImageProducts/" + safeFileName;
        }
        public static string FixUser(string? fileName)
        {
            // اگر خالی بود
            if (string.IsNullOrWhiteSpace(fileName))
                return "/images/imageUsers/noimage.jpg";

            // فقط اسم فایل (امنیت + جلوگیری از path injection)
            var safeFileName = Path.GetFileName(fileName);

            // مسیر نهایی ثابت
            return "/images/imageUsers/" + safeFileName;
        }
    }
}
