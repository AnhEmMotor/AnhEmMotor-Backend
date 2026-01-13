using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.ChangePasswordCurrentUser;

public record ChangePasswordCurrentUserCommand : IRequest<Result<ChangePasswordUserByUserResponse>>
{
    public string? UserId { get; set; }

    public string? CurrentPassword { get; set; }

    public string? NewPassword { get; set; }
}
