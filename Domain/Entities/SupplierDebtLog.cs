using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("SupplierDebtLog")]
    public class SupplierDebtLog : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int SupplierId { get; set; }

        public decimal AmountPaid { get; set; }

        public decimal RemainingDebt { get; set; }

        public DateTimeOffset PaymentDate { get; set; } = DateTimeOffset.UtcNow;

        public Guid? CreatedById { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; } = null!;

        [ForeignKey("CreatedById")]
        public virtual ApplicationUser? CreatedBy { get; set; }

        public virtual ICollection<SupplierDebtLogImage> ProofImages { get; set; } = new List<SupplierDebtLogImage>();
    }
}
