using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(IUserUpdateRepository userUpdateRepository, IHttpTokenAccessorService httpTokenAccessor) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var userIdFromToken = httpTokenAccessor.GetUserId();
        if (userIdFromToken is not null && Guid.TryParse(userIdFromToken, out var userId))
        {
            await userUpdateRepository.ClearRefreshTokenAsync(userId, cancellationToken).ConfigureAwait(false);
        }
        httpTokenAccessor.DeleteRefreshTokenFromCookie();
        return Result.Success();
    }
}
