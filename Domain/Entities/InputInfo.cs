using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InputInfo")]
    public class InputInfo : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("InputId")]
        [ForeignKey("InputReceipt")]
        public int InputId { get; set; }

        [Column("Count")]
        public int? Count { get; set; }

        [Column("RemainingCount")]
        public int? RemainingCount { get; set; }

        [Column("ParentOutputInfoId")]
        [ForeignKey("ParentOutputInfo")]
        public int? ParentOutputInfoId { get; set; }

        [Column("PurchaseRequestItemId")]
        [ForeignKey("PurchaseRequestItem")]
        public int? PurchaseRequestItemId { get; set; }

        [Column("QuotationProductRowId")]
        [ForeignKey("QuotationProductRow")]
        public int? QuotationProductRowId { get; set; }

        public InventoryReceipt? InputReceipt { get; set; }

        public PurchaseRequestItem? PurchaseRequestItem { get; set; }

        public QuotationProductRow? QuotationProductRow { get; set; }

        public OutputInfo? ParentOutputInfo { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = [];
    }
}
