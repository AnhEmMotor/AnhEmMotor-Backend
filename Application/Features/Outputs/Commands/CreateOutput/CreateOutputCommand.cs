using Application.ApiContracts.Output;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed record CreateOutputCommand : IRequest<OutputResponse>
{
    public string? StatusId { get; init; }
    public string? Notes { get; init; }
    public ICollection<OutputInfoDto> Products { get; init; } = [];
}
