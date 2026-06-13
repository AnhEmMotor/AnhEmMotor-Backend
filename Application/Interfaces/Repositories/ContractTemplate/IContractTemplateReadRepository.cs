using ContractTemplateEntity = Domain.Entities.ContractTemplate;

namespace Application.Interfaces.Repositories.ContractTemplate;

public interface IContractTemplateReadRepository
{
    public Task<List<ContractTemplateEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<ContractTemplateEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

