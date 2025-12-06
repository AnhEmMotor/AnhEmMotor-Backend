using Application.ApiContracts.Auth.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber,
    string Gender
) : IRequest<RegistrationSuccessResponse>;
