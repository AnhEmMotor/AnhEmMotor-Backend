using Application.ApiContracts.Output.Requests;
using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.CreateOutput;

public sealed record CreateOutputCommand(CreateOutputRequest Model) : IRequest<Result<OutputResponse?>>;