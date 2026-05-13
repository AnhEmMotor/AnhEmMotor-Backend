using System.Text.Json.Serialization;

namespace Infrastructure.Services.Models;

public class FacebookPicture
{
    [JsonPropertyName("data")]
    public FacebookPictureData? Data { get; set; }
}
