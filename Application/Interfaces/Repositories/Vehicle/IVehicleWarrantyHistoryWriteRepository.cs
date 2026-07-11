using Domain.Entities;

namespace Application.Interfaces.Repositories.Vehicle;

public interface IVehicleWarrantyHistoryWriteRepository
{
    void Add(VehicleWarrantyHistory entity);
}
