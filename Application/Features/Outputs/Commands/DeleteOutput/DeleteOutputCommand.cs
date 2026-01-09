
using MediatR;

namespace Application.Features.Outputs.Commands.DeleteOutput;

public sealed record DeleteOutputCommand(int Id) : IRequest<Common.Models.ErrorResponse?>;
