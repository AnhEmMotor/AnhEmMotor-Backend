using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.CloneInput;

public sealed record CloneInputCommand : IRequest<Result<InputResponse?>>
{
    public int? Id { get; init; }
}
