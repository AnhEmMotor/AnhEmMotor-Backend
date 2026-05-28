using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.Users.Commands.UpdateCurrentUser;

public record UpdateCurrentUserCommand : IRequest<Result<UserDTOForManagerResponse>>
{
    public string? FullName { get; init; }

    public string? Gender { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; init; }
}
