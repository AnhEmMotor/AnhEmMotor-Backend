using Application.Interfaces.Repositories.ContractTemplate;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using ContractTemplateEntity = Domain.Entities.ContractTemplate;

namespace Infrastructure.Repositories.ContractTemplate;

public class ContractTemplateReadRepository(ApplicationDBContext context) : IContractTemplateReadRepository
{
    public Task<List<ContractTemplateEntity>> GetAllAsync(CancellationToken cancellationToken = default) => context.Set<ContractTemplateEntity>(
        )
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    public Task<ContractTemplateEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => context.Set<ContractTemplateEntity>(
        )
        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
