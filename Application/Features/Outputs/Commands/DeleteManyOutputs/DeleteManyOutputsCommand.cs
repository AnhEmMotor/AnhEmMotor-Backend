using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.DeleteManyOutputs;

public sealed record DeleteManyOutputsCommand : IRequest<ErrorResponse?>
{
    public ICollection<int> Ids { get; init; } = [];
}
