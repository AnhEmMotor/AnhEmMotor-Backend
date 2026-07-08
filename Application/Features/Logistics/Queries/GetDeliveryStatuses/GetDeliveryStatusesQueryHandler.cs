using Domain.Enums;
using MediatR;

namespace Application.Features.Logistics.Queries.GetDeliveryStatuses
{
    public class GetDeliveryStatusesQueryHandler : IRequestHandler<GetDeliveryStatusesQuery, List<DeliveryStatusResponse>>
    {
        public Task<List<DeliveryStatusResponse>> Handle(
            GetDeliveryStatusesQuery request,
            CancellationToken cancellationToken)
        {
            var statuses = new List<DeliveryStatusResponse>
            {
                new DeliveryStatusResponse
                {
                    Id = (int)ParcelDeliveryStatus.Shipping,
                    NameEn = "Shipping",
                    NameVi = "Đang giao hàng"
                },
                new DeliveryStatusResponse
                {
                    Id = (int)ParcelDeliveryStatus.Completed,
                    NameEn = "Completed",
                    NameVi = "Đã giao hàng xong"
                },
                new DeliveryStatusResponse
                {
                    Id = (int)ParcelDeliveryStatus.Returned,
                    NameEn = "Returned",
                    NameVi = "Đã hoàn trả"
                }
            };
            return Task.FromResult(statuses);
        }
    }
}
