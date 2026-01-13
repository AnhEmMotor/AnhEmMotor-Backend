using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputById;

public sealed record GetInputByIdQuery : IRequest<Result<InputResponse?>>
{
    public int Id { get; init; }
}
