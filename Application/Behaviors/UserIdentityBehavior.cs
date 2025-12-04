using Application.Interfaces.Authentication;
using MediatR;

namespace Application.Behaviors;

/// <summary>
/// Behavior tự động gán UserId vào request nếu request implement IHaveUserId
/// </summary>
public class UserIdentityBehavior<TRequest, TResponse>(ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IHaveUserId userIdRequest)
        {
            var userId = currentUserService.UserId;
            if (userId.HasValue)
            {
                userIdRequest.UserId = userId.Value;
            }
        }

        return await next();
    }
}
