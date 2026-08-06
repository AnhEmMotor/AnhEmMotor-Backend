namespace Application.Features.ChatTools.Queries.GetWarrantyTermsForChat;

public class ChatWarrantyTermDto
{
    public int TermId { get; init; }

    public string? BrandName { get; init; }

    public string TermName { get; init; } = string.Empty;

    public string VehicleType { get; init; } = string.Empty;

    public string ErrorCategory { get; init; } = string.Empty;

    public int? DurationMonths { get; init; }

    public int? DurationKm { get; init; }

    public string? Coverage { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime? EffectiveDate { get; init; }

    public DateTime? ExpirationDate { get; init; }
}
