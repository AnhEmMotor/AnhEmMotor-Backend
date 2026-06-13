using Application.ApiContracts.ServiceCategory.Responses;
using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using ServiceCategoryEntity = Domain.Entities.ServiceCategory;

namespace Application.Interfaces.Repositories.ServiceCategory
{
    public interface IServiceCategoryReadRepository
    {
public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default);

public Task<List<ServiceCategoryEntity>> GetFilteredListAsync(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            CancellationToken cancellationToken = default);

public Task<IEnumerable<ServiceCategoryEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

public Task<ServiceCategoryEntity?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

public Task<IEnumerable<ServiceCategoryEntity>> GetByIdAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

public Task<ServiceCategoryEntity?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

public Task<IEnumerable<ServiceCategoryEntity>> GetRootCategoriesAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

public Task<IEnumerable<ServiceCategoryEntity>> GetSubCategoriesAsync(
            int parentId,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);
    }
}

