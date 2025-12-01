using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreManyOutputs;

public sealed record RestoreManyOutputsCommand : IRequest<ErrorResponse?>
{
    public ICollection<int> Ids { get; init; } = [];
}
