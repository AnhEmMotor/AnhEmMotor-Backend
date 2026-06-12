using ContractTemplateEntity = Domain.Entities.ContractTemplate;

namespace Application.Interfaces.Repositories.ContractTemplate;

public interface IContractTemplateReadRepository
{
    Task<List<ContractTemplateEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ContractTemplateEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
