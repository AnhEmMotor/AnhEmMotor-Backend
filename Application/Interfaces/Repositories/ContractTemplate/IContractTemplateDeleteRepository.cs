using ContractTemplateEntity = Domain.Entities.ContractTemplate;

namespace Application.Interfaces.Repositories.ContractTemplate;

public interface IContractTemplateDeleteRepository
{
    public void Remove(ContractTemplateEntity contractTemplate);
}

