using Application.ApiContracts.Output.Requests;
using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateManyOutputStatus;

public sealed record UpdateManyOutputStatusCommand(UpdateManyOutputStatusRequest Model) : IRequest<Result<List<OutputResponse>?>>;