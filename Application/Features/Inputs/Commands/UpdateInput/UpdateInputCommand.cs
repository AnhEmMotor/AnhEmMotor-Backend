using Application.ApiContracts.Input;
using MediatR;

namespace Application.Features.Inputs.Commands.UpdateInput;

public sealed record UpdateInputCommand : IRequest<InputResponse>
{
    public int Id { get; init; }
    public string? StatusId { get; init; }
    public int? SupplierId { get; init; }
    public string? Notes { get; init; }
    public ICollection<InputInfoDto> Products { get; init; } = [];
}
