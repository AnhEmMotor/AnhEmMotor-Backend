using Application.ApiContracts.Input.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreManyInputs;

public sealed record RestoreManyInputsCommand : IRequest<(List<InputResponse>? Data, ErrorResponse? Error)>
{
    public ICollection<int> Ids { get; init; } = [];
}
