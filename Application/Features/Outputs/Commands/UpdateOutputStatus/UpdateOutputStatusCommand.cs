using Application.ApiContracts.Output.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public sealed record UpdateOutputStatusCommand : IRequest<(OutputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string StatusId { get; init; } = string.Empty;

    public Guid? CurrentUserId { get; init; }
}
