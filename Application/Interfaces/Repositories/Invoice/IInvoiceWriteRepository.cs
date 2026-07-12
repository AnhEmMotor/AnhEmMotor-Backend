using InvoiceEntity = Domain.Entities.Invoice;

namespace Application.Interfaces.Repositories.Invoice;

public interface IInvoiceWriteRepository
{
    public void Update(InvoiceEntity invoice);

    public void Add(InvoiceEntity invoice);
}
