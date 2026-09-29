using Framework.Common.Extensions;

namespace DomainModel.Validation;

public static class BankingValidationExtensions
{
    public static bool IsValidIranianCardNumber(this string? value)
        => IranianBankingRules.IsValidIranianCardNumber(value);

    public static bool IsValidIranianSheba(this string? value)
        => IranianBankingRules.IsValidIranianSheba(value);

    public static bool HasAccountNumberFormat(this string? value)
        => IranianBankingRules.IsValidAccountNumber(value);

    public static bool HasAccountNumberFormat(this string? value, int maximumLength)
    {
        if (maximumLength < IranianBankingRules.AccountNumberMinLength)
            return false;

        if (!IranianBankingRules.IsValidAccountNumber(value))
            return false;

        int length = value!.ToEnglishDigits()
            .Replace(" ", "")
            .Replace("-", "")
            .Trim()
            .Length;

        return length <= maximumLength;
    }
}
