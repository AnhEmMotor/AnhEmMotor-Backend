using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.OptionValue;
using MediatR;

namespace Application.Features.OptionValues.Commands.UpdateOptionValue;

public record UpdateOptionValueCommand : IRequest<Result>
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? ColorCode { get; init; }
}


