using Application.ApiContracts.Notification.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Notifications.Queries.GetNotificationStream;

public record GetNotificationStreamQuery : IRequest<IAsyncEnumerable<Result<NotificationResponse>>>;
