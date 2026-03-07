using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreOutput;

public sealed record RestoreOutputCommand : IRequest<Result<OrderDetailResponse>>
{
    public int Id { get; init; }
}
