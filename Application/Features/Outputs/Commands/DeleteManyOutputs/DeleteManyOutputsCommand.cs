using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.DeleteManyOutputs;

public sealed record DeleteManyOutputsCommand : IRequest<Result>
{
    public ICollection<int> Ids { get; init; } = [];
}
