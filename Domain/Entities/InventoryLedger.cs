using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InventoryLedger")]
    public class InventoryLedger : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("TransactionDate")]
        public DateTimeOffset TransactionDate { get; set; }

        [Column("DocumentCode", TypeName = "nvarchar(50)")]
        public string DocumentCode { get; set; } = string.Empty;

        [Column("TransactionType", TypeName = "nvarchar(50)")]
        public string TransactionType { get; set; } = string.Empty;

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int ProductVariantId { get; set; }

        [Column("ProductVariantColorId")]
        [ForeignKey("ProductVariantColor")]
        public int? ProductVariantColorId { get; set; }

        [Column("PartnerName", TypeName = "nvarchar(255)")]
        public string? PartnerName { get; set; }

        [Column("ImportQty")]
        public int ImportQty { get; set; }

        [Column("ExportQty")]
        public int ExportQty { get; set; }

        [Column("UnitPrice", TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Column("TotalAmount", TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Column("StockAfter")]
        public int StockAfter { get; set; }

        public ProductVariant? ProductVariant { get; set; }

        public ProductVariantColor? ProductVariantColor { get; set; }
    }
}
