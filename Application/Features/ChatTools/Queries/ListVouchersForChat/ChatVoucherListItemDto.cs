using Domain.Enums;

namespace Application.Features.ChatTools.Queries.ListVouchersForChat;

public record ChatVoucherListItemDto
{
    public int Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public DiscountType DiscountType { get; init; }

    public decimal DiscountValue { get; init; }

    public decimal? MaxDiscountAmount { get; init; }

    public string Currency { get; init; } = "VND";

    public DateTime ValidFrom { get; init; }

    public DateTime ValidTo { get; init; }
}
