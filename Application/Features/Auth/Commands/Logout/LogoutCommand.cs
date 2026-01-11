using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string? UserId) : IRequest<Result>;
