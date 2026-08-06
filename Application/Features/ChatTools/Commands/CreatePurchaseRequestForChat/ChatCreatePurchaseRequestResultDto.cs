namespace Application.Features.ChatTools.Commands.CreatePurchaseRequestForChat;

/// <summary>
/// DTO xác nhận cho chatbot sau khi tạo yêu cầu mua hàng thành công.
/// </summary>
public sealed record ChatCreatePurchaseRequestResultDto
{
    public int PurchaseRequestId { get; init; }

    public string Status { get; init; } = string.Empty;

    public int ItemCount { get; init; }
}
