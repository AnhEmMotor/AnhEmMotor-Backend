using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Domain.Entities
{
    public class QuotationProductRow
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("QuotationId")]
        [ForeignKey("QuotationReceipt")]
        public int? QuotationId { get; set; }

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

        public Quotation? QuotationReceipt { get; set; }

        public ProductVariant? ProductVariant { get; set; }

        public ProductVariantColor? ProductVariantColor { get; set; }
    }
}
