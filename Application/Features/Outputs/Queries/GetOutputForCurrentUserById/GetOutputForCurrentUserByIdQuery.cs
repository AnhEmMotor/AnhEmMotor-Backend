using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Queries.GetOutputForCurrentUserById;

public sealed record GetOutputForCurrentUserByIdQuery(int Id) : IRequest<Result<OrderDetailResponse>>;
