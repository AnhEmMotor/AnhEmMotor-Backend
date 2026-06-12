using Application.Common.Models;
using MediatR;

namespace Application.Features.RepairOrders.Commands.AssignTechnician
{
    public class AssignTechnicianCommand : IRequest<Result<bool>>
    {
        public int RepairOrderId { get; set; }
        public int TechnicianId { get; set; }
    }
}
