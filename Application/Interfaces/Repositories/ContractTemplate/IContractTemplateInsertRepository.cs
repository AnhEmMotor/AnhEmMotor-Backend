using ContractTemplateEntity = Domain.Entities.ContractTemplate;
using ContractTemplateAuditLogEntity = Domain.Entities.ContractTemplateAuditLog;

namespace Application.Interfaces.Repositories.ContractTemplate;

public interface IContractTemplateInsertRepository
{
    public void Add(ContractTemplateEntity contractTemplate);
    public void AddAuditLog(ContractTemplateAuditLogEntity auditLog);
}

