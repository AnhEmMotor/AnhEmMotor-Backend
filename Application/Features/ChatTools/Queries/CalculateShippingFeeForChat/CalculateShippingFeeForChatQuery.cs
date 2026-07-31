using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.CalculateShippingFeeForChat;

public sealed record CalculateShippingFeeForChatQuery : IRequest<Result<ChatToolEnvelope<ChatShippingFeeDto>>>
{
    public int ProvinceId { get; init; }

    public string WardId { get; init; } = string.Empty;

    public int ProductVariantId { get; init; }

    public int Quantity { get; init; } = 1;
}
