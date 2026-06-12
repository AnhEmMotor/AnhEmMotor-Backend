using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Returns.Queries.GetReturnDetail;

public class GetReturnDetailQuery : IRequest<ReturnDetailDto?>
{
    public int Id { get; set; }
}

public class ReturnDetailDto
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string OriginalTrackingNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? BoxCondition { get; set; }
    public string? ProductCondition { get; set; }
    public string? ReturnProofImage { get; set; }
    public string? ReturnInternalNote { get; set; }
    public string? ReturnAction { get; set; }

    public List<ReturnDetailItemDto> Items { get; set; } = new();
}

public class ReturnDetailItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int Quantity { get; set; }
}

public class GetReturnDetailQueryHandler(IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository)
    : IRequestHandler<GetReturnDetailQuery, ReturnDetailDto?>
{
    public async Task<ReturnDetailDto?> Handle(GetReturnDetailQuery request, CancellationToken cancellationToken)
    {
        var order = await parcelDeliveryOrderReadRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order == null || order.Status != ParcelDeliveryStatus.Returned) return null;

        return new ReturnDetailDto
        {
            Id = order.Id,
            TrackingNumber = order.TrackingNumber ?? string.Empty,
            OriginalTrackingNumber = order.TrackingNumber ?? string.Empty,
            CustomerName = order.CustomerName ?? string.Empty,
            Carrier = order.Carrier ?? string.Empty,
            Reason = order.ReturnReason ?? "Khách bom hàng",
            Status = GetReturnStatus(order),
            CreatedAt = order.CreatedAt,
            BoxCondition = order.BoxCondition,
            ProductCondition = order.ProductCondition,
            ReturnProofImage = order.ReturnProofImage,
            ReturnInternalNote = order.ReturnInternalNote,
            ReturnAction = order.ReturnAction,
            Items = order.Items.Select(i => new ReturnDetailItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Sku = i.Sku,
                ThumbnailUrl = i.ThumbnailUrl,
                Quantity = i.Quantity
            }).ToList()
        };
    }

    private string GetReturnStatus(ParcelDeliveryOrder order)
    {
        if (order.ReturnAction != null) return "completed";
        if (order.InspectedAt != null) return "inspecting";
        return "pending";
    }
}
