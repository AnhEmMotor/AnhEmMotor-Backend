using Domain.Enums;
using MediatR;

namespace Application.Features.Logistics.Commands.UpdateParcelStatusCommand;

public class UpdateParcelStatusCommand : IRequest<bool>
{
    public int Id { get; set; }

    public ParcelDeliveryStatus NewStatus { get; set; }
}
