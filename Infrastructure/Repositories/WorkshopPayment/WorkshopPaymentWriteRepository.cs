using Application.Interfaces.Repositories.WorkshopPayment;
using Infrastructure.DBContexts;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories.WorkshopPayment;

public class WorkshopPaymentWriteRepository : IWorkshopPaymentWriteRepository
{
    private readonly ApplicationDBContext _context;

    public WorkshopPaymentWriteRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Domain.Entities.WorkshopPayment payment, CancellationToken cancellationToken)
    {
        await _context.WorkshopPayments.AddAsync(payment, cancellationToken);
    }
}
