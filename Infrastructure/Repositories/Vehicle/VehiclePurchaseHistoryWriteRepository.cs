using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Vehicle;

public class VehiclePurchaseHistoryWriteRepository(ApplicationDBContext context) : IVehiclePurchaseHistoryWriteRepository
{
    public void Add(VehiclePurchaseHistory entity)
    {
        context.VehiclePurchaseHistories.Add(entity);
    }
}
