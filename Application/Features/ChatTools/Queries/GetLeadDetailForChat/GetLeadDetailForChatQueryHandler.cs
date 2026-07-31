using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Lead.Lead;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetLeadDetailForChat;

public class GetLeadDetailForChatQueryHandler(
    ILeadReadRepository leadReadRepository,
    IServerDateProvider dateProvider)
    : IRequestHandler<GetLeadDetailForChatQuery, Result<ChatToolEnvelope<ChatLeadDetailDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatLeadDetailDto>>> Handle(
        GetLeadDetailForChatQuery request,
        CancellationToken cancellationToken)
    {
        var lead = await leadReadRepository.GetByIdAsync(request.LeadId, cancellationToken).ConfigureAwait(false);
        if (lead == null)
        {
            return Result<ChatToolEnvelope<ChatLeadDetailDto>>.Failure(Error.NotFound("Không tìm thấy khách hàng tiềm năng"));
        }
        var dto = new ChatLeadDetailDto
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
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ILeadReadRepository.GetByIdAsync",
            new Dictionary<string, string>(),
            "lead",
            null);
        return ChatToolEnvelope<ChatLeadDetailDto>.WrapSingle(dto, meta);
    }
}
