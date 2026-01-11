using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputById;

public sealed record GetOutputByIdQuery(int Id) : IRequest<Result<OutputResponse?>>;
