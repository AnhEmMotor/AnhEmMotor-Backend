using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public record UpdateCurrentUserCommand : IRequest<Result<UserDTOForManagerResponse>>
{
    public string? UserId { get; init; }
    public string? FullName { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
}
