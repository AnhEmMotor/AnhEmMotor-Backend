namespace Application.Interfaces.Services;

public record StoreChatHistoryItem(string Role, string Message);

public record StoreChatAiReplyResult(string Text, string? CardsJson);

public interface IStoreChatAiClient
{
    public Task<StoreChatAiReplyResult> GetReplyAsync(
        Guid sessionId,
        string visitorMessage,
        IReadOnlyList<StoreChatHistoryItem> history,
        CancellationToken cancellationToken);
}
