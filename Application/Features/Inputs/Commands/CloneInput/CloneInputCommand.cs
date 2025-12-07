using Application.ApiContracts.Input.Responses;
using MediatR;

namespace Application.Features.Inputs.Commands.CloneInput;

public sealed record CloneInputCommand(int Id) : IRequest<(InputResponse? Data, Common.Models.ErrorResponse? Error)>;
