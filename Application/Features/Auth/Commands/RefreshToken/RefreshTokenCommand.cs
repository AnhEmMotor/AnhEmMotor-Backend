using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<Result<GetAccessTokenFromRefreshTokenResponse>>
{
    public string? AccessToken { get; init; }

    public string? RefreshToken { get; init; }
}
