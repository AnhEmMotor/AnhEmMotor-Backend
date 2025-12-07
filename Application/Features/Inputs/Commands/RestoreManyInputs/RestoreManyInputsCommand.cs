using Application.ApiContracts.Input.Responses;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreManyInputs;

public sealed record RestoreManyInputsCommand : IRequest<(List<InputResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public ICollection<int> Ids { get; init; } = [];
}
