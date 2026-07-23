using Application.Interfaces.Repositories.WarrantyTerm;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.WarrantyTerm;

public class WarrantyTermDeleteRepository(ApplicationDBContext context) : IWarrantyTermDeleteRepository
{
    public void Delete(Domain.Entities.WarrantyTerm warrantyTerm)
    {
        context.Set<Domain.Entities.WarrantyTerm>().Remove(warrantyTerm);
    }
}
