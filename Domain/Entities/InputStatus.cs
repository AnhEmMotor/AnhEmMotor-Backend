using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("InventoryReceiptStatus")]
    public class InventoryReceiptStatus : BaseEntity
    {
        [Key]
        [Column("Key")]
        public string Key { get; set; } = string.Empty;

        public ICollection<InventoryReceipt> InventoryReceiptReceipts { get; set; } = [];
    }
}
