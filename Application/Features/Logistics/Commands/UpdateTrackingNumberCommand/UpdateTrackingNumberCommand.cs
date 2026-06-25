using MediatR;

namespace Application.Features.Logistics.Commands.UpdateTrackingNumberCommand;

public class UpdateTrackingNumberCommand : IRequest<bool>
{
    public int Id { get; set; }

    public string TrackingNumber { get; set; } = string.Empty;
}
