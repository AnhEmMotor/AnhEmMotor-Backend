using Domain.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteInput;

public sealed record DeleteInputCommand(int Id) : IRequest<Common.Models.ErrorResponse?>;
