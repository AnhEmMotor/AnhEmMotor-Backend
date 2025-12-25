using Application.ApiContracts.Output.Responses;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutputByManager;

public sealed record CreateOutputByManagerCommand : IRequest<(OutputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public Guid? BuyerId { get; init; }

    public ICollection<OutputInfoResponse> OutputInfos { get; init; } = [];
}
