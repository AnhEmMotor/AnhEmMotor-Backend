namespace Application.ApiContracts.ConversionTools.Responses;

public class ConversionToolResponse
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int? DelaySeconds { get; set; }
    public string? Pages { get; set; }
    public bool IsActive { get; set; }
    public int Views { get; set; }
    public int Clicks { get; set; }
    public string? ImageUrl { get; set; }
    public string? Url { get; set; }
    public string? Status { get; set; }
    public int Leads { get; set; }
}
