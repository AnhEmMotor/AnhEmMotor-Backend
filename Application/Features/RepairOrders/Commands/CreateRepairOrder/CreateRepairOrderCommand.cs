using Application.Common.Models;
using MediatR;

namespace Application.Features.RepairOrders.Commands.CreateRepairOrder
{
    public class CreateRepairOrderCommand : IRequest<Result<int>>
    {
        public int? VehicleId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public int Mileage { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
