using MediatR;

namespace Application.Features.Logistics.Commands.ProcessFulfillment;

public class ToggleItemPickCommand : IRequest<bool>
{
    public int ItemId { get; set; }
    public bool IsPicked { get; set; }
}
