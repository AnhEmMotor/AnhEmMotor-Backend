using Application.Common.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Primitives;
using SieveModel = global::Sieve.Models.SieveModel;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories
{
    public interface IVehicleTypeRepository
    {
        public Task<bool> ExistsByNameAsync(
            string name,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<bool> ExistsByNameExceptIdAsync(
            string name,
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<List<VehicleType>> GetAllAsync(
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<VehicleType?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken,
            DataFetchMode mode = DataFetchMode.ActiveOnly);

        public Task<PagedResult<TResponse>> GetPagedAsync<TResponse>(
            SieveModel sieveModel,
            DataFetchMode mode = DataFetchMode.ActiveOnly,
            Expression<Func<VehicleType, bool>>? filter = null,
            CancellationToken cancellationToken = default);

        public void Add(VehicleType vehicleType);

        public void Update(VehicleType vehicleType);

        public void Remove(VehicleType vehicleType);
    }
}
