using Application.Common.Models;
using MediatR;

namespace Application.Features.OptionValues.Commands.CreateOptionValue;

public record CreateOptionValueCommand : IRequest<Result<int>>
{
    public int OptionId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? ColorCode { get; init; }
}

