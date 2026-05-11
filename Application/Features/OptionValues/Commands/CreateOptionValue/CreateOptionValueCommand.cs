using Application.Common.Models;
using MediatR;

namespace Application.Features.OptionValues.Commands.CreateOptionValue;

public record CreateOptionValueCommand : IRequest<Result<int>>
{
    public int OptionId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? ImageUrl { get; init; }

    public string? SeoTitle { get; init; }

    public string? SeoDescription { get; init; }

    public bool IsActive { get; init; } = true;

    public string? ColorCode { get; init; }
}
