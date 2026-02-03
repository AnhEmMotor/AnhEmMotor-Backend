using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<Result<RegisterResponse>>
{
    public string? Username { get; init; }

    public string? Email { get; init; }

    public string? Password { get; init; }

    public string? FullName { get; init; }

    public string? PhoneNumber { get; init; }

    public string? Gender { get; init; }
}
