using Application.Interfaces.Repositories.VehicleType.VehicleType;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.VehicleType.VehicleType;

public class VehicleTypeInsertRepository(ApplicationDBContext context) : IVehicleTypeInsertRepository
{
    public void Add(Domain.Entities.VehicleType vehicleType)
    {
        context.VehicleTypes.Add(vehicleType);
    }
}
