using Application.DTOs.StoreChat;
using Domain.Entities;

namespace Application.Interfaces.Repositories.StoreChat;

public interface IStoreChatReadRepository
{
    public Task<StoreChatSession?> GetSessionByVisitorKeyAsync(
        string visitorKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Phiên đã xoá mềm còn giữ đúng VisitorKey — dùng để giải phóng key khỏi unique index khi khách quay lại với
    /// VisitorKey cũ.
    /// </summary>
    public Task<StoreChatSession?> GetDeletedSessionByVisitorKeyAsync(
        string visitorKey,
        CancellationToken cancellationToken = default);

    public Task<StoreChatSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    public Task<List<StoreChatMessage>> GetHistoryAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Danh sách phiên cho trang quản trị (Stage 06) — tối đa 200 phiên mới nhất, kèm tên nhân viên và tin nhắn cuối.
    /// </summary>
    public Task<List<StoreChatSessionListItemDto>> GetSessionsForStaffAsync(
        CancellationToken cancellationToken = default);

    public Task<string?> GetStaffNameAsync(Guid staffId, CancellationToken cancellationToken = default);
}
