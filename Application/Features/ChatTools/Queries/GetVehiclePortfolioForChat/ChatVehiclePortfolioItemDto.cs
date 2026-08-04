namespace Application.Features.ChatTools.Queries.GetVehiclePortfolioForChat;

public sealed record ChatVehiclePortfolioItemDto
{
    public int VehicleId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string LicensePlate { get; init; } = string.Empty;

    public string VinNumber { get; init; } = string.Empty;

    public string? BrandName { get; init; }

    public string? VariantName { get; init; }

    public string? ColorName { get; init; }

    public DateTimeOffset PurchaseDate { get; init; }
}
