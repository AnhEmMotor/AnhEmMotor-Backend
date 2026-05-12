using Application.Interfaces.Repositories.VehicleType.VehicleType;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.VehicleType.VehicleType;

public class VehicleTypeUpdateRepository(ApplicationDBContext context) : IVehicleTypeUpdateRepository
{
    public void Update(Domain.Entities.VehicleType vehicleType)
    {
        context.VehicleTypes.Update(vehicleType);
    }

    public void Restore(Domain.Entities.VehicleType vehicleType)
    {
        vehicleType.DeletedAt = null;
        context.VehicleTypes.Update(vehicleType);
    }
}
