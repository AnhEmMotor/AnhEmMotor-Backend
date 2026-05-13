using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using MediatR;
using System.Text.Json.Serialization;

namespace Application.Features.ProductCategories.Commands.UpdateProductCategory;

public sealed record UpdateProductCategoryCommand : IRequest<Result<ProductCategoryResponse?>>
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("slug")]
    public string? Slug { get; init; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; }

    [JsonPropertyName("sort_order")]
    public int SortOrder { get; init; }

    [JsonPropertyName("parent_id")]
    public int? ParentId { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("category_group")]
    public string? CategoryGroup { get; init; }

    [JsonPropertyName("max_purchase_quantity")]
    public int? MaxPurchaseQuantity { get; init; }
}
