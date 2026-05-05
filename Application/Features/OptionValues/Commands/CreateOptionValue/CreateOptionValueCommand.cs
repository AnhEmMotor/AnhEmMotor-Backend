using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Option;
using Application.Interfaces.Repositories.OptionValue;
using MediatR;
using OptionEntity = Domain.Entities.Option;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Features.OptionValues.Commands.CreateOptionValue;

public record CreateOptionValueCommand : IRequest<Result<int>>
{
    public int OptionId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? ColorCode { get; init; }
}


