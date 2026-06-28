using Application.Interfaces.Repositories.Invoice;
using InvoiceEntity = Domain.Entities.Invoice;

namespace Application.Interfaces.Repositories.Invoice;

public interface IInvoiceWriteRepository
{
    void Update(InvoiceEntity invoice);
    void Add(InvoiceEntity invoice);
}
