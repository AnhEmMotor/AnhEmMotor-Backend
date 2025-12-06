using Application.ApiContracts.Input.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputById;

public sealed record GetInputByIdQuery(int Id) : IRequest<(InputResponse? Data, ErrorResponse? Error)>;
