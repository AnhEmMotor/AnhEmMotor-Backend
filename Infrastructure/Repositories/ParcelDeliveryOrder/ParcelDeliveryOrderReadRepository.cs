using Application.Interfaces.Repositories.ParcelDeliveryOrder;
using ParcelDeliveryOrderEntity = Domain.Entities.Logistics.ParcelDeliveryOrder;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;

namespace Infrastructure.Repositories.ParcelDeliveryOrder;

public class ParcelDeliveryOrderReadRepository(ApplicationDBContext context) : IParcelDeliveryOrderReadRepository
{
    public Task<List<ParcelDeliveryOrderEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => context.Set<ParcelDeliveryOrderEntity>().Include(x => x.Items).ToListAsync(cancellationToken);

    public Task<List<ParcelDeliveryOrderEntity>> GetReturnedAsync(CancellationToken cancellationToken = default)
        => context.Set<ParcelDeliveryOrderEntity>()
            .Where(x => x.Status == ParcelDeliveryStatus.Returned)
            .Include(x => x.Items)
            .ToListAsync(cancellationToken);

    public Task<ParcelDeliveryOrderEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => context.Set<ParcelDeliveryOrderEntity>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<ParcelDeliveryOrderEntity?> FindByTrackingOrPhoneAsync(string? search, CancellationToken cancellationToken = default)
    {
        var normalized = search?.Trim();
        return context.Set<ParcelDeliveryOrderEntity>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x =>
                string.Compare(x.TrackingNumber, normalized) == 0 ||
                string.Compare(x.CustomerPhone, normalized) ==0 ||
                string.Compare(x.OriginalOrderCode, normalized) == 0, cancellationToken);
    }
}
