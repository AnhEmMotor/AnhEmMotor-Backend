using Application.Common.Models;
using Application.Interfaces.Repositories.User;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler(IUserUpdateRepository userUpdateRepository) : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(request.UserId is not null && Guid.TryParse(request.UserId, out var userId))
        {
            await userUpdateRepository.ClearRefreshTokenAsync(userId, cancellationToken).ConfigureAwait(false);
        }
        return Result.Success();
    }
}
