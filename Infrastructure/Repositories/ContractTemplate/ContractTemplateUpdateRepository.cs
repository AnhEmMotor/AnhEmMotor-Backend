using Application.Interfaces.Repositories.ContractTemplate;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.ContractTemplate;

public class ContractTemplateUpdateRepository(ApplicationDBContext context) : IContractTemplateUpdateRepository
{
    public void Update(Domain.Entities.ContractTemplate contractTemplate) => context.Set<Domain.Entities.ContractTemplate>(
        )
        .Update(contractTemplate);
}
