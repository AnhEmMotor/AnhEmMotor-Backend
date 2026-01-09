using Domain.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteManyInputs;

public sealed record DeleteManyInputsCommand : IRequest<Common.Models.ErrorResponse?>
{
    public ICollection<int> Ids { get; init; } = [];
}
