using Application.ApiContracts.Output;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed record UpdateOutputCommand : IRequest<OutputResponse>
{
    public int Id { get; init; }
    public string? StatusId { get; init; }
    public string? Notes { get; init; }
    public ICollection<OutputInfoDto> Products { get; init; } = [];
}
