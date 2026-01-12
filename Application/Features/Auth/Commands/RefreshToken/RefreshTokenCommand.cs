using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string? RefreshToken, string AccessToken) : IRequest<Result<GetAccessTokenFromRefreshTokenResponse>>;
