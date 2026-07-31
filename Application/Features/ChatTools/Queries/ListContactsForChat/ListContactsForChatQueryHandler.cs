using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Contact;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListContactsForChat;

public class ListContactsForChatQueryHandler(
    IContactReadRepository contactReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<ListContactsForChatQuery, Result<ChatToolEnvelope<ChatContactListItemDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatContactListItemDto>>> Handle(
        ListContactsForChatQuery request,
        CancellationToken cancellationToken)
    {
        var contacts = await contactReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var ordered = contacts.OrderByDescending(c => c.CreatedAt).ToList();
        var limit = ChatToolLimit.Clamp(request.Limit);
        var dtos = ordered
            .Take(limit)
            .Select(
                c => new ChatContactListItemDto
                {
                    ContactId = c.Id,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Subject = c.Subject,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt
                })
            .ToList();
        var inner = new ChatToolResult<ChatContactListItemDto>(dtos, ordered.Count, ordered.Count > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "IContactReadRepository.GetAllAsync",
            new Dictionary<string, string> { ["Sắp xếp"] = "Mới nhất trước" },
            "lien-he",
            null);
        return ChatToolEnvelope<ChatContactListItemDto>.Wrap(inner, meta);
    }
}
