using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output.Requests;

public class UpdateOutputForManagerRequest
{
    public string? Notes { get; set; }

    [JsonPropertyName("products")]
    public List<UpdateOutputInfoRequest> OutputInfos { get; set; } = [];
}
