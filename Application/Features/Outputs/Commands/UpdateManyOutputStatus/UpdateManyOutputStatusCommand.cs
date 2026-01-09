using Application.ApiContracts.Output.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public sealed record UpdateManyOutputStatusCommand : IRequest<(List<OutputResponse>? Data, Common.Models.ErrorResponse? Error)>
{
    public ICollection<int> Ids { get; init; } = [];

    public string StatusId { get; init; } = string.Empty;
}
