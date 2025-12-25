namespace Application.ApiContracts.Output.Responses;

public class OutputInfoResponse
{
    public int? Id { get; set; }

    public int? ProductId { get; set; }

    public string? ProductName { get; set; }

    public int? Count { get; set; }

    public decimal? Price { get; set; }

    public decimal? CostPrice { get; set; }
}
