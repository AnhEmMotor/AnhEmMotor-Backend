using MediatR;

namespace Application.Features.Logistics.Queries.GetDeliveryStatuses
{
    public class GetDeliveryStatusesQuery : IRequest<List<DeliveryStatusResponse>>
    {
    }

    public class DeliveryStatusResponse
    {
        public int Id { get; set; }

        public string NameEn { get; set; } = string.Empty;

        public string NameVi { get; set; } = string.Empty;
    }
}
