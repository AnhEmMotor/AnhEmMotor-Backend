using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Domain.Entities
{
    [Table("Output")]
    public class Output : BaseEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("CustomerName")]
        public string? CustomerName { get; set; }

        [Column("CustomerAddress")]
        public string? CustomerAddress { get; set; }

        [Column("CustomerPhone")]
        public string? CustomerPhone { get; set; }

        [Column("LastStatusChangedAt")]
        public DateTimeOffset? LastStatusChangedAt { get; set; }

        [Column("BuyerId")]
        [ForeignKey("Buyer")]
        public Guid? BuyerId { get; set; }

        [Column("CreatedBy")]
        [ForeignKey("CreatedByUser")]
        public Guid? CreatedBy { get; set; }

        [Column("FinishedBy")]
        [ForeignKey("FinishedByUser")]
        public Guid? FinishedBy { get; set; }

        [Column("StatusId")]
        [ForeignKey("OutputStatus")]
        public string? StatusId { get; set; }

        [Column("PaymentMethod")]
        public string? PaymentMethod { get; set; }

        [Column("TransactionId")]
        public string? TransactionId { get; set; }

        [Column("PaymentStatus")]
        public string? PaymentStatus { get; set; }

        [Column("PaidAmount", TypeName = "decimal(18, 2)")]
        public decimal? PaidAmount { get; set; }

        [Column("PaidAt")]
        public DateTimeOffset? PaidAt { get; set; }

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        [Column("DepositRatio")]
        public int? DepositRatio { get; set; }

        [Column("PaymentUrl", TypeName = "nvarchar(MAX)")]
        public string? PaymentUrl { get; set; }

        [Column("PaymentCode")]
        public string? PaymentCode { get; set; }

        [Column("PaymentExpiredAt")]
        public DateTimeOffset? PaymentExpiredAt { get; set; }

        public ApplicationUser? Buyer { get; set; }

        public ApplicationUser? CreatedByUser { get; set; }

        public ApplicationUser? FinishedByUser { get; set; }

        public OutputStatus? OutputStatus { get; set; }

        public ICollection<OutputInfo> OutputInfos { get; set; } = [];

        [InverseProperty("Output")]
        public ICollection<Input> Returns { get; set; } = [];

        [NotMapped]
        public decimal Total => OutputInfos?.Sum(x => (x.Price ?? 0) * (x.Count ?? 0)) ?? 0;

        [NotMapped]
        public decimal DepositAmount => Total * (DepositRatio ?? 0) / 100;
    }
}