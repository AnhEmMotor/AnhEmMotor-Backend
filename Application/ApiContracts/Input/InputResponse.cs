namespace Application.ApiContracts.Input;

public class InputResponse
{
    public int? Id { get; set; }

    public DateTimeOffset? InputDate { get; set; }

    public string? Notes { get; set; }

    public string? StatusId { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public long? TotalPayable { get; set; }

    public List<InputInfoDto> Products { get; set; } = [];
}
