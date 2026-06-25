using Domain.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("OrderLogistics")]
    public class OrderLogistics : BaseEntity
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        public int OrderId { get; set; }

        public OrderPipelineStage CurrentStage { get; set; }

        public string BottleneckDescription { get; set; } = string.Empty;

        public bool IsBottleneck { get; set; }

        public string DriverName { get; set; } = string.Empty;

        public string DriverPhone { get; set; } = string.Empty;

        public double CurrentLat { get; set; }

        public double CurrentLng { get; set; }

        public DateTime? EstimatedArrivalTime { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
