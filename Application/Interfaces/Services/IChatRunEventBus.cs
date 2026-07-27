using Application.DTOs.Chat;

namespace Application.Interfaces.Services;

public interface IChatRunEventBus
{
    void Publish(Guid runId, ChatRunEventDto evt);
    IAsyncEnumerable<ChatRunEventDto> SubscribeAsync(Guid runId, CancellationToken ct);
}
