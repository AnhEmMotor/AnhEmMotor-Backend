using System.Text.Json.Serialization;

namespace Application.ApiContracts.Output;

public class UpdateOutputStatusRequest
{
    public string? StatusId { get; set; }
}
