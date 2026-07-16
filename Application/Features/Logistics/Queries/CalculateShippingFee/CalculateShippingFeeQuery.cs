using Application.Common.Models;
using MediatR;

namespace Application.Features.Logistics.Queries.CalculateShippingFee;

public class CalculateShippingFeeItemDto
{
    public int ProductVariantId { get; set; }
    public int? ProductVariantColorId { get; set; }
    public int Quantity { get; set; }
}

public class CalculateShippingFeeQuery : IRequest<Result<decimal>>
{
    public int ProvinceId { get; set; }
    public string WardId { get; set; } = string.Empty;
    public List<CalculateShippingFeeItemDto> Items { get; set; } = new();
}

