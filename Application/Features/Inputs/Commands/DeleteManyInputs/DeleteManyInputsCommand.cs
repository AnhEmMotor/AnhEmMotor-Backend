
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteManyInputs;

public sealed record DeleteManyInputsCommand : IRequest<Result>
{
    public ICollection<int> Ids { get; init; } = [];
}
