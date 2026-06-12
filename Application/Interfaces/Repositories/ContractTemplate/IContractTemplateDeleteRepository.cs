using ContractTemplateEntity = Domain.Entities.ContractTemplate;

namespace Application.Interfaces.Repositories.ContractTemplate;

public interface IContractTemplateDeleteRepository
{
    void Remove(ContractTemplateEntity contractTemplate);
}
