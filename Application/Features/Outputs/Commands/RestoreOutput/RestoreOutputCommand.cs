using MediatR;

namespace Application.Features.Outputs.Commands.RestoreOutput;

public sealed record RestoreOutputCommand(int Id) : IRequest<Unit>;
