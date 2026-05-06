using Domain.Constants;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IVehicleTypeRepository
    {
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly);
        Task<bool> ExistsByNameExceptIdAsync(string name, int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly);
        Task<List<VehicleType>> GetAllAsync(CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly);
        Task<VehicleType?> GetByIdAsync(int id, CancellationToken cancellationToken, DataFetchMode mode = DataFetchMode.ActiveOnly);
        IQueryable<VehicleType> GetQueryable(DataFetchMode mode = DataFetchMode.ActiveOnly);
        void Add(VehicleType vehicleType);
        void Update(VehicleType vehicleType);
        void Remove(VehicleType vehicleType);
    }
}
