namespace Application.ApiContracts.SupplierContracts.Responses;

public class SupplierContractAuditLogResponse
{
    public Guid Id { get; set; }

    public Guid SupplierContractId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Details { get; set; }

    public string? ChangedBy { get; set; }

    public string? IpAddress { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
}
