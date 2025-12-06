using Application.ApiContracts.Input.Responses;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.CloneInput;

public sealed record CloneInputCommand(int Id) : IRequest<(InputResponse? Data, ErrorResponse? Error)>;
