using ContractTemplateAuditLogEntity = Domain.Entities.ContractTemplateAuditLog;
using ContractTemplateEntity = Domain.Entities.ContractTemplate;

namespace Application.Interfaces.Repositories.ContractTemplate;

public interface IContractTemplateInsertRepository
{
    public void Add(ContractTemplateEntity contractTemplate);

    public void AddAuditLog(ContractTemplateAuditLogEntity auditLog);
}

