using System;
using System.Collections.Generic;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Queries.GetFulfillmentDetail;

public class GetFulfillmentDetailQuery : IRequest<FulfillmentDetailResponse>
{
    public int Id { get; set; }
}

public class FulfillmentDetailResponse
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string OriginalOrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public ParcelDeliveryStatus Status { get; set; }
    public decimal CodAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpectedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    
    public List<FulfillmentItemDto> Items { get; set; } = new();
}

public class FulfillmentItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string ShelfLocation { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool IsPicked { get; set; }
    public bool IsRestricted { get; set; }
    public bool IsOutOfStock { get; set; }
}
