using Application.ApiContracts.Input.Responses;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputById;

public sealed record GetInputByIdQuery(int Id) : IRequest<(InputResponse? Data, Common.Models.ErrorResponse? Error)>;
