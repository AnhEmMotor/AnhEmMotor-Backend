using Application.Interfaces.Repositories.Logistics.Shipment;
using Domain.Entities.Logistics;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.Logistics.Shipment;

public class ShipmentRepository : IShipmentInsertRepository, IShipmentUpdateRepository, IShipmentReadRepository
{
    private readonly ApplicationDBContext _context;

    public ShipmentRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Domain.Entities.Logistics.Shipment shipment, CancellationToken cancellationToken = default)
    {
        await _context.Shipments.AddAsync(shipment, cancellationToken);
    }

    public void Update(Domain.Entities.Logistics.Shipment shipment)
    {
        _context.Shipments.Update(shipment);
    }

    public async Task<Domain.Entities.Logistics.Shipment?> GetByOutputIdAsync(int outputId, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(s => s.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(pv => pv.Product)
            .Include(s => s.Items)
                .ThenInclude(i => i.ProductVariantColor)
            .FirstOrDefaultAsync(s => s.OutputId == outputId, cancellationToken);
    }

    public async Task<List<Domain.Entities.Logistics.Shipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(s => s.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(pv => pv.Product)
            .Include(s => s.Items)
                .ThenInclude(i => i.ProductVariantColor)
            .ToListAsync(cancellationToken);
    }
}
