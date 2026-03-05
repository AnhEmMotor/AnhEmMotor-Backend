using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreManyInputs;

public sealed record RestoreManyInputsCommand : IRequest<Result<List<InputDetailResponse>>>
{
    public ICollection<int> Ids { get; init; } = [];
}
