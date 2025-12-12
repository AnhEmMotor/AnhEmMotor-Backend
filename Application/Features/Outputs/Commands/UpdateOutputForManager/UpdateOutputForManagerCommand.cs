using Application.ApiContracts.Output.Responses;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed record UpdateOutputForManagerCommand : IRequest<(OutputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public ICollection<OutputInfoResponse> OutputInfos { get; init; } = [];
}
