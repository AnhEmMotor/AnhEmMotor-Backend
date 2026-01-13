using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result>
{
    public Task<Result> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    { throw new NotImplementedException(); }
}
