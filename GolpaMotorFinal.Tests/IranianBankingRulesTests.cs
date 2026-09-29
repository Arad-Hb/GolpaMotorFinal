using DomainModel.Validation;
using Xunit;

namespace GolpaMotorFinal.Tests;

public class IranianBankingRulesTests
{
    private const string ValidCard = "4111111111111111";
    private const string ValidBban = "0000000000000000000001";

    [Fact]
    public void Empty_values_are_invalid_at_rule_level()
    {
        Assert.False(IranianBankingRules.IsValidIranianCardNumber(null));
        Assert.False(IranianBankingRules.IsValidIranianCardNumber("  "));
        Assert.False(IranianBankingRules.IsValidIranianSheba(""));
        Assert.False(IranianBankingRules.IsValidAccountNumber(null));
        Assert.False("".IsValidIranianCardNumber());
    }

    [Fact]
    public void Card_accepts_luhn_and_persian_digits()
    {
        Assert.True(IranianBankingRules.IsValid(BankingFieldKind.Card, ValidCard));
        Assert.True(ValidCard.IsValidIranianCardNumber());
        Assert.True(ToPersianDigits(ValidCard).IsValidIranianCardNumber());
        Assert.True($"4111-1111-1111-1111".IsValidIranianCardNumber());
        Assert.False("4111111111111112".IsValidIranianCardNumber());
        Assert.False("1234".IsValidIranianCardNumber());
    }

    [Fact]
    public void Sheba_accepts_iran_mod97_and_rejects_bad_format()
    {
        string sheba = MakeIranSheba(ValidBban);
        Assert.Equal(26, sheba.Length);
        Assert.True(IranianBankingRules.IsValid(BankingFieldKind.Sheba, sheba));
        Assert.True(sheba.IsValidIranianSheba());
        Assert.True($" {sheba[..4]} {sheba[4..]} ".IsValidIranianSheba());
        Assert.False("IR000000000000000000000000".IsValidIranianSheba());
        Assert.False("DE89370400440532013000".IsValidIranianSheba());
        Assert.False("IR123".IsValidIranianSheba());
    }

    [Fact]
    public void Account_requires_eight_to_fifty_digits()
    {
        Assert.True(IranianBankingRules.IsValid(BankingFieldKind.Account, "12345678"));
        Assert.True("۱۲۳۴۵۶۷۸".HasAccountNumberFormat());
        Assert.False("1234567".HasAccountNumberFormat());
        Assert.False("1234567a".HasAccountNumberFormat());
        Assert.True(new string('1', 50).HasAccountNumberFormat());
        Assert.False(new string('1', 51).HasAccountNumberFormat());
        Assert.True("123456789012".HasAccountNumberFormat(20));
        Assert.False("123456789012".HasAccountNumberFormat(10));
    }

    private static string ToPersianDigits(string value)
    {
        var chars = value.Select(ch => ch is >= '0' and <= '9'
            ? (char)('۰' + (ch - '0'))
            : ch);
        return new string(chars.ToArray());
    }

    private static string MakeIranSheba(string bban22)
    {
        string body = bban22 + "IR00";
        int remainder = 0;
        foreach (char character in body)
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

        int check = 98 - remainder;
        return $"IR{check:00}{bban22}";
    }
}
