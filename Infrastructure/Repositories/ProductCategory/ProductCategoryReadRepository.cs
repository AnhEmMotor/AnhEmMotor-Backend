using Application.ApiContracts.ProductCategory.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ProductCategory;
using Domain.Entities;
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
    private static string GetCurrentLanguage()
    {
        var culture = CultureInfo.CurrentCulture.Name;
        return culture.StartsWith("vi", StringComparison.OrdinalIgnoreCase) ? "vi" : "en";
    }

    private string BuildLocalizedNameSelect(string lang)
    {
        return lang == "vi" ? "t.Name" : "COALESCE(tEn.Name, tVi.Name)";
    }

    private string BuildLocalizedDescSelect(string lang)
    {
        return lang == "vi" ? "t.Description" : "COALESCE(tEn.Description, tVi.Description)";
    }

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
        var lang = GetCurrentLanguage();
        var nameField = lang == "vi" ? "tVi.Name" : "COALESCE(tEn.Name, tVi.Name)";
        var descField = lang == "vi" ? "tVi.Description" : "COALESCE(tEn.Description, tVi.Description)";

        PagedResult<ProductCategoryResponse> result;

        var allQuery = from c in context.GetQuery<CategoryEntity>(DataFetchMode.ActiveOnly)
                        join tvi in context.ProductCategoryTranslations
                            .Where(tr => tr.LanguageCode == "vi" && tr.DeletedAt == null)
                            on c.Id equals tvi.ProductCategoryId into tviGroup
                        from tVi in tviGroup.DefaultIfEmpty()
                        join tEn in context.ProductCategoryTranslations
                            .Where(tr => tr.LanguageCode == "en" && tr.DeletedAt == null)
                            on c.Id equals tEn.ProductCategoryId into tEnGroup
                        from tEn in tEnGroup.DefaultIfEmpty()
                        select new { c, tVi, tEn };

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var allItems = await allQuery
                   .Select(x => new
                   {
                       x.c,
                       Name = EF.Property<string>(x.tVi, "Name") ?? EF.Property<string>(x.tEn, "Name") ?? x.c.Name,
                       DescVi = EF.Property<string>(x.tVi, "Description"),
                       DescEn = EF.Property<string>(x.tEn, "Description")
                   })
                   .ToListAsync(cancellationToken)
                   .ConfigureAwait(false);

            var matched = allItems
                .Where(x => RemoveDiacritics(x.Name ?? string.Empty)
                    .Contains(RemoveDiacritics(searchKeyword), StringComparison.OrdinalIgnoreCase))
                .ToList();

            var resultIds = new HashSet<int>();
            foreach (var item in matched)
            {
                resultIds.Add(item.c.Id);
                var parent = item.c;
                while (parent.ParentId.HasValue)
                {
                    var parentId = parent.ParentId.Value;
                    if (!resultIds.Add(parentId)) break;
                    parent = allItems.FirstOrDefault(x => x.c.Id == parentId)?.c;
                    if (parent == null) break;
                }
                var children = matched.Where(x => x.c.ParentId == item.c.Id).Select(x => x.c);
                foreach (var child in children) resultIds.Add(child.Id);
            }
            var finalItems = allItems.Where(x => resultIds.Contains(x.c.Id)).ToList();
            var totalCount = finalItems.Count;
            
            var entityItems = finalItems.Select(x => x.c).AsQueryable();
            var pagedQuery = sieveProcessor.Apply(sieveModel, entityItems, applyFiltering: false);
            var paginated = pagedQuery.ToList();
            
            var responseItems = paginated.Select(c => {
                var response = c.Adapt<ProductCategoryResponse>();
                var info = finalItems.First(x => x.c.Id == c.Id);
                response.Name = info.Name ?? c.Name;
                response.Description = !string.IsNullOrWhiteSpace(info.DescVi) ? info.DescVi : info.DescEn;
                return response;
            }).ToList();
            result = new PagedResult<ProductCategoryResponse>(responseItems, totalCount, sieveModel.Page ?? 1, sieveModel.PageSize ?? 10);
        }
        else
        {
            var pagedSieveQuery = sieveProcessor.Apply(sieveModel, context.GetQuery<CategoryEntity>(DataFetchMode.ActiveOnly));
            
            var totalCountAll = await sieveProcessor.Apply(sieveModel, context.GetQuery<CategoryEntity>(DataFetchMode.ActiveOnly), applyPagination: false).CountAsync(cancellationToken).ConfigureAwait(false);

            var pagedEntities = await pagedSieveQuery.ToListAsync(cancellationToken).ConfigureAwait(false);
            var entityIds = pagedEntities.Select(e => e.Id).ToList();

            var translations = await context.ProductCategoryTranslations
                .Where(t => entityIds.Contains(t.ProductCategoryId) && t.DeletedAt == null)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var responseItems = pagedEntities.Select(c =>
            {
                var response = c.Adapt<ProductCategoryResponse>();
                var tVi = translations.FirstOrDefault(t => t.ProductCategoryId == c.Id && t.LanguageCode == "vi");
                var tEn = translations.FirstOrDefault(t => t.ProductCategoryId == c.Id && t.LanguageCode == "en");

                var name = c.Name;
                var nameVi = tVi?.Name;
                var nameEn = tEn?.Name;
                
                if (!string.IsNullOrWhiteSpace(name))
                {
                    response.Name = name;
                }
                else
                {
                    response.Name = !string.IsNullOrWhiteSpace(nameVi) ? nameVi : nameEn;
                }

                response.Description = !string.IsNullOrWhiteSpace(tVi?.Description) ? tVi.Description : tEn?.Description;
                return response;
            }).ToList();

            result = new PagedResult<ProductCategoryResponse>(responseItems, totalCountAll, sieveModel.Page ?? 1, sieveModel.PageSize ?? 10);
        }

        if (result.Items != null)
        {
            await PopulateInventoryQtyAsync(result.Items, cancellationToken).ConfigureAwait(false);
        }
        return result;
    }

    private static void SetLocalizedNames(
        List<ProductCategoryResponse> responses,
        IEnumerable<object> localizedData)
    {
        var dict = localizedData
            .Select((x, i) => (Response: responses.ElementAtOrDefault(i), Data: x))
            .Where(x => x.Response != null)
            .ToList();

        foreach (var (response, data) in dict)
        {
            var nameProp = data.GetType().GetProperty("Name");
            var nameViProp = data.GetType().GetProperty("NameVi");
            var nameEnProp = data.GetType().GetProperty("NameEn");
            var descViProp = data.GetType().GetProperty("DescVi");
            var descEnProp = data.GetType().GetProperty("DescEn");

            var name = nameProp?.GetValue(data) as string;
            var nameVi = nameViProp?.GetValue(data) as string;
            var nameEn = nameEnProp?.GetValue(data) as string;
            var descVi = descViProp?.GetValue(data) as string;
            var descEn = descEnProp?.GetValue(data) as string;

            if (!string.IsNullOrWhiteSpace(name))
            {
                response!.Name = name;
            }
            else
            {
                response!.Name = !string.IsNullOrWhiteSpace(nameVi) ? nameVi : (nameEn ?? string.Empty);
            }

            response!.Description = !string.IsNullOrWhiteSpace(descVi) ? descVi : (descEn ?? string.Empty);
        }
    }

    private async Task PopulateInventoryQtyAsync(
        List<ProductCategoryResponse> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0) return;
        var targetMonth = DateTimeOffset.UtcNow.Month;
        var targetYear = DateTimeOffset.UtcNow.Year;
        var categoryInventory = await context.InventoryOnHands
            .Where(
                x => x.Month == targetMonth &&
                    x.Year == targetYear &&
                    x.ProductVariant != null &&
                    x.ProductVariant.Product != null &&
                    x.ProductVariant.Product.CategoryId != null)
            .GroupBy(x => x.ProductVariant!.Product!.CategoryId)
            .Select(g => new { CategoryId = g.Key!.Value, TotalStock = g.Sum(x => x.StockQty) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.TotalStock, cancellationToken)
            .ConfigureAwait(false);
        foreach (var item in items)
        {
            if (item.Id.HasValue)
            {
                item.InventoryQty = categoryInventory.GetValueOrDefault(item.Id.Value, 0);
            }
        }
    }

    public Task<List<CategoryEntity>> GetAllAsync(
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).Include(c => c.Products).ToListAsync(cancellationToken);
    }

    public Task<CategoryEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<List<CategoryEntity>> GetByIdAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode)
            .Include(c => c.Products)
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(cancellationToken);
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
        return context.GetQuery<CategoryEntity>(mode)
            .Include(c => c.Products)
            .Where(x => x.ParentId == parentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AnyCategoryInTreeHasProductsAsync(
        int rootId,
        CancellationToken cancellationToken,
        DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        var hasProducts = await context.GetQuery<Domain.Entities.Product>(mode)
            .AnyAsync(p => p.CategoryId == rootId, cancellationToken)
            .ConfigureAwait(false);
        if (hasProducts) return true;
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
        if (rootIdList.Count == 0) return false;
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

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).AnyAsync(c => c.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameExceptIdAsync(string name, int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).AnyAsync(c => c.Name == name && c.Id != id, cancellationToken);
    }

    internal IQueryable<CategoryEntity> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly)
    {
        return context.GetQuery<CategoryEntity>(mode).Include(c => c.Products);
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
        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd')
            .Replace('Đ', 'D');
    }
}
