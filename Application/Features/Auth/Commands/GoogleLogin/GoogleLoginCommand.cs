using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand : IRequest<Result>
{
    public string? IdToken { get; init; }
}
