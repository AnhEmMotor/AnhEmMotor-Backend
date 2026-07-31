using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetRepairOrderDetailForChat;

public sealed record GetRepairOrderDetailForChatQuery : IRequest<Result<ChatToolEnvelope<ChatRepairOrderDetailDto>>>
{
    public int RepairOrderId { get; init; }
}
