using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Commands.CreatePurchaseRequestForChat;

/// <summary>
/// Item tối giản cho chatbot — chỉ variant + số lượng, không hỗ trợ chọn màu/nhà cung cấp/giá ở v1.
/// </summary>
public sealed record ChatCreatePurchaseRequestItemInput
{
    public int ProductVariantId { get; init; }

    public int Quantity { get; init; }
}

/// <summary>
/// Tool ghi dữ liệu (write) đầu tiên của chatbot — tạo yêu cầu mua hàng (draft) bằng cách tái sử dụng <see
/// cref="Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest.CreatePurchaseRequestCommand" />.
/// </summary>
public sealed record CreatePurchaseRequestForChatCommand : IRequest<Result<ChatToolEnvelope<ChatCreatePurchaseRequestResultDto>>>
{
    public List<ChatCreatePurchaseRequestItemInput> Items { get; init; } = [];

    public string? Note { get; init; }
}
