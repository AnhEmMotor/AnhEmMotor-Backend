
using Application.Common.Models;
using MediatR;

namespace Application.Features.Inputs.Commands.DeleteInput;

public sealed record DeleteInputCommand : IRequest<Result>
{
    public int? Id { get; init; }
}