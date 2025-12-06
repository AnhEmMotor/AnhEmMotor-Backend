using Application.ApiContracts.Auth.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<GetAccessTokenFromRefreshTokenResponse>;
