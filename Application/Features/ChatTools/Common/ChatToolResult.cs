namespace Application.Features.ChatTools.Common;

public record ChatToolResult<T>(IReadOnlyList<T> Items, int TotalCount, bool Truncated);
