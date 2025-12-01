using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreManyInputs;

public sealed record RestoreManyInputsCommand : IRequest<ErrorResponse?>
{
    public ICollection<int> Ids { get; init; } = [];
}
