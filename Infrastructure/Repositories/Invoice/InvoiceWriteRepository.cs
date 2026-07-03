using Application.Interfaces.Repositories.Invoice;
using Infrastructure.DBContexts;
using InvoiceEntity = Domain.Entities.Invoice;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Invoice;

public class InvoiceWriteRepository(ApplicationDBContext context) : IInvoiceWriteRepository
{
    public void Update(InvoiceEntity invoice)
    {
        context.Set<InvoiceEntity>().Update(invoice);
    }

    public void Add(InvoiceEntity invoice)
    {
        context.Set<InvoiceEntity>().Add(invoice);
    }
}
