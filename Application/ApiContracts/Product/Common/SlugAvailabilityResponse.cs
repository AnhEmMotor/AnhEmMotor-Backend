namespace Application.ApiContracts.Product.Common;

public class SlugAvailabilityResponse
{
    public string Slug { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}

