using System.Text.Json.Serialization;
using Application.ApiContracts.Output;
using Application;
using Application.ApiContracts;

namespace Application.ApiContracts.Output.Requests;

public class CreateOutputRequest
{
    public string? Notes { get; set; }

    [JsonPropertyName("products")]
    public List<Requests.CreateOutputInfoRequest> OutputInfos { get; set; } = [];
}
