using FluentValidation;

namespace Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    { 
        // Allow empty RefreshToken; the Handler will return 401 Unauthorized if it's missing.
    }
}