using Framework.Common.Extensions;

namespace DomainModel.Validation;

public static class IranianBankingRules
{
    public const int AccountNumberMinLength = 8;
    public const int AccountNumberMaxLength = 50;

    public static bool IsValid(BankingFieldKind kind, string? value)
        => kind switch
        {
            BankingFieldKind.Card => IsValidIranianCardNumber(value),
            BankingFieldKind.Sheba => IsValidIranianSheba(value),
            BankingFieldKind.Account => IsValidAccountNumber(value),
            _ => false
        };

    public static bool IsValidIranianCardNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string number = Normalize(value);

        if (number.Length != 16 || !number.All(char.IsAsciiDigit))
            return false;

        int sum = 0;
        bool doubleDigit = false;

        for (int index = number.Length - 1; index >= 0; index--)
        {
            int digit = number[index] - '0';

            if (doubleDigit)
            {
                digit *= 2;

                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            doubleDigit = !doubleDigit;
        }

        return sum % 10 == 0;
    }

    public static bool IsValidIranianSheba(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string sheba = Normalize(value).ToUpperInvariant();

        if (sheba.Length != 26 ||
            !sheba.StartsWith("IR", StringComparison.Ordinal))
        {
            return false;
        }

        for (int index = 2; index < sheba.Length; index++)
        {
            if (!char.IsAsciiDigit(sheba[index]))
                return false;
        }

        string rearranged = sheba[4..] + sheba[..4];
        int remainder = 0;

        foreach (char character in rearranged)
        {
            if (char.IsAsciiDigit(character))
            {
                remainder = (remainder * 10 + character - '0') % 97;
            }
            else
            {
                int letterValue = character - 'A' + 10;
                remainder = (remainder * 100 + letterValue) % 97;
            }
        }

        return remainder == 1;
    }

    public static bool IsValidAccountNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string accountNumber = Normalize(value);

        return accountNumber.Length >= AccountNumberMinLength &&
               accountNumber.Length <= AccountNumberMaxLength &&
               accountNumber.All(char.IsAsciiDigit);
    }

    private static string Normalize(string value)
        => value.ToEnglishDigits()
            .Replace(" ", "")
            .Replace("-", "")
            .Trim();
}
