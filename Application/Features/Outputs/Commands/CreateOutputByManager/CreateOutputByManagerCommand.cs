using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutputByManager;

public sealed record CreateOutputByManagerCommand : IRequest<Result<OutputResponse?>>
{
    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public Guid? BuyerId { get; init; }

    public ICollection<OutputInfoResponse> OutputInfos { get; init; } = [];
}
