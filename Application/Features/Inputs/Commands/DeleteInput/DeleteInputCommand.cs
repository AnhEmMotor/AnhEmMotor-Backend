using MediatR;

namespace Application.Features.Inputs.Commands.DeleteInput;

public sealed record DeleteInputCommand(int Id) : IRequest<Unit>;
