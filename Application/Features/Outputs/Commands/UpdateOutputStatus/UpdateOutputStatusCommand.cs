using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public sealed record UpdateOutputStatusCommand : IRequest<Result<OutputResponse?>>
{
    public int Id { get; init; }

    public string StatusId { get; init; } = string.Empty;

    public Guid? CurrentUserId { get; init; }
}
