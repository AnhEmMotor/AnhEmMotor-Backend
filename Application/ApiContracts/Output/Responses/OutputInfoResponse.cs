
namespace Application.ApiContracts.Output.Responses;

public class OutputInfoResponse
{
    public int? Id { get; set; }

    public int? ProductVarientId { get; set; }

    public int? ProductVarientColorId { get; set; }

    public string? ProductName { get; set; }

    public int? Count { get; set; }

    public decimal? Price { get; set; }

    public decimal? CostPrice { get; set; }

    public string? CoverImageUrl { get; set; }
}
