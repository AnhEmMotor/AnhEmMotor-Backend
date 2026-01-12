using Application.ApiContracts.Output.Requests;
using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutputForManager;

public sealed record UpdateOutputForManagerCommand(UpdateOutputForManagerRequest Model) : IRequest<Result<OutputResponse?>>;