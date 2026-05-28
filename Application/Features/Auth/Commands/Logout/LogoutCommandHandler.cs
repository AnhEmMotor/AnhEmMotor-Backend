using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(IUserUpdateRepository userUpdateRepository, ICookieTokenManager cookieTokenManager, ICurrentUserContext currentUserContext) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var userIdFromToken = currentUserContext.GetUserId();
        await userUpdateRepository.ClearRefreshTokenAsync(userIdFromToken, cancellationToken).ConfigureAwait(false);
        cookieTokenManager.DeleteRefreshToken();
        return Result.Success();
    }
}
