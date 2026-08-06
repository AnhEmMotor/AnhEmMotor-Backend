using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetWarrantyTermsForChat;

public sealed record GetWarrantyTermsForChatQuery : IRequest<Result<ChatToolEnvelope<ChatWarrantyTermDto>>>
{
    public int Limit { get; init; } = 10;
}
