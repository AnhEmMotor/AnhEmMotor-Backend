using System.Text.RegularExpressions;
using Application.DTOs.StoreChat;
using Application.Interfaces.Repositories.StoreChat;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.StoreChat;

public partial class StoreChatReadRepository(ApplicationDBContext context) : IStoreChatReadRepository
{
    // Tin nhắn Staff soạn bằng rich-text (WangEditor) là HTML — preview ở danh sách phiên chỉ hiện chữ
    // trơn, không phải chỗ render HTML, nên phải bỏ thẻ trước khi hiện, không thì hiện nguyên văn "<p>...".
    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();

    private static string? StripHtmlTags(string? content) =>
        string.IsNullOrEmpty(content) ? content : HtmlTagRegex().Replace(content, string.Empty).Trim();

    public async Task<StoreChatSession?> GetSessionByVisitorKeyAsync(string visitorKey, CancellationToken cancellationToken = default)
    {
        return await context.StoreChatSessions
            .FirstOrDefaultAsync(s => s.VisitorKey == visitorKey, cancellationToken);
    }

    public async Task<StoreChatSession?> GetDeletedSessionByVisitorKeyAsync(string visitorKey, CancellationToken cancellationToken = default)
    {
        return await context.All<StoreChatSession>()
            .FirstOrDefaultAsync(s => s.VisitorKey == visitorKey && s.DeletedAt != null, cancellationToken);
    }

    public async Task<StoreChatSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.StoreChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<List<StoreChatMessage>> GetHistoryAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.StoreChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // ponytail: cap 200 phiên mới nhất, chưa phân trang — thêm khi số phiên Ai tồn đọng vượt ngưỡng
    public async Task<List<StoreChatSessionListItemDto>> GetSessionsForStaffAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await context.StoreChatSessions
            .OrderByDescending(s => s.LastMessageAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        var sessionIds = sessions.Select(s => s.Id).ToList();
        var lastMessageBySession = await context.StoreChatMessages
            .Where(m => sessionIds.Contains(m.SessionId))
            .GroupBy(m => m.SessionId)
            .Select(g => g.OrderByDescending(m => m.CreatedAt).First())
            .ToDictionaryAsync(m => m.SessionId, m => m.Content, cancellationToken);

        // Gộp chung 1 query cho cả tên nhân viên phụ trách và tên khách đã đăng nhập (CustomerUserId) —
        // cả 2 đều là Guid trỏ vào Users, không cần 2 lượt query riêng.
        var userIds = sessions
            .SelectMany(s => new[] { s.AssignedStaffId, s.CustomerUserId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var nameById = await context.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        return sessions.Select(s => new StoreChatSessionListItemDto
        {
            Id = s.Id,
            Mode = s.Mode,
            ContactName = s.ContactName,
            ContactPhone = s.ContactPhone,
            CustomerName = s.CustomerUserId.HasValue
                ? nameById.GetValueOrDefault(s.CustomerUserId.Value)
                : null,
            PreviousSessionId = s.PreviousSessionId,
            AssignedStaffId = s.AssignedStaffId,
            AssignedStaffName = s.AssignedStaffId.HasValue
                ? nameById.GetValueOrDefault(s.AssignedStaffId.Value)
                : null,
            LastMessageAt = s.LastMessageAt,
            LastMessagePreview = StripHtmlTags(lastMessageBySession.GetValueOrDefault(s.Id))
        }).ToList();
    }

    public async Task<string?> GetStaffNameAsync(Guid staffId, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Where(u => u.Id == staffId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
