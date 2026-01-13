using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreManyOutputs;

public sealed record RestoreManyOutputsCommand : IRequest<Result<List<OutputResponse>?>>
{
    public ICollection<int> Ids { get; init; } = [];
}
