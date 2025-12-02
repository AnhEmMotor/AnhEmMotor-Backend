using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output;

public class UpdateOutputRequest
{
    public string? Notes { get; set; }

    [JsonPropertyName("products")]
    public List<UpdateOutputInfoRequest> OutputInfos { get; set; } = [];
}

public class UpdateOutputInfoRequest
{
    public int? Id { get; set; }

    public int? ProductId { get; set; }

    public short? Count { get; set; }
}
