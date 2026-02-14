using Application.Common.Validators;
using Domain.Constants;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Application.Features.UserManager.Commands.UpdateUser;

public partial class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full Name is required.")
            .MaximumLength(255)
            .WithMessage("Full Name must not exceed 255 characters.");

        RuleFor(x => x.PhoneNumber).MustBeValidPhoneNumber().When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Gender)
            .Must(IsValidGender)
            .WithMessage("Invalid gender.")
            .When(x => !string.IsNullOrEmpty(x.Gender));
    }

    public static bool IsValidEmail(string? email)
    {
        if(string.IsNullOrWhiteSpace(email))
            return false;
        var regex = RegexCheckEmail();
        return regex.IsMatch(email.Trim());
    }

    public static bool IsValidGender(string? gender) { return ValidGenders.Contains(gender?.Trim()); }

    public static IReadOnlyList<string> ValidGenders => [ GenderStatus.Male, GenderStatus.Female, GenderStatus.Other ];

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex RegexCheckEmail();
}
