namespace Application.ApiContracts.Admin.Invoices;

public record AdminInvoiceSummaryResponse(
    int Id,
    string InvoiceNumber,
    DateTime IssueDate,
    string CustomerName,
    string CustomerPhone,
    string CustomerIdCard,
    string CustomerAddress,
    string VehicleModel,
    string VehicleColor,
    string ChassisNo,
    string EngineNo,
    decimal VehiclePrice,
    decimal RegistrationFee,
    decimal InsuranceFee,
    decimal TotalAmount,
    string PaymentMethod,
    string? BankName,
    string Status,
    string? SalesPerson,
    DateTime? DeliveryDate,
    string? ProcessedBy,
    DateTime? ProcessedAt,
    DateTimeOffset? CreatedAt
);

public record AdminInvoiceDetailResponse(
    int Id,
    string InvoiceNumber,
    DateTime IssueDate,
    string CustomerName,
    string CustomerPhone,
    string CustomerIdCard,
    string CustomerAddress,
    string VehicleModel,
    string VehicleColor,
    string ChassisNo,
    string EngineNo,
    decimal VehiclePrice,
    decimal RegistrationFee,
    decimal InsuranceFee,
    decimal TotalAmount,
    string PaymentMethod,
    string? BankName,
    string Status,
    string? SalesPerson,
    DateTime? DeliveryDate,
    string? ProcessedBy,
    DateTime? ProcessedAt,
    DateTimeOffset? CreatedAt,
    List<InvoicePaymentBreakdownItem> PaymentBreakdown
);

public record InvoicePaymentBreakdownItem(
    string Method,
    decimal Amount,
    string? Note
);

public record CreateAdminInvoiceRequest(
    string CustomerName,
    string CustomerPhone,
    string CustomerIdCard,
    string CustomerAddress,
    string VehicleModel,
    string VehicleColor,
    string ChassisNo,
    string EngineNo,
    decimal VehiclePrice,
    decimal RegistrationFee,
    decimal InsuranceFee,
    string PaymentMethod,
    string? BankName,
    string? SalesPerson,
    DateTime? DeliveryDate,
    List<InvoicePaymentBreakdownItem>? PaymentBreakdown
);

public record UpdateAdminInvoiceRequest(
    string CustomerName,
    string CustomerPhone,
    string CustomerIdCard,
    string CustomerAddress,
    string VehicleModel,
    string VehicleColor,
    string ChassisNo,
    string EngineNo,
    decimal VehiclePrice,
    decimal RegistrationFee,
    decimal InsuranceFee,
    string PaymentMethod,
    string? BankName,
    string Status,
    string? SalesPerson,
    DateTime? DeliveryDate,
    List<InvoicePaymentBreakdownItem>? PaymentBreakdown
);

public record UpdateInvoiceStatusRequest(
    string Status,
    string? ProcessedBy
);
