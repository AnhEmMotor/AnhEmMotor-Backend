using Application.Common.Models;
using MediatR;

namespace Application.Features.Maintenances.Commands.UpdateOdo
{
    public class UpdateOdoCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }

        public double CurrentOdo { get; set; }
    }
}
