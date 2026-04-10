using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.FacebookLogin;

public record FacebookLoginCommand : IRequest<Result<LoginResponse>>
{
    public required string AccessToken { get; init; }
}
