using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputForManager;

public sealed record UpdateOutputForManagerCommand : IRequest<Result<OutputResponse?>>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public ICollection<OutputInfoResponse> OutputInfos { get; init; } = [];
}
