using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output.Requests;

public class CreateOutputForManagerRequest
{
    public Guid? BuyerId { get; set; }

    public Guid? CurrentUserId { get; init; }

    public string? Notes { get; set; }

    public string? StatusId { get; init; }

    [JsonPropertyName("products")]
    public List<CreateOutputInfoRequest> OutputInfos { get; set; } = [];
}
