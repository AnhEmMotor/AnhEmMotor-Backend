using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetPayrollSummaryForChat;

public sealed record GetPayrollSummaryForChatQuery : IRequest<Result<ChatToolEnvelope<ChatPayrollSummaryItemDto>>>
{
    public int? EmployeeId { get; init; }

    public int? Month { get; init; }

    public int? Year { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
