using AnhEmMotor.Domain.Constants;
using System;
using System.Collections.Generic;
using Domain.Entities;

namespace AnhEmMotor.Domain.Entities
{
    public class SupportTicket : BaseEntity
    {
        public string CustomerId { get; set; } = string.Empty;
        public virtual ApplicationUser Customer { get; set; } = null!;

        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketPriority Priority { get; set; }
        public string Status { get; set; } = string.Empty; // Open, InProgress, Resolved, Closed
        public DateTime? ResolvedAt { get; set; }
        public DateTime SLADeadline { get; set; }
        
        public string AssignedAdminId { get; set; } = string.Empty;
        public virtual ApplicationUser AssignedAdmin { get; set; } = null!;

        public virtual ICollection<CustomerContactReply> Replies { get; set; } = new List<CustomerContactReply>();
    }
}
