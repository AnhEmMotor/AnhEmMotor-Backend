using Application.ApiContracts.UserManager.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.UserManager.Commands.CreateUserByManager;

public record CreateUserByManagerCommand : IRequest<Result<UserDTOForManagerResponse>>
{
    private readonly string? _username;
    public string? Username 
    { 
        get => _username; 
        init => _username = value?.Trim(); 
    }

    private readonly string? _email;
    public string? Email 
    { 
        get => _email; 
        init => _email = value?.Trim(); 
    }

    public string? Password { get; init; }

    public string? FullName { get; init; }

    public string? PhoneNumber { get; init; }

    public string? Gender { get; init; }

    public List<string>? RoleNames { get; init; }

    public string? Status { get; init; }
}
