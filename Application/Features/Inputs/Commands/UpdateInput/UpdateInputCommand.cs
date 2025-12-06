using Application.ApiContracts.Input.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInput;

public sealed record UpdateInputCommand : IRequest<(InputResponse? Data, ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }

    public int? SupplierId { get; init; }

    public string? Notes { get; init; }

    public ICollection<InputInfoResponse> Products { get; init; } = [];
}
