using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.ListEmployeesForChat;

public sealed record ListEmployeesForChatQuery : IRequest<Result<ChatToolEnvelope<ChatEmployeeListItemDto>>>
{
    public string? Keyword { get; init; }

    public int Limit { get; init; } = ChatToolLimit.Default;
}
