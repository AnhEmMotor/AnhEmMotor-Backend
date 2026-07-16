
namespace Application.ApiContracts.Output.Responses;

public class MyOrderItemResponse
{
    public string? ProductName { get; set; }

    public int? Count { get; set; }

    public decimal? Price { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? VariantName { get; set; }

    public string? ColorName { get; set; }

    public string? ColorCode { get; set; }
}
