using System.Text.Json.Serialization;
using Application.ApiContracts.Output;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Output.Requests;

public class UpdateOutputRequest
{
    public string? Notes { get; set; }

    [JsonPropertyName("products")]
    public List<Requests.UpdateOutputInfoRequest> OutputInfos { get; set; } = [];
}
