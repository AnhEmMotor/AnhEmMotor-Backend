using Application.ApiContracts.Output;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed record CreateOutputCommand : IRequest<(OutputResponse? Data, ErrorResponse? Error)>
{
    public string? StatusId { get; init; }
    public string? Notes { get; init; }
    public ICollection<OutputInfoDto> OutputInfos { get; init; } = [];
}
