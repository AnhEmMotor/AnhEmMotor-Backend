using System.Text.Json.Serialization;

namespace Application.ApiContracts.Input.Requests
{
public class CreateInputInfoRequest
{
	public int? ProductVarientId { get; set; }

	[JsonIgnore]
	public int? ProductId
	{
		get => ProductVarientId;
		set => ProductVarientId = value;
	}

	public int? ProductVarientColorId { get; set; }

	[JsonIgnore]
	public int? ProductVariantColorId
	{
		get => ProductVarientColorId;
		set => ProductVarientColorId = value;
	}

	public int? Count { get; set; }

	public decimal? Price { get; set; }

	[JsonIgnore]
	public decimal? InputPrice
	{
		get => Price;
		set => Price = value;
	}

	public List<VehicleInputRequest>? Vehicles { get; set; }
}
}
