using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("CustomerContactReply")]
    public class CustomerContactReply : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ContactId")]
        [ForeignKey("CustomerContact")]
        public int ContactId { get; set; }

        [Column("ReplyContent", TypeName = "nvarchar(MAX)")]
        public string ReplyContent { get; set; } = string.Empty;

        [Column("IsInternal")]
        public bool IsInternal { get; set; }

        [Column("RepliedBy")]
        [ForeignKey("RepliedByUser")]
        public Guid? RepliedBy { get; set; }

        [Column("SentAt")]
        public DateTimeOffset? SentAt { get; set; }

        public CustomerContact? CustomerContact { get; set; }

        public ApplicationUser? RepliedByUser { get; set; }
    }
}
