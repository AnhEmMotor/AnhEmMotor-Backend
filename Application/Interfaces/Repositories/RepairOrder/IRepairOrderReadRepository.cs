using Domain.Constants;
using Domain.Primitives;
using Sieve.Models;
using System;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories.RepairOrder
{
    public interface IRepairOrderReadRepository
    {
        public Task<List<Domain.Entities.RepairOrder>> GetAllAsync(CancellationToken cancellationToken = default);

        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<Domain.Entities.RepairOrder, bool>>? filter = null,
            CancellationToken cancellationToken = default);

        public Task<Domain.Entities.RepairOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        public Task<List<Domain.Entities.RepairOrder>> GetByCustomerPhoneAsync(
            string phone,
            CancellationToken cancellationToken = default);
    }
}

