
using Application.ApiContracts.ProductCategory.Responses;

using Application.Common.Models;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed record CreateProductCategoryCommand : IRequest<Result<ProductCategoryResponse>>
{
    [Required(ErrorMessage = "Tên danh mục (Tiếng Việt) không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên danh mục (Tiếng Việt) không được vượt quá 255 ký tự.")]
    public string NameVi { get; init; } = string.Empty;

    [Required(ErrorMessage = "Tên danh mục (English) không được để trống.")]
    [MaxLength(255, ErrorMessage = "Tên danh mục (English) không được vượt quá 255 ký tự.")]
    public string NameEn { get; init; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Mô tả (Tiếng Việt) không được vượt quá 500 ký tự.")]
    public string? Description { get; init; }

    [MaxLength(500, ErrorMessage = "Mô tả (English) không được vượt quá 500 ký tự.")]
    public string? DescriptionEn { get; init; }

    public string? Slug { get; init; }

    public string? ImageUrl { get; init; }

    public bool IsActive { get; init; } = true;

    public int? ParentId { get; init; }

    public string? ManagementType { get; init; }

    public int? MaxPurchaseQuantity { get; init; }
}
