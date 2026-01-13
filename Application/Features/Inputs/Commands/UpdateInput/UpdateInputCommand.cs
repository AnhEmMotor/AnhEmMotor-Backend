using Application.ApiContracts.Input.Requests;
using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInput;

public sealed record UpdateInputCommand : IRequest<Result<InputResponse?>>
{
    public int Id { get; init; }

    public string? StatusId { get; init; }

    public int? SupplierId { get; init; }

    public string? Notes { get; init; }

    public ICollection<UpdateInputInfoRequest> Products { get; init; } = [];
}
