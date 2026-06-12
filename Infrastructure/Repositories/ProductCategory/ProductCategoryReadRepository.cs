using Application.ApiContracts.ProductCategory.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using System.Globalization;
using System.Text;
using CategoryEntity = Domain.Entities.ProductCategory;

namespace Infrastructure.Repositories.ProductCategory;

public class ProductCategoryReadRepository(
    ApplicationDBContext context,
    ISievePaginator paginator,
    ISieveProcessor sieveProcessor) : IProductCategoryReadRepository
{
    public async Task<ProductCategoryStatsResponse> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var query = context.GetQuery<CategoryEntity>(DataFetchMode.ActiveOnly);
        var totalProductCategories = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var latestProductCategory = await query
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt ?? DateTimeOffset.MinValue)
            .ThenByDescending(c => c.Id)
            .Select(c => new { c.Name, LatestTime = c.UpdatedAt ?? c.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        return new ProductCategoryStatsResponse
        {
            TotalCategories = totalProductCategories,
            ProductCategoriesCount = totalProductCategories,
            LatestUpdatedCategoryName = latestProductCategory?.Name,
            LatestUpdatedAt = latestProductCategory?.LatestTime
        };
    }

    public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
        SieveModel sieveModel,
        DataFetchMode mode = DataFetchMode.ActiveOnly,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(mode);
        return paginator.ApplyAsync<CategoryEntity, TResponse>(query, sieveModel, mode, cancellationToken);
    }

    public async Task<PagedResult<ProductCategoryResponse>> GetPagedListAsync(
        SieveModel sieveModel,
        string? searchKeyword,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var allCategories = await GetAllAsync(cancellationToken).ConfigureAwait(false);
            var matchedCategories = allCategories.Where(
                c => RemoveDiacritics(c.Name ?? string.Empty)
                    .Contains(RemoveDiacritics(searchKeyword), StringComparison.OrdinalIgnoreCase))
                .ToList();
            var resultIds = new HashSet<int>();
            foreach (var cat in matchedCategories)
            {
                resultIds.Add(cat.Id);
                var parent = cat;
                while (parent.ParentId.HasValue)
                {
                    var parentId = parent.ParentId.Value;
                    if (!resultIds.Add(parentId))
                        break;
                    parent = allCategories.FirstOrDefault(c => c.Id == parentId);
                    if (parent == null)
                        break;
                }
                var children = allCategories.Where(c => c.ParentId == cat.Id);
                foreach (var child in children)
                {
                    resultIds.Add(child.Id);
                }
            }
            var finalCategories = allCategories.Where(c => resultIds.Contains(c.Id)).ToList();
            var query = finalCategories.AsQueryable();
            var totalCount = query.Count();
            var pagedQuery = sieveProcessor.Apply(sieveModel, query, applyFiltering: false);
            var paginatedCategories = pagedQuery.ToList();
            var responseItems = paginatedCategories.Select(c => c.Adapt<ProductCategoryResponse>()).ToList();
            return new PagedResult<ProductCategoryResponse>(
                responseItems,
                totalCount,
                sieveModel.Page ?? 1,
                sieveModel.PageSize ?? 10);
        }
        return await GetPagedAsync<ProductCategoryResponse>(sieveModel, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode)
            .AnyAsync(c => string.Compare(c.Name, name) == 0, cancellationToken);
    }

    public Task<bool> ExistsByNameExceptIdAsync(
        string name,
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode)
            .AnyAsync(x => string.Compare(x.Name, name) == 0 && x.Id != id, cancellationToken);
    }

    public Task<List<CategoryEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).ToListAsync(cancellationToken);
    }

    public Task<CategoryEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<List<CategoryEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).Where(c => ids.Contains(c.Id)).ToListAsync(cancellationToken);
    }

    public Task<bool> HasSubCategoriesAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).AnyAsync(x => x.ParentId == id, cancellationToken);
    }

    public Task<List<CategoryEntity>> GetSubCategoriesAsync(
        int parentId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).Where(x => x.ParentId == parentId).ToListAsync(cancellationToken);
    }

    public async Task<bool> AnyCategoryInTreeHasProductsAsync(
        int rootId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var hasProducts = await context.GetQuery<Domain.Entities.Product>(mode)
            .AnyAsync(p => p.CategoryId == rootId, cancellationToken)
            .ConfigureAwait(false);
        if (hasProducts)
            return true;
        var subCategoryIds = await context.GetQuery<CategoryEntity>(mode)
            .Where(c => c.ParentId == rootId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (subCategoryIds.Count != 0)
        {
            return await context.GetQuery<Domain.Entities.Product>(mode)
                .AnyAsync(p => p.CategoryId.HasValue && subCategoryIds.Contains(p.CategoryId.Value), cancellationToken)
                .ConfigureAwait(false);
        }
        return false;
    }

    public async Task<bool> AnyInTreeHasProductsAsync(
        IEnumerable<int> rootIds,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var rootIdList = rootIds.ToList();
        if (rootIdList.Count == 0)
            return false;
        var subCategoryIds = await context.GetQuery<CategoryEntity>(mode)
            .Where(c => c.ParentId.HasValue && rootIdList.Contains(c.ParentId.Value))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var allIds = rootIdList.Union(subCategoryIds).Distinct().ToList();
        return await context.GetQuery<Domain.Entities.Product>(mode)
            .AnyAsync(p => p.CategoryId.HasValue && allIds.Contains(p.CategoryId.Value), cancellationToken)
            .ConfigureAwait(false);
    }

    internal IQueryable<CategoryEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode);
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }
}
