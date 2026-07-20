namespace Application.ApiContracts.Ai;

public class AiSearchResult
{
    public string Keyword { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public int PriceMin { get; set; }
    public int PriceMax { get; set; } = 60000000;
    public List<string> Colors { get; set; } = new();
    public string Intent { get; set; } = "search";
}
