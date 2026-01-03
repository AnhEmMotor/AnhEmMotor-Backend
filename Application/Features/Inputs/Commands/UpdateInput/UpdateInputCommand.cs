using Application.ApiContracts.Input.Responses;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInput;

public sealed record UpdateInputCommand : IRequest<(InputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }

    public int? SupplierId { get; init; }

    public string? Notes { get; init; }

    public ICollection<UpdateInputProductCommand> Products { get; init; } = [];
}
