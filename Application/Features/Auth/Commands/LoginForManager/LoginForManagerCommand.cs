using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.LoginForManager;

public record LoginForManagerCommand : IRequest<Result<LoginResponse>>
{
    public string? UsernameOrEmail { get; init; }

    public string? Password { get; init; }
}
