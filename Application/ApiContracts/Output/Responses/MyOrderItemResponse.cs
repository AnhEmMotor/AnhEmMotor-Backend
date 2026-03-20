namespace Application.ApiContracts.Output.Responses;

public class MyOrderItemResponse
{
    public string? ProductName { get; set; }

    public int? Count { get; set; }

    public decimal? Price { get; set; }

    public string? CoverImageUrl { get; set; }
}
