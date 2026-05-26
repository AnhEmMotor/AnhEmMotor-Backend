using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities;

namespace AnhEmMotor.Domain.Entities
{
    public class Lead : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // Catalog, Callback, Facebook, etc.
        public string InterestProduct { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // New, Contacted, Qualified, Lost, Converted
        public string Priority { get; set; } = string.Empty; // Hot, Warm, Cold
        
        public string AssignedSaleId { get; set; } = string.Empty;
        public virtual ApplicationUser AssignedSale { get; set; } = null!;
    }
}
