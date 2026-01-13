using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputById;

public sealed record GetOutputByIdQuery : IRequest<Result<OutputResponse?>>
{
    public int Id { get; init; }
}
