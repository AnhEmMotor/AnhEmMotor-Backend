using Application.ApiContracts.Logistics.Responses;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Logistics.Queries.GetActiveShipments
{
    public class GetActiveShipmentsQueryHandler(IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository)
    : IRequestHandler<GetActiveShipmentsQuery, List<ActiveShipmentResponse>>
    {
        public async Task<List<ActiveShipmentResponse>> Handle(GetActiveShipmentsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var shipments = (await parcelDeliveryOrderReadRepository.GetAllAsync(cancellationToken).ConfigureAwait(false))
                .Where(x => x.Status == ParcelDeliveryStatus.Shipping)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            var result = shipments.Select(x => new ActiveShipmentResponse
            {
                Id = x.Id,
                TrackingNumber = x.TrackingNumber ?? string.Empty,
                CustomerName = x.CustomerName ?? string.Empty,
                CustomerPhone = x.CustomerPhone ?? string.Empty,
                CustomerAddress = x.CustomerAddress ?? string.Empty,
                Carrier = x.Carrier ?? string.Empty,
                Status = (int)x.Status,
                CodAmount = x.CodAmount,
                ShippingCost = x.ShippingCost,
                CreatedAt = x.CreatedAt,
                ExpectedAt = x.ExpectedAt,
                DaysInTransit = (int)(now - x.CreatedAt).TotalDays,
                IsStuck = (now - x.CreatedAt).TotalHours > 48
            }).ToList();

            return result;
        }
    }
}
