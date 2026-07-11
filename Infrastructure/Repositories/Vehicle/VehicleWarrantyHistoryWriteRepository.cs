using Application.Interfaces.Repositories.Vehicle;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.Vehicle;

public class VehicleWarrantyHistoryWriteRepository(ApplicationDBContext context) : IVehicleWarrantyHistoryWriteRepository
{
    public void Add(VehicleWarrantyHistory entity)
    {
        context.VehicleWarrantyHistories.Add(entity);
    }
}
