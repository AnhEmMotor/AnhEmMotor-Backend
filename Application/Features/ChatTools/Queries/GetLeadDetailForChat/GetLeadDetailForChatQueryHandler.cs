using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Lead.Lead;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetLeadDetailForChat;

public class GetLeadDetailForChatQueryHandler(ILeadReadRepository leadReadRepository, IServerDateProvider dateProvider) : IRequestHandler<GetLeadDetailForChatQuery, Result<ChatToolEnvelope<ChatLeadDetailDto>>>
{
    private const int MaxResults = 5;

    public async Task<Result<ChatToolEnvelope<ChatLeadDetailDto>>> Handle(
        GetLeadDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var leads = await leadReadRepository.GetLoyaltyMembersAsync(request.Keyword, cancellationToken)
            .ConfigureAwait(false);
        var totalCount = leads.Count;
        var dtos = leads
            .Take(MaxResults)
            .Select(
                lead => new ChatLeadDetailDto
                {
                    LeadId = lead.Id,
                    FullName = lead.FullName,
                    PhoneNumber = lead.PhoneNumber,
                    Email = lead.Email,
                    Status = lead.Status,
                    Source = lead.Source,
                    InterestedVehicle = lead.InterestedVehicle,
                    Score = lead.Score,
                    Tier = lead.Tier,
                    Points = lead.Points,
                    AssignedToName = lead.AssignedTo?.FullName,
                    CreatedAt = lead.CreatedAt ?? DateTimeOffset.MinValue
                })
            .ToList();
        var inner = new ChatToolResult<ChatLeadDetailDto>(dtos, totalCount, totalCount > dtos.Count);
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ILeadReadRepository.GetLoyaltyMembersAsync",
            new Dictionary<string, string>(),
            "lead",
            null);
        return ChatToolEnvelope<ChatLeadDetailDto>.Wrap(inner, meta);
    }
}
