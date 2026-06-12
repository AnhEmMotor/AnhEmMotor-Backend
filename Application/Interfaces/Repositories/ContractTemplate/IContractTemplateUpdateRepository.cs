using ContractTemplateEntity = Domain.Entities.ContractTemplate;

namespace Application.Interfaces.Repositories.ContractTemplate;

public interface IContractTemplateUpdateRepository
{
    void Update(ContractTemplateEntity contractTemplate);
}
