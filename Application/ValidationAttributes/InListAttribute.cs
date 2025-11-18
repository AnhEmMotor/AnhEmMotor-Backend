using System.ComponentModel.DataAnnotations;

namespace Application.ValidationAttributes;

public sealed class InListAttribute(string allowedValues) : ValidationAttribute
{
    private readonly string[] _AllowedValues = [.. allowedValues
            .Split(',')
            .Select(s => s.Trim())];

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }
        var stringValue = value.ToString();
        if (!_AllowedValues.Contains(stringValue, StringComparer.OrdinalIgnoreCase))
        {
            var allowedList = string.Join(", ", _AllowedValues);
            var error = string.IsNullOrEmpty(ErrorMessage)
                ? $"{validationContext.DisplayName} phải là một trong các giá trị: {allowedList}."
                : ErrorMessage;
            return new ValidationResult(error, [validationContext.MemberName!]);
        }
        return ValidationResult.Success;
    }
}
