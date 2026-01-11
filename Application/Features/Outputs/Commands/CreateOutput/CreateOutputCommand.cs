using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed record CreateOutputCommand : IRequest<Result<OutputResponse?>>
{
    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public Guid? CurrentUserId { get; init; }

    public ICollection<OutputInfoResponse> OutputInfos { get; init; } = [];
}
