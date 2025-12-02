using Application.ApiContracts.Output;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed record UpdateOutputCommand : IRequest<(OutputResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }
    public string? StatusId { get; init; }
    public string? Notes { get; init; }
    public ICollection<OutputInfoDto> OutputInfos { get; init; } = [];
}
