using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateManyInputStatus;

public sealed record UpdateManyInputStatusCommand : IRequest<Result<List<InputResponse>?>>
{
    public ICollection<int> Ids { get; init; } = [];

    public string StatusId { get; init; } = string.Empty;
}
