using System.Text.Json.Serialization;

namespace Application.ApiContracts.Option.Responses
{
    public class OptionResponse
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        [JsonPropertyName("values")]
        public List<OptionValueResponse> OptionValues { get; set; } = [];
    }
}
