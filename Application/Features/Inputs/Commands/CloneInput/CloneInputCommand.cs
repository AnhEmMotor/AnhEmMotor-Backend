using Application.ApiContracts.Input;
using Domain.Helpers;
using MediatR;

namespace Application.Features.Inputs.Commands.CloneInput;

public sealed record CloneInputCommand(int Id) : IRequest<(InputResponse? Data, ErrorResponse? Error)>;
