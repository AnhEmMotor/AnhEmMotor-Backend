using Application.ApiContracts.Auth.Responses;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public record LoginCommand(string UsernameOrEmail, string Password) : IRequest<LoginResponse>;
