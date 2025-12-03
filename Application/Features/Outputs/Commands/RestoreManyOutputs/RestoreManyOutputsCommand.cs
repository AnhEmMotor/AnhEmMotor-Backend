using Application.ApiContracts.Output;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreManyOutputs;

public sealed record RestoreManyOutputsCommand : IRequest<(List<OutputResponse>? Data, ErrorResponse? Error)>
{
    public ICollection<int> Ids { get; init; } = [];
}
