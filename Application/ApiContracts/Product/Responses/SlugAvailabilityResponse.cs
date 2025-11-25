namespace Application.ApiContracts.Product.Responses;

public class SlugAvailabilityResponse
{
    public string Slug { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }
}

