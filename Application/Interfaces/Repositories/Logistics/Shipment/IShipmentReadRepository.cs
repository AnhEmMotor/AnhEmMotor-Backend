using Domain.Entities.Logistics;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.Logistics.Shipment;

public interface IShipmentReadRepository
{
    Task<Domain.Entities.Logistics.Shipment?> GetByOutputIdAsync(int outputId, CancellationToken cancellationToken = default);
    Task<System.Collections.Generic.List<Domain.Entities.Logistics.Shipment>> GetAllAsync(CancellationToken cancellationToken = default);
}
