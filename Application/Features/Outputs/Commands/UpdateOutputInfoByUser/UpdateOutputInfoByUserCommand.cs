using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputInfoByUser;

public sealed record UpdateOutputInfoByUserCommand : IRequest<Result<OrderDetailResponse>>
{
    public int Id { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerPhone { get; init; }
    public string? CustomerAddress { get; init; }
    public string? Notes { get; init; }
    public Guid? CurrentUserId { get; set; }
}
