using Application.ApiContracts.User.Responses;
using Application.Common.Models;
using Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace Application.Features.Users.Queries.GetCurrentUser;

public class GetCurrentUserStreamQueryHandler(
    IUserStreamService userStreamService,
    ICurrentUserContext currentUserContext,
    IServiceProvider serviceProvider) : IRequestHandler<GetCurrentUserStreamQuery, IAsyncEnumerable<Result<UserResponse>>>
{
    public Task<IAsyncEnumerable<Result<UserResponse>>> Handle(
        GetCurrentUserStreamQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserContext.GetUserId();
        return Task.FromResult(GetStreamAsync(userId.ToString(), cancellationToken));
    }

    private async IAsyncEnumerable<Result<UserResponse>> GetStreamAsync(
        string? userIdString,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            yield return Error.BadRequest("Invalid user ID.");
            yield break;
        }
        using (var initialScope = serviceProvider.CreateScope())
        {
            var initialMediator = initialScope.ServiceProvider.GetRequiredService<IMediator>();
            var initialResult = await initialMediator.Send(new GetCurrentUserQuery(), cancellationToken)
                .ConfigureAwait(false);
            yield return initialResult;
        }
        while (!cancellationToken.IsCancellationRequested)
        {
            await userStreamService.WaitForUpdateAsync(userId, cancellationToken).ConfigureAwait(true);
            using var scope = serviceProvider.CreateScope();
            var scopedMediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var result = await scopedMediator.Send(new GetCurrentUserQuery(), cancellationToken).ConfigureAwait(false);
            yield return result;
        }
    }
}
