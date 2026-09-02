namespace Application.ApiContracts.SalesContracts.Responses;

public class SalesContractResponse
{
    public Guid Id { get; set; }

    public string ContractNumber { get; set; } = string.Empty;

    public int? InvoiceId { get; set; }

    public string? InvoiceNumber { get; set; }

    public Guid? CustomerId { get; set; }

    public string Status { get; set; } = "Draft";

    public DateTimeOffset? SignedDate { get; set; }

    public string? ScannedFileUrl { get; set; }

    public string? SpecialTerms { get; set; }

    public string? WarrantyPeriod { get; set; }

    public string? WarrantyScope { get; set; }

    public string? Note { get; set; }

    public string? RejectReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? ShowroomName { get; set; }

    public string? ShowroomTaxCode { get; set; }

    public string? ShowroomAddress { get; set; }

    public string? ShowroomRepresentative { get; set; }

    public string? CustomerFullName { get; set; }

    public string? CustomerCCCD { get; set; }

    public string? CustomerAddress { get; set; }

    public string CustomerPhone { get; set; } = string.Empty;

    public string? CustomerName => CustomerFullName;

    public string VehicleModel { get; set; } = string.Empty;

    public string? VehicleVersion { get; set; }

    public string? VehicleColor { get; set; }

    public string? Vehicle => !string.IsNullOrEmpty(VehicleVersion) ? $"{VehicleModel} {VehicleVersion}" : VehicleModel;

    public string FrameNumber { get; set; } = string.Empty;

    public string EngineNumber { get; set; } = string.Empty;

    public decimal ActualSalePrice { get; set; }

    public decimal DepositAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public DateTimeOffset? FinalPaymentDeadline { get; set; }

    public DateTimeOffset? DeliveryDeadline => FinalPaymentDeadline;
}
