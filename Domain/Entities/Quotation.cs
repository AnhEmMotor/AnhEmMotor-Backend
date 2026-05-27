using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Domain.Entities
{
    public class Quotation : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Code", TypeName = "nvarchar(100)")]
        public string? Code { get; set; }

        [Column("SupplierId")]
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        [Column("Status", TypeName = "varchar(30)")]
        public string? Status { get; set; }

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        public ICollection<QuotationProductRow> QuotationProductRows { get; set; } = [];

        public Supplier? Supplier { get; set; }
    }
}
