using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetCommissionRecordsForChat;

public sealed record GetCommissionRecordsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatCommissionRecordItemDto>>>
{
    public int? EmployeeId { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
