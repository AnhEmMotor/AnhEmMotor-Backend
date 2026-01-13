using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreInput;

public sealed record RestoreInputCommand : IRequest<Result<InputResponse>>
{
    public int? Id { get; init; }
}
