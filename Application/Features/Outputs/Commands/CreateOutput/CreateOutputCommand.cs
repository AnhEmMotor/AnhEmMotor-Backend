using Application.ApiContracts.Output.Responses;

using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed record CreateOutputCommand : IRequest<(OutputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public Guid? CurrentUserId { get; init; }

    public ICollection<OutputInfoResponse> OutputInfos { get; init; } = [];
}
