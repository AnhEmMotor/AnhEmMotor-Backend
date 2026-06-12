using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Logistics;

public enum ParcelDeliveryStatus
{
    Pending = 0,      // Chờ soạn
    Packing = 1,      // Đang gói
    Shipping = 2,     // Đang đi đường
    Completed = 3,    // Giao thành công
    Returned = 4      // Khách bom hàng/Chuyển hoàn
}

public class ParcelDeliveryOrder
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string TrackingNumber { get; set; } = string.Empty;

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string Carrier { get; set; } = string.Empty;

    public ParcelDeliveryStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpectedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public decimal CodAmount { get; set; } // COD

    public decimal ShippingCost { get; set; } // chi phí ship thực tế

    public DateTime? InspectedAt { get; set; } // hoàn đã khui hộp chưa

    public string? ReturnReason { get; set; } // Lý do hoàn từ hệ thống

    public string? BoxCondition { get; set; } // Tình trạng vỏ hộp/bao bì

    public string? ProductCondition { get; set; } // Tình trạng phụ tùng bên trong

    public string? ReturnProofImage { get; set; } // Ảnh bằng chứng

    public string? ReturnInternalNote { get; set; } // Ghi chú nội bộ

    public string? ReturnAction { get; set; } // Hành động: Restock, Defect, Refund

    [Required]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    public string CustomerAddress { get; set; } = string.Empty;

    public string OriginalOrderCode { get; set; } = string.Empty;

    public ICollection<ParcelDeliveryOrderItem> Items { get; set; } = new List<ParcelDeliveryOrderItem>();

    public ParcelDeliveryOrder() { }
}

