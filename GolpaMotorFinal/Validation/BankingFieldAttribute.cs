using System.ComponentModel.DataAnnotations;
using DomainModel.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GolpaMotorFinal.Validation;

public abstract class BankingFieldAttribute : ValidationAttribute, IClientModelValidator
{
    protected BankingFieldAttribute(BankingFieldKind kind)
    {
        Kind = kind;
    }

    public BankingFieldKind Kind { get; }

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        if (value is not string text)
            return false;

        if (string.IsNullOrWhiteSpace(text))
            return true;

        return IranianBankingRules.IsValid(Kind, text);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(
            context.Attributes,
            "data-val-bankingfield",
            FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
        MergeAttribute(
            context.Attributes,
            "data-val-bankingfield-kind",
            Kind.ToString().ToLowerInvariant());

        if (Kind == BankingFieldKind.Account)
        {
            MergeAttribute(
                context.Attributes,
                "data-val-bankingfield-min",
                IranianBankingRules.AccountNumberMinLength.ToString());
            MergeAttribute(
                context.Attributes,
                "data-val-bankingfield-max",
                IranianBankingRules.AccountNumberMaxLength.ToString());
        }
    }

    private static void MergeAttribute(
        IDictionary<string, string> attributes,
        string key,
        string value)
    {
        attributes.TryAdd(key, value);
    }
}

public sealed class IranianCardNumberAttribute : BankingFieldAttribute
{
    public IranianCardNumberAttribute()
        : base(BankingFieldKind.Card)
    {
        ErrorMessage = "شماره کارت معتبر نیست.";
    }
}

public sealed class IranianShebaAttribute : BankingFieldAttribute
{
    public IranianShebaAttribute()
        : base(BankingFieldKind.Sheba)
    {
        ErrorMessage = "شماره شبا معتبر نیست.";
    }
}

public sealed class IranianAccountNumberAttribute : BankingFieldAttribute
{
    public IranianAccountNumberAttribute()
        : base(BankingFieldKind.Account)
    {
        ErrorMessage = "شماره حساب معتبر نیست.";
    }
}
