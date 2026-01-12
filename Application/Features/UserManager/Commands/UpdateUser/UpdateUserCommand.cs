using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<Result<UserDTOForManagerResponse>>
{
    public Guid? UserId { get; set; }
    public string? FullName { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
}
