
namespace Application.ApiContracts.Input.Responses;

public class InputInfoResponse
{
    public int? Id { get; set; }

    public int? ProductVarientId { get; set; }

    public int? ProductVarientColorId { get; set; }

    public string? ProductVarientColorName { get; set; }

    public string? Name { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? ImportPrice { get; set; }

    public decimal? Discount { get; set; }

    public decimal? Total { get; set; }

    public int? RemainingCount { get; set; }

    public List<InputVehicleResponse> Vehicles { get; set; } = [];
}
