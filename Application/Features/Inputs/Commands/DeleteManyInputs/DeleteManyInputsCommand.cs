using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteManyInputs;

public sealed record DeleteManyInputsCommand : IRequest<ErrorResponse?>
{
    public ICollection<int> Ids { get; init; } = [];
}
