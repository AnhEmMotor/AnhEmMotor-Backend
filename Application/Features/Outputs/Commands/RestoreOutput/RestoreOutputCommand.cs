using Application.ApiContracts.Output.Responses;
using MediatR;

namespace Application.Features.Outputs.Commands.RestoreOutput;

public sealed record RestoreOutputCommand(int Id) : IRequest<(OutputResponse? Data, Common.Models.ErrorResponse? Error)>;
