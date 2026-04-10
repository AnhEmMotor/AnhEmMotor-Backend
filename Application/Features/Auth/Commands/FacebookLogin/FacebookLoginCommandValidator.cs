using FluentValidation;

namespace Application.Features.Auth.Commands.FacebookLogin;

public class FacebookLoginCommandValidator : AbstractValidator<FacebookLoginCommand>
{
    public FacebookLoginCommandValidator()
    { RuleFor(v => v.AccessToken).NotEmpty().WithMessage("AccessToken is required."); }
}
