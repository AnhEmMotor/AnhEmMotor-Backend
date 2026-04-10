using FluentValidation;

namespace Application.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator() { RuleFor(v => v.IdToken).NotEmpty().WithMessage("IdToken is required."); }
}
