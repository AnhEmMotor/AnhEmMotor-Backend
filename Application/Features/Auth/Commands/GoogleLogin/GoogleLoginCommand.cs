using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand : IRequest<Result<LoginResponse>>
{
    public required string IdToken { get; init; }
}
