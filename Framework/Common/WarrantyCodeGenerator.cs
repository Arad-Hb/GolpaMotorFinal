using System.Security.Cryptography;
using System.Text;

namespace Framework.Common
{
    public static class WarrantyCodeGenerator
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static string Serial()
        {
            return "GM" + DateTime.UtcNow.ToString("yyMMdd") + RandomToken(6);
        }

        public static string ScratchedCode()
        {
            return RandomToken(8);
        }

        private static string RandomToken(int length)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var sb = new StringBuilder(length);
            foreach (var b in bytes)
                sb.Append(Alphabet[b % Alphabet.Length]);
            return sb.ToString();
        }
    }
}
