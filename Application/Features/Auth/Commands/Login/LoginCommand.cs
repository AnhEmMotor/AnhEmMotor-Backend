using Application.ApiContracts.Auth.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Login;

public record LoginCommand(string UsernameOrEmail, string Password) : IRequest<Result<LoginResponse>>;
