
namespace Application.Interfaces.Repositories.VehicleType.VehicleType;

public interface IVehicleTypeUpdateRepository
{
    public void Update(Domain.Entities.VehicleType VehicleTypeEntity);

    public void Restore(Domain.Entities.VehicleType VehicleTypeEntity);
}
