using Application.Common.Models;
using Domain.Primitives;
using Domain.Constants;
using Sieve.Models;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories.News
{
    public interface INewsReadRepository
    {
        public Task<Domain.Primitives.PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.News, bool>>? filter = null,
            CancellationToken cancellationToken = default);

        public Task<Domain.Entities.News?> GetByIdAsync(int id, CancellationToken cancellationToken);

        public Task<Domain.Entities.News?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    }
}

