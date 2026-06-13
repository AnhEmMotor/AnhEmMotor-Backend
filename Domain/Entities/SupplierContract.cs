using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class SupplierContract : BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int? SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier? Supplier { get; set; }

        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ContractFilePath { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime? ExpirationDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ContractValue { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        [MaxLength(500)]
        public string? Terms { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CreditLimit { get; set; }

        public int? PaymentWindowDays { get; set; }

        [MaxLength(50)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(200)]
        public string? BankName { get; set; }

        public int? MinimumVolumePerMonth { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? DiscountRate { get; set; }

        public Guid? ParentContractId { get; set; }

        [ForeignKey("ParentContractId")]
        public SupplierContract? ParentContract { get; set; }

        public ICollection<SupplierContract> Addendums { get; set; } = [];

        public ICollection<SupplierContractItem> ContractItems { get; set; } = [];

        public ICollection<SupplierContractAuditLog> AuditLogs { get; set; } = [];
    }
}
