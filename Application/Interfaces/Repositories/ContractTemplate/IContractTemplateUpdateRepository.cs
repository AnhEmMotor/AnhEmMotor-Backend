using ContractTemplateEntity = Domain.Entities.ContractTemplate;

namespace Application.Interfaces.Repositories.ContractTemplate;

public interface IContractTemplateUpdateRepository
{
    public void Update(ContractTemplateEntity contractTemplate);
}

