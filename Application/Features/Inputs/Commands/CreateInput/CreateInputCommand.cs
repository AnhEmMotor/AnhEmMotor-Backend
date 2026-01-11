using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.CreateInput;

public sealed record CreateInputCommand : IRequest<Result<InputResponse?>>
{
    public string? Notes { get; init; }

    public string? StatusId { get; init; }

    public int? SupplierId { get; init; }

    public List<CreateInputProductCommand> Products { get; init; } = [];
}
