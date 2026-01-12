
using Application.Common.Models;
using MediatR;

namespace Application.Features.Outputs.Commands.DeleteOutput;

public sealed record DeleteOutputCommand : IRequest<Result>
{
    public int Id { get; init; }
}
