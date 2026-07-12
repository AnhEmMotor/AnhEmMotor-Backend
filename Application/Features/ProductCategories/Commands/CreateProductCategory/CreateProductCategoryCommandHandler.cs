using Application.ApiContracts.ProductCategory.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using Domain.Entities;
using Mapster;
using MediatR;
using ProductCategoryEntity = Domain.Entities.ProductCategory;

namespace Application.Features.ProductCategories.Commands.CreateProductCategory;

public class CreateProductCategoryCommandHandler(
	IProductCategoryInsertRepository repository,
	IProductCategoryReadRepository readRepository,
	IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCategoryCommand, Result<ProductCategoryResponse>>
{
	public async Task<Result<ProductCategoryResponse>> Handle(
		CreateProductCategoryCommand request,
		CancellationToken cancellationToken)
	{
		var nameVi = request.NameVi.Trim();
		var isDuplicated = await readRepository.ExistsByNameExceptIdAsync(
			nameVi,
			0,
			cancellationToken,
			DataFetchMode.All)
			.ConfigureAwait(false);
		if (isDuplicated)
		{
			return Result<ProductCategoryResponse>.Failure(
				Error.Conflict($"Category name '{nameVi}' already exists."));
		}

		if (request.ParentId.HasValue)
		{
			var parent = await readRepository.GetByIdAsync(request.ParentId.Value, cancellationToken)
				.ConfigureAwait(false);
			if (parent == null)
			{
				return Result<ProductCategoryResponse>.Failure(
					Error.NotFound($"Parent category with Id {request.ParentId.Value} not found."));
			}
			if (parent.ParentId.HasValue)
			{
				return Result<ProductCategoryResponse>.Failure(
					Error.Validation("Cannot create a category at this level. Only 2 levels are allowed (Parent and Child)."));
			}
		}

		var category = new ProductCategoryEntity
		{
			Name = nameVi ?? string.Empty,
			Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
			IsActive = request.IsActive,
			ParentId = request.ParentId,
			ManagementType = request.ManagementType ?? string.Empty,
			MaxPurchaseQuantity = request.MaxPurchaseQuantity,
			Slug = GenerateSlug(nameVi ?? string.Empty),
		};

		repository.Add(category);
		await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

		category.Translations = new List<ProductCategoryTranslation>
		{
			new()
			{
				LanguageCode = "vi",
				Name = nameVi ?? string.Empty,
				Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
				CreatedAt = DateTimeOffset.UtcNow,
			},
			new()
			{
				LanguageCode = "en",
				Name = request.NameEn.Trim(),
				Description = string.IsNullOrWhiteSpace(request.DescriptionEn) ? null : request.DescriptionEn.Trim(),
				CreatedAt = DateTimeOffset.UtcNow,
			}
		};

		await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

		return category.Adapt<ProductCategoryResponse>();
	}

	private static string GenerateSlug(string name)
	{
		var vi = new System.Globalization.CultureInfo("vi-VN");
		var normalized = name.Normalize(System.Text.NormalizationForm.FormD);
		var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark);
		var slug = new string(chars.ToArray()).Normalize(System.Text.NormalizationForm.FormC).ToLower(vi);
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[\s-]+", "-").Trim('-');
		return slug;
	}
}
