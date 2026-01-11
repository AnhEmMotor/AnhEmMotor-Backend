using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreOutput;

public sealed record RestoreOutputCommand(int Id) : IRequest<Result<OutputResponse?>>;
