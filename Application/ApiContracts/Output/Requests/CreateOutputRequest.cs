using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output.Requests;

public class CreateOutputRequest
{
    public string? Notes { get; set; }

    [JsonPropertyName("products")]
    public List<CreateOutputInfoRequest> OutputInfos { get; set; } = [];
}
