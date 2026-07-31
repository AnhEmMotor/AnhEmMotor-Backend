using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetProductDetailForChat;

public sealed record GetProductDetailForChatQuery : IRequest<Result<ChatToolEnvelope<ChatProductDetailDto>>>
{
    public int ProductId { get; init; }
}
