using Application.ApiContracts.Notification.Responses;
using Application.Common.Models;
using Application.Interfaces.Services;
using MediatR;
using System;
using System.Runtime.CompilerServices;

namespace Application.Features.Notifications.Queries.GetNotificationStream;

public class GetNotificationStreamQueryHandler(INotificationService notificationService) : IRequestHandler<GetNotificationStreamQuery, IAsyncEnumerable<Result<NotificationResponse>>>
{
    public Task<IAsyncEnumerable<Result<NotificationResponse>>> Handle(
        GetNotificationStreamQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(GetStreamAsync(cancellationToken));
    }

    private async IAsyncEnumerable<Result<NotificationResponse>> GetStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new NotificationResponse { Message = "Connected to notification stream" };
        while (!cancellationToken.IsCancellationRequested)
        {
            string message;
            try
            {
                message = await notificationService.WaitForNotificationAsync(cancellationToken).ConfigureAwait(false);
            } catch (OperationCanceledException)
            {
                yield break;
            }
            yield return new NotificationResponse
            {
                Type = "NewBooking",
                Message = message,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }
}
