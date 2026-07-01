using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ReturnRequest")]
    public class ReturnRequest : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string OriginalTrackingNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        
        /// <summary>
        /// 'return' or 'cancel'
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// 'pending', 'inspecting', 'completed', 'rejected'
        /// </summary>
        public string Status { get; set; } = "pending";
        
        public string Reason { get; set; } = string.Empty;
        public string? CancelReason { get; set; }
        public string? Note { get; set; }
        
        /// <summary>
        /// 'restock', 'defect', 'refund'
        /// </summary>
        public string? ReturnAction { get; set; }
        
        public string? EvidenceImagesJson { get; set; }
        public string? RejectionReason { get; set; }
        public DateTimeOffset? InspectedAt { get; set; }

        [ForeignKey("OrderId")]
        public virtual Output? Order { get; set; }
        
        public virtual ICollection<ReturnRequestItem> Items { get; set; } = new List<ReturnRequestItem>();
    }
}
