using Application.Interfaces.Repositories.VehicleType.VehicleType;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.VehicleType.VehicleType;

public class VehicleTypeDeleteRepository(ApplicationDBContext context) : IVehicleTypeDeleteRepository
{
    public void Remove(Domain.Entities.VehicleType vehicleType)
    {
        context.VehicleTypes.Remove(vehicleType);
    }
}
