using MediatR;

namespace Application.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand>
{
    public Task Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Google login not implemented yet.");
    }
}
