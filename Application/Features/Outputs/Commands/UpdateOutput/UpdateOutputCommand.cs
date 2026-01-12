using Application.ApiContracts.Output.Requests;
using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.UpdateOutput;

public sealed record UpdateOutputCommand(UpdateOutputRequest Model) : IRequest<Result<OutputResponse?>>