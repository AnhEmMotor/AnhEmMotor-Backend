using Domain.Entities;

namespace Application.Interfaces.Repositories.Vehicle;

public interface IVehiclePurchaseHistoryWriteRepository
{
    void Add(VehiclePurchaseHistory entity);
}
