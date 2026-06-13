using Application.ApiContracts.Return.Responses;
using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using Domain.Entities.Logistics;
using Domain.Enums;
using MediatR;
using System;

namespace Application.Features.Logistics.Returns.Queries.GetReturnDetail
{
    public class GetReturnDetailQueryHandler(IParcelDeliveryOrderReadRepository parcelDeliveryOrderReadRepository) : IRequestHandler<GetReturnDetailQuery, ReturnDetailResponse?>
    {
        public async Task<ReturnDetailResponse?> Handle(
            GetReturnDetailQuery request,
            CancellationToken cancellationToken)
        {
            var order = await parcelDeliveryOrderReadRepository.GetByIdAsync(request.Id, cancellationToken)
                .ConfigureAwait(false);
            if (order == null || order.Status != ParcelDeliveryStatus.Returned)
                return null;
            return new ReturnDetailResponse
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
                Items =
                    [.. order.Items
                        .Select(
                            i => new ReturnDetailItemResponse
                        {
                            Id = i.Id,
                            ProductId = i.ProductId,
                            ProductName = i.ProductName,
                            Sku = i.Sku,
                            ThumbnailUrl = i.ThumbnailUrl,
                            Quantity = i.Quantity
                        })]
            };
        }

        private static string GetReturnStatus(ParcelDeliveryOrder order)
        {
            if (order.ReturnAction != null)
                return "completed";
            if (order.InspectedAt != null)
                return "inspecting";
            return "pending";
        }
    }
}
