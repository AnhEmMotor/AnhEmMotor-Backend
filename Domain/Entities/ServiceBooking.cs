using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnhEmMotor.Domain.Entities
{
    public class ServiceBooking : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; } = null!;

        public string ServiceType { get; set; } = string.Empty; // Bảo dưỡng định kỳ / Sửa chữa / Lái thử
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Notes { get; set; } = string.Empty;
        public BookingStatus Status { get; set; }

        public Guid? AssignedSaleId { get; set; }
        public virtual ApplicationUser? AssignedSale { get; set; }

        public string CustomerNote { get; set; } = string.Empty;
        public string AdminNote { get; set; } = string.Empty;

        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
    }
}
