using Domain.Enums;
using System;

namespace Domain.Entities
{
    public class ContractTemplate : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public decimal Version { get; set; } = 1.0m;

        public string Content { get; set; } = string.Empty;

        public string DynamicFields { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ContractTemplateStatus Status { get; set; } = ContractTemplateStatus.Active;

        public Guid? ParentId { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
