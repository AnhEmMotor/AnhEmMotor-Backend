using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InventoryReceipt")]
    public class InventoryReceipt : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("InventoryReceiptDate")]
        public DateTimeOffset? InventoryReceiptDate { get; set; }

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        [Column("StatusId")]
        [ForeignKey("InventoryReceiptStatus")]
        public string? StatusId { get; set; }

        [Column("PurchaseRequestId")]
        [ForeignKey("PurchaseRequest")]
        public int? PurchaseRequestId { get; set; }

        [Column("CreatedBy")]
        [ForeignKey("CreatedByUser")]
        public Guid? CreatedBy { get; set; }

        [Column("ConfirmedBy")]
        [ForeignKey("ConfirmedByUser")]
        public Guid? ConfirmedBy { get; set; }

        [Column("SourceOrderId")]
        [ForeignKey("Output")]
        public int? SourceOrderId { get; set; }

        public InventoryReceiptStatus? InventoryReceiptStatus { get; set; }

        public Output? Output { get; set; }

        public PurchaseRequest? PurchaseRequest { get; set; }

        public ApplicationUser? CreatedByUser { get; set; }

        public ApplicationUser? ConfirmedByUser { get; set; }

        public ICollection<InventoryReceiptInfo> InventoryReceiptInfos { get; set; } = [];
    }
}
