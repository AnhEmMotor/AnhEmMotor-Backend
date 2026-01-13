using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.ChangePasswordCurrentUser;

public record ChangePasswordCurrentUserCommand : IRequest<Result<ChangePasswordUserByUserResponse>>
{
    public string? UserId { get; init; }

    public string? CurrentPassword { get; init; }

    public string? NewPassword { get; init; }
}
