using AnhEmMotor.Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities;

namespace AnhEmMotor.Domain.Entities
{
    [Table("SupportTicket")]
    public class SupportTicket : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        public Guid? CustomerId { get; set; }
        public virtual ApplicationUser? Customer { get; set; }

        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketPriority Priority { get; set; }
        public string Status { get; set; } = string.Empty; // Open, InProgress, Resolved, Closed
        public DateTime? ResolvedAt { get; set; }
        public DateTime SLADeadline { get; set; }
        
        public Guid? AssignedAdminId { get; set; }
        public virtual ApplicationUser? AssignedAdmin { get; set; }

        public virtual ICollection<CustomerContactReply> Replies { get; set; } = new List<CustomerContactReply>();
    }
}
