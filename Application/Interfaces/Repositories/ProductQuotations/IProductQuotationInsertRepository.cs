using Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories.ProductQuotations
{
    public interface IProductQuotationInsertRepository
    {
        Task AddAsync(ProductQuotation row, CancellationToken cancellationToken);
    }
}
