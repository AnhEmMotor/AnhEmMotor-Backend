using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        public ApplicationUser? Buyer { get; set; }

        public ApplicationUser? CreatedByUser { get; set; }

        public ApplicationUser? FinishedByUser { get; set; }

        public OutputStatus? OutputStatus { get; set; }

        public ICollection<OutputInfo> OutputInfos { get; set; } = [];

        [InverseProperty("Output")]
        public ICollection<Input> Returns { get; set; } = [];
    }
}