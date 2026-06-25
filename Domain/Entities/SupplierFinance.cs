using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class SupplierFinance : BaseEntity
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        public decimal CurrentDebt { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public Supplier? Supplier { get; set; }
    }
}

