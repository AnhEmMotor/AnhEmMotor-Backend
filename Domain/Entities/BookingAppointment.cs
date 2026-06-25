using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("BookingAppointment")]
    public class BookingAppointment : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("FullName", TypeName = "nvarchar(150)")]
        public string FullName { get; set; } = string.Empty;

        [Column("Phone", TypeName = "nvarchar(20)")]
        public string Phone { get; set; } = string.Empty;

        [Column("Email", TypeName = "nvarchar(150)")]
        public string? Email { get; set; }

        [Column("ProductVariantId")]
        [ForeignKey("ProductVariant")]
        public int? ProductVariantId { get; set; }

        [Column("PreferredDate")]
        public DateTime? PreferredDate { get; set; }

        [Column("PreferredTimeSlot", TypeName = "nvarchar(100)")]
        public string? PreferredTimeSlot { get; set; }

        [Column("AppointmentAt")]
        public DateTimeOffset? AppointmentAt { get; set; }

        [Column("Showroom", TypeName = "nvarchar(150)")]
        public string? Showroom { get; set; }

        [Column("Status", TypeName = "nvarchar(30)")]
        public string Status { get; set; } = "pending";

        [Column("Notes", TypeName = "nvarchar(MAX)")]
        public string? Notes { get; set; }

        [Column("ConfirmedAt")]
        public DateTimeOffset? ConfirmedAt { get; set; }

        [Column("ConfirmedBy")]
        [ForeignKey("ConfirmedByUser")]
        public Guid? ConfirmedBy { get; set; }

        [Column("CancelReason", TypeName = "nvarchar(500)")]
        public string? CancelReason { get; set; }

        public ProductVariant? ProductVariant { get; set; }

        public ApplicationUser? ConfirmedByUser { get; set; }
    }
}
