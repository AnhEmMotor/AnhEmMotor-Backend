
namespace Application.ApiContracts.Output.Responses;

public class OutputInfoResponse
{
    public int? Id { get; set; }

    public int? ProductVariantId { get; set; }

    public int? ProductVariantColorId { get; set; }

    public string? ProductName { get; set; }

    public int? Count { get; set; }

    public decimal? Price { get; set; }

    public decimal? CostPrice { get; set; }

    public string? CoverImageUrl { get; set; }

    public List<VehicleAssignmentOptionResponse> AssignedVehicles { get; set; } = [];
}
