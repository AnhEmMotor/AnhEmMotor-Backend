using Application.ApiContracts.Output;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public sealed record UpdateOutputStatusCommand : IRequest<OutputResponse>
{
    public int Id { get; init; }
    public string NewStatusId { get; init; } = string.Empty;
}
