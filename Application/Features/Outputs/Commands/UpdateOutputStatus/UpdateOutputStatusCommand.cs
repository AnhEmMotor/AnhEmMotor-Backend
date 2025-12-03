using Application.ApiContracts.Output;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputStatus;

public sealed record UpdateOutputStatusCommand : IRequest<(OutputResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string StatusId { get; init; } = string.Empty;
}
