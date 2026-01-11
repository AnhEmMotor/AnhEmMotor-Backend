using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Queries.GetInputById;

public sealed record GetInputByIdQuery(int Id) : IRequest<Result<InputResponse?>>;
