using Application.ApiContracts.Output.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputById;

public sealed record GetOutputByIdQuery(int Id) : IRequest<(OutputResponse? Data, ErrorResponse? Error)>;
