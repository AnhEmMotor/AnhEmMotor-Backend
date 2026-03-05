namespace Application.ApiContracts.Input.Responses;

public class InputListResponse
{
    public int? Id { get; set; }

    public string? Notes { get; set; }

    public string? StatusId { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public long? TotalPayable { get; set; }

    public List<InputInfoResponse> Products { get; set; } = [];
}
