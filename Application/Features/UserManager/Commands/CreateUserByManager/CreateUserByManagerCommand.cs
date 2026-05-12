using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.CreateUserByManager;

public record CreateUserByManagerCommand : IRequest<Result<UserDTOForManagerResponse>>
{
    public string? Username { get; init; }

    public string? Email { get; init; }

    public string? Password { get; init; }

    public string? FullName { get; init; }

    public string? PhoneNumber { get; init; }

    public string? Gender { get; init; }

    public List<string>? RoleNames { get; init; }

    public string? Status { get; init; }
}
