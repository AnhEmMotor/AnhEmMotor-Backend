using Application.Interfaces.Repositories.ContractTemplate;
using Domain.Entities;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.ContractTemplate;

public class ContractTemplateInsertRepository(ApplicationDBContext context) : IContractTemplateInsertRepository
{
    public void Add(Domain.Entities.ContractTemplate contractTemplate) => context.Set<Domain.Entities.ContractTemplate>(
        )
        .Add(contractTemplate);

    public void AddAuditLog(ContractTemplateAuditLog auditLog) => context.Set<ContractTemplateAuditLog>().Add(auditLog);
}
