using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetCustomerProfileForChat;

public sealed record GetCustomerProfileForChatQuery : IRequest<Result<ChatToolEnvelope<ChatCustomerProfileDto>>>
{
    public int CustomerId { get; init; }
}
