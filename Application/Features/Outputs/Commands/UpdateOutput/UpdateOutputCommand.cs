using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed record UpdateOutputCommand : IRequest<Result<OutputResponse?>>
{
    public int Id { get; init; }

    public Guid? CurrentUserId { get; init; }

    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public ICollection<OutputInfoResponse> OutputInfos { get; init; } = [];
}
