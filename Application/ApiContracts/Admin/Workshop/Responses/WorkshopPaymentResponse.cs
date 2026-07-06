namespace Application.ApiContracts.Admin.Workshop.Responses;

public class WorkshopPaymentResponse
{
    public int Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? VehicleInfo { get; set; }
    public string? ServiceDescription { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentStatusLabel { get; set; }
    public string? ReceivedByName { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? InvoicePrintedAt { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
