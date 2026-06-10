using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Logistics.Queries.GetFulfillmentDetail;

public class GetFulfillmentDetailQueryHandler : IRequestHandler<GetFulfillmentDetailQuery, FulfillmentDetailResponse>
{
    private readonly IApplicationDbContext _context;

    public GetFulfillmentDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FulfillmentDetailResponse> Handle(GetFulfillmentDetailQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.ParcelDeliveryOrders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (order == null)
            return null; // Normally we throw NotFoundException

        return new FulfillmentDetailResponse
        {
            Id = order.Id,
            TrackingNumber = order.TrackingNumber,
            OriginalOrderCode = order.OriginalOrderCode,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerAddress = order.CustomerAddress,
            Carrier = order.Carrier,
            Status = order.Status,
            CodAmount = order.CodAmount,
            ShippingCost = order.ShippingCost,
            CreatedAt = order.CreatedAt,
            ExpectedAt = order.ExpectedAt,
            DeliveredAt = order.DeliveredAt,
            Items = order.Items.Select(i => new FulfillmentItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Sku = i.Sku,
                ThumbnailUrl = i.ThumbnailUrl,
                ShelfLocation = i.ShelfLocation,
                Quantity = i.Quantity,
                IsPicked = i.IsPicked,
                IsRestricted = i.IsRestricted,
                IsOutOfStock = i.IsOutOfStock
            }).ToList()
        };
    }
}
