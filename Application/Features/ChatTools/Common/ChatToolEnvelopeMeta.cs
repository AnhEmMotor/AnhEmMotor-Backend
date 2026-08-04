namespace Application.Features.ChatTools.Common;

/// <summary>
/// Metadata cần khai báo tại chỗ gọi <see cref="ChatToolEnvelope{T}.Wrap" /> — xem 16.5.
/// </summary>
public sealed record ChatToolEnvelopeMeta(
    DateTimeOffset AsOf,
    string Source,
    IReadOnlyDictionary<string, string> FiltersApplied,
    string? Definition,
    string? Currency,
    IReadOnlyList<string>? Warnings = null);

/// <summary>
/// Envelope bắt buộc cho mọi tool chat — không tool nào được trả dữ liệu trần (Stage 16.5).
/// </summary>
public sealed record ChatToolEnvelope<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    bool Truncated,
    DateTimeOffset AsOf,
    string Timezone,
    string Source,
    IReadOnlyDictionary<string, string> FiltersApplied,
    string? Definition,
    string? Currency,
    IReadOnlyList<string> Warnings)
{
    public static ChatToolEnvelope<T> Wrap(ChatToolResult<T> inner, ChatToolEnvelopeMeta meta) => new(
        inner.Items,
        inner.TotalCount,
        inner.Truncated,
        meta.AsOf,
        "Asia/Ho_Chi_Minh",
        meta.Source,
        meta.FiltersApplied,
        meta.Definition,
        meta.Currency,
        meta.Warnings ?? []);

    /// <summary>
    /// Dùng cho tool trả 1 đối tượng đơn (không phải danh sách) — bọc thành envelope 1 phần tử.
    /// </summary>
    public static ChatToolEnvelope<T> WrapSingle(T item, ChatToolEnvelopeMeta meta) => Wrap(
        new ChatToolResult<T>([item], 1, false),
        meta);
}
