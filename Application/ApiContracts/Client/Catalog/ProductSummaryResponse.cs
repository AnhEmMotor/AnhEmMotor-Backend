namespace Application.ApiContracts.Client.Catalog
{
    public record ProductSummaryResponse(
        int Id,
        string Name,
        string ImageUrl,
        decimal ReferencePrice,
        string PromotionText,
        string? CategoryName,
        string? BrandName,
        string? TypeName);

    public record ProductDetailResponse(
        int Id,
        string Name,
        string Description,
        decimal ReferencePrice,
        List<string> Features,
        bool IsCompatibleWithMyVehicle,
        string CompatibilityNote);

    public record ConsultationRequest(int ProductId, string CustomerNote, string PreferredContactTime);
}
