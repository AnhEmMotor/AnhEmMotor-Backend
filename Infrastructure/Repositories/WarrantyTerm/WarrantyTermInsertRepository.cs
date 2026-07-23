using Application.Interfaces.Repositories.WarrantyTerm;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.WarrantyTerm;

public class WarrantyTermInsertRepository(ApplicationDBContext context) : IWarrantyTermInsertRepository
{
    public void Add(Domain.Entities.WarrantyTerm warrantyTerm)
    {
        context.Set<Domain.Entities.WarrantyTerm>().Add(warrantyTerm);
    }
}
