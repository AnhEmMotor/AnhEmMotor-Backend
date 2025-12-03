using Application.ApiContracts.Output;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreOutput;

public sealed record RestoreOutputCommand(int Id) : IRequest<(OutputResponse? Data, ErrorResponse? Error)>;
