using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.CloneInput;

public sealed record CloneInputCommand(int Id) : IRequest<Result<InputResponse?>>;
