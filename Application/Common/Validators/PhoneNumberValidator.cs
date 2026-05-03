using FluentValidation;
using PhoneNumbers;

namespace Application.Common.Validators;

public static class PhoneNumberValidator
{
    public static IRuleBuilderOptions<T, string?> MustBeValidPhoneNumber<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(IsValidPhoneNumber)
            .WithMessage("Định dạng số điện thoại không hợp lệ.");
    }

    private static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return true;
        }
        try
        {
            var phoneUtil = PhoneNumberUtil.GetInstance();
            var numberProto = phoneUtil.Parse(phoneNumber, "VN");
            return phoneUtil.IsValidNumber(numberProto);
        } catch (NumberParseException)
        {
            return false;
        }
    }
}
