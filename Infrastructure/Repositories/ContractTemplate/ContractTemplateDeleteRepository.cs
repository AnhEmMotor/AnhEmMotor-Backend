using Application.Interfaces.Repositories.ContractTemplate;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.ContractTemplate;

public class ContractTemplateDeleteRepository(ApplicationDBContext context) : IContractTemplateDeleteRepository
{
    public void Remove(Domain.Entities.ContractTemplate contractTemplate) => context.Set<Domain.Entities.ContractTemplate>().Remove(contractTemplate);
}
