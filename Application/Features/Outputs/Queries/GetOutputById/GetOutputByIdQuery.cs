using Application.ApiContracts.Output.Responses;
using Domain.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputById;

public sealed record GetOutputByIdQuery(int Id) : IRequest<(OutputResponse? Data, Common.Models.ErrorResponse? Error)>;
