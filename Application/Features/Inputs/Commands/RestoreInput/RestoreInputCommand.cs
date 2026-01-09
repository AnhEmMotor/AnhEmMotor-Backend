using Application.ApiContracts.Input.Responses;

using MediatR;

namespace Application.Features.Inputs.Commands.RestoreInput;

public sealed record RestoreInputCommand(int Id) : IRequest<(InputResponse? Data, Common.Models.ErrorResponse? Error)>;
