using VehicleTypeEntity = Domain.Entities.VehicleType;

namespace Application.Interfaces.Repositories.VehicleType.VehicleType;

public interface IVehicleTypeDeleteRepository
{
    public void Remove(Domain.Entities.VehicleType VehicleTypeEntity);
}
