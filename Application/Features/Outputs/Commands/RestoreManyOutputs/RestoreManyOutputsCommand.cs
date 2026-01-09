using Application.ApiContracts.Output.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreManyOutputs;

public sealed record RestoreManyOutputsCommand : IRequest<(List<OutputResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public ICollection<int> Ids { get; init; } = [];
}
