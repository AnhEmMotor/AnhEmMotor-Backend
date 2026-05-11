using VehicleTypeEntity = Domain.Entities.VehicleType;

namespace Application.Interfaces.Repositories.VehicleType.VehicleType;

public interface IVehicleTypeInsertRepository
{
    public void Add(Domain.Entities.VehicleType VehicleTypeEntity);
}
