using Application.ApiContracts.Output.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed record UpdateOutputCommand : IRequest<(OutputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }

    public Guid? CurrentUserId { get; init; }

    public string? StatusId { get; init; }

    public string? Notes { get; init; }

    public ICollection<OutputInfoResponse> OutputInfos { get; init; } = [];
}
