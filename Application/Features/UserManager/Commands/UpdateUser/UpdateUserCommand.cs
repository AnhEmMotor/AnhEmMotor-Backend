using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<Result<UserDTOForManagerResponse>>
{
    public Guid? UserId { get; init; }

    public string? FullName { get; init; }

    public string? Gender { get; init; }

    public string? PhoneNumber { get; init; }
}
