using Application.Common.Models;
using MediatR;

namespace Application.Features.Maintenances.Commands.UpdateOdo
{
    public class UpdateOdoCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; } // Vehicle ID
        public double CurrentOdo { get; set; }
    }
}
