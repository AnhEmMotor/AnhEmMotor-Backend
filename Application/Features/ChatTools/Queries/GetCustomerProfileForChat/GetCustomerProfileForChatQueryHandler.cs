using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.ChatTools.Common;
using Application.Interfaces.Repositories.Lead.Lead;
using Application.Interfaces.Repositories.Output;
using Domain.Constants.Order;
using MediatR;
using System.Linq;

namespace Application.Features.ChatTools.Queries.GetCustomerProfileForChat;

public class GetCustomerProfileForChatQueryHandler(
    ILeadReadRepository leadReadRepository,
    IOutputReadRepository outputReadRepository,
    IServerDateProvider dateProvider) : IRequestHandler<GetCustomerProfileForChatQuery, Result<ChatToolEnvelope<ChatCustomerProfileDto>>>
{
    public async Task<Result<ChatToolEnvelope<ChatCustomerProfileDto>>> Handle(
        GetCustomerProfileForChatQuery request,
        CancellationToken cancellationToken)
    {
        var lead = await leadReadRepository.GetByIdAsync(request.CustomerId, cancellationToken).ConfigureAwait(false);
        if (lead == null)
        {
            return Result<ChatToolEnvelope<ChatCustomerProfileDto>>.Failure(Error.NotFound("Không tìm thấy khách hàng"));
        }
        var outputs = await outputReadRepository.GetByLeadIdAsync(request.CustomerId, cancellationToken)
            .ConfigureAwait(false);
        var dto = new ChatCustomerProfileDto
        {
            CustomerId = lead.Id,
            FullName = lead.FullName,
            PhoneNumber = lead.PhoneNumber,
            TotalOrders = outputs.Count(o => o.StatusId != OrderStatus.Cancelled),
            TotalSpent = outputs.Where(o => o.StatusId == OrderStatus.Completed).Sum(o => o.Total),
            Tier = lead.Tier
        };
        var meta = new ChatToolEnvelopeMeta(
            dateProvider.VietnamNow,
            "ILeadReadRepository.GetByIdAsync",
            new Dictionary<string, string>(),
            "ho-so-khach-hang",
            null);
        return ChatToolEnvelope<ChatCustomerProfileDto>.WrapSingle(dto, meta);
    }
}
