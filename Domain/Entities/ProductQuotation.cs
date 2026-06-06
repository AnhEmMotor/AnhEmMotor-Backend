using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ProductQuotation
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SupplierId")]
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int? ProductVariantId { get; set; }

        [Column("ProductVariantColorId")]
        [ForeignKey("ProductVariantColor")]
        public int? ProductVariantColorId { get; set; }

        [Column("QuotePrice")]
        public int? QuotePrice { get; set; }

        [Column("Note", TypeName = "nvarchar(MAX)")]
        public string? Note { get; set; }

        public ProductVariant? ProductVariant { get; set; }

        public ProductVariantColor? ProductVariantColor { get; set; }
    }
}
