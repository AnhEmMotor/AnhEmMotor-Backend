using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output;

public class CreateOutputRequest
{
    public string? Notes { get; set; }

    [JsonPropertyName("products")]
    public List<CreateOutputInfoRequest> OutputInfos { get; set; } = [];
}

public class CreateOutputInfoRequest
{
    public int? ProductId { get; set; }

    public short? Count { get; set; }
}
