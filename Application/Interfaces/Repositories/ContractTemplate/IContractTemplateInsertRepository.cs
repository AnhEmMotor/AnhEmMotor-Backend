using ContractTemplateEntity = Domain.Entities.ContractTemplate;
using ContractTemplateAuditLogEntity = Domain.Entities.ContractTemplateAuditLog;

namespace Application.Interfaces.Repositories.ContractTemplate;

public interface IContractTemplateInsertRepository
{
    void Add(ContractTemplateEntity contractTemplate);
    void AddAuditLog(ContractTemplateAuditLogEntity auditLog);
}
