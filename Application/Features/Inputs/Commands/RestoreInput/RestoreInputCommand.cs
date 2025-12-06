using Application.ApiContracts.Input.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.RestoreInput;

public sealed record RestoreInputCommand(int Id) : IRequest<(InputResponse? Data, ErrorResponse? Error)>;
