using Application.ApiContracts.Input.Responses;

using MediatR;

namespace Application.Features.Inputs.Commands.CreateInput;

public sealed record CreateInputCommand : IRequest<(InputResponse? Data, Common.Models.ErrorResponse? Error)>
{
    public string? Notes { get; init; }

    public string? StatusId { get; init; }

    public int? SupplierId { get; init; }

    public List<CreateInputProductCommand> Products { get; init; } = [];
}
