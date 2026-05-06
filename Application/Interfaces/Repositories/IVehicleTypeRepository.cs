using Domain.Constants;
using Domain.Entities;

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

        public IQueryable<VehicleType> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);

        public void Add(VehicleType vehicleType);

        public void Update(VehicleType vehicleType);

        public void Remove(VehicleType vehicleType);
    }
}
