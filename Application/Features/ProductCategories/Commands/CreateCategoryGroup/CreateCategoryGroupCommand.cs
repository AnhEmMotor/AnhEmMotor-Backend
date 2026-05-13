using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.ProductCategories.Commands.CreateCategoryGroup;

public sealed record CreateCategoryGroupCommand : IRequest<Result<string>>
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("category_ids")]
    public List<int> CategoryIds { get; init; } = [];
}

