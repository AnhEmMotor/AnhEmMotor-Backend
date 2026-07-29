using Application.DTOs.Chat;

namespace Application.Interfaces.Services;

public interface IChatRunEventBus
{
    public void Publish(Guid runId, ChatRunEventDto evt);
    public IAsyncEnumerable<ChatRunEventDto> SubscribeAsync(Guid runId, CancellationToken ct);
}
