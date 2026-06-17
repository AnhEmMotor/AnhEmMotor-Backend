using AnhEmMotor.Domain.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities;

namespace AnhEmMotor.Domain.Entities
{
    [Table("OrderLogistics")]
    public class OrderLogistics : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public int OrderId { get; set; } // Assuming Order exists or use InvoiceId
        public OrderPipelineStage CurrentStage { get; set; }
        public string BottleneckDescription { get; set; } = string.Empty;
        public bool IsBottleneck { get; set; }
        
        // Shipping Info
        public string DriverName { get; set; } = string.Empty;
        public string DriverPhone { get; set; } = string.Empty;
        public double CurrentLat { get; set; }
        public double CurrentLng { get; set; }
        public DateTime? EstimatedArrivalTime { get; set; }
        
        public DateTime LastUpdated { get; set; }
    }
}