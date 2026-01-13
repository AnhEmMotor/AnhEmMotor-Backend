using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public sealed record UpdateManyOutputStatusCommand : IRequest<Result<List<OutputResponse>?>>
{
    public List<int>? Ids { get; init; }

    public string? StatusId { get; init; }
}
