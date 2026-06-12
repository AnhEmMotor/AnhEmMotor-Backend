using Application.Interfaces.Repositories.ContractTemplate;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.ContractTemplate;

public class ContractTemplateInsertRepository(ApplicationDBContext context) : IContractTemplateInsertRepository
{
    public void Add(Domain.Entities.ContractTemplate contractTemplate) => context.Set<Domain.Entities.ContractTemplate>().Add(contractTemplate);
    public void AddAuditLog(Domain.Entities.ContractTemplateAuditLog auditLog) => context.Set<Domain.Entities.ContractTemplateAuditLog>().Add(auditLog);
}
