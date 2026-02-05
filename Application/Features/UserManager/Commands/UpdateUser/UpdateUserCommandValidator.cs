using System.Text.RegularExpressions;
using Domain.Constants;
using FluentValidation;

namespace Application.Features.UserManager.Commands.UpdateUser;

public partial class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required.")
            .MaximumLength(255).WithMessage("Full Name must not exceed 255 characters.");

        RuleFor(x => x.PhoneNumber)
            .Must(IsValidPhoneNumber).WithMessage("Invalid phone number.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Gender)
            .Must(IsValidGender).WithMessage("Invalid gender.")
            .When(x => !string.IsNullOrEmpty(x.Gender));
    }

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        // Simple regex for email validation
        // Allow user+tag@example.co.uk
        // Allow user.name@example.com
        var regex = RegexCheckEmail();
        return regex.IsMatch(email.Trim());
    }

    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
        // Vietnamese phone number format: starts with 0 or 84 or +84, followed by 9 digits
        var regex = RegexCheckPhone();
        return regex.IsMatch(phoneNumber.Trim());
    }

    public static bool IsValidGender(string? gender)
    {
        return ValidGenders.Contains(gender?.Trim());
    }

    public static IReadOnlyList<string> ValidGenders =>
    [
        GenderStatus.Male,
        GenderStatus.Female,
        GenderStatus.Other
    ];

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex RegexCheckEmail();
    [GeneratedRegex(@"^(0|84|\+84)[0-9]{9}$")]
    private static partial Regex RegexCheckPhone();
}
