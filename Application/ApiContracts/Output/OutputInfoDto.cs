namespace Application.ApiContracts.Output;

public class OutputInfoDto
{
    public int? Id { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public short? Count { get; set; }

    public long? Price { get; set; }

    public long? CostPrice { get; set; }
}
