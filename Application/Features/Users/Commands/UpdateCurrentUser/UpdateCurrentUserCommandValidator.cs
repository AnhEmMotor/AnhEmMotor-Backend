using Domain.Constants;
using FluentValidation;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public sealed class UpdateCurrentUserCommandValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is missing.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Invalid User ID format.");

        RuleFor(x => x.Gender)
            .NotEmpty()
            .WithMessage("Gender is required.")
            .Must(GenderStatus.IsValid)
            .WithMessage("Invalid gender. Please check again.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full Name is required.")
            .MaximumLength(100)
            .WithMessage("Full Name cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone Number is required.")
            .Must(IsValidPhoneNumber)
            .WithMessage("Invalid phone number format");
    }

    private bool IsValidPhoneNumber(string? phoneNumber)
    {
        if(string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        try
        {
            var phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
            var numberProto = phoneUtil.Parse(phoneNumber, "VN");
            return phoneUtil.IsValidNumber(numberProto);
        } catch(PhoneNumbers.NumberParseException)
        {
            return false;
        }
    }
}
