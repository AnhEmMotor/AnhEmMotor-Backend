using Application.ApiContracts.Notification.Responses;
using Application.Common.Models;
using Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Notifications.Queries.GetNotificationStream;

public class GetNotificationStreamQueryHandler(INotificationService notificationService)
    : IRequestHandler<GetNotificationStreamQuery, IAsyncEnumerable<Result<NotificationResponse>>>
{
    public Task<IAsyncEnumerable<Result<NotificationResponse>>> Handle(GetNotificationStreamQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(GetStreamAsync(cancellationToken));
    }

    private async IAsyncEnumerable<Result<NotificationResponse>> GetStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // 1. Send connection message immediately
        yield return new NotificationResponse
        {
            Message = "Connected to notification stream"
        };

        // 2. Loop and wait for updates
        while (!cancellationToken.IsCancellationRequested)
        {
            string message;
            try
            {
                message = await notificationService.WaitForNotificationAsync(cancellationToken).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
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
