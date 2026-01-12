using Application.Common.Models;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public record LogoutCommand : IRequest<Result>
{
    public string? UserId { get; set; }
}
