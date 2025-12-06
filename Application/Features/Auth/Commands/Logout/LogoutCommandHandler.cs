using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId is not null)
        {
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user is not null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTimeOffset.MinValue;
                await userManager.UpdateAsync(user);
            }
        }
    }
}
