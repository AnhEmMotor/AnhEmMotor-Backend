using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories.NewsCategory
{
    public interface INewsCategoryReadRepository
    {
        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.NewsCategory, bool>>? filter = null,
            CancellationToken cancellationToken = default);

        public Task<Domain.Entities.NewsCategory?> GetByIdAsync(int id, CancellationToken cancellationToken);

        public Task<Domain.Entities.NewsCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    }
}

