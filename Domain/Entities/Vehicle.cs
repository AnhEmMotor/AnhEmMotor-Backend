using System;
using System.Collections.Generic;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnhEmMotor.Domain.Entities
{
    public class Vehicle : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public string Vin { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public double CurrentOdo { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public DateTime NextMaintenanceDate { get; set; }
        public double NextMaintenanceOdo { get; set; }
        public string ElectronicWarrantyQrCode { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Year { get; set; }
        
        // FK to User (Owner)
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        // Relation to Product (Catalog)
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
    }
}