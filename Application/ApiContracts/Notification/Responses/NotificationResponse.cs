using System;

namespace Application.ApiContracts.Notification.Responses;

public class NotificationResponse
{
    public string? Type { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTimeOffset? Timestamp { get; init; }
}
