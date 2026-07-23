using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyTerms.Commands.UpdateWarrantyTerm;

public sealed record UpdateWarrantyTermCommand : IRequest<Result<bool>>
{
    public int Id { get; init; }

    public string TermName { get; init; } = string.Empty;

    public string? TermNameJson { get; init; }

    public int BrandId { get; init; }

    public string VehicleType { get; init; } = string.Empty;

    public string ErrorCategory { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? DescriptionJson { get; init; }

    public int? DurationMonths { get; init; }

    public int? DurationKm { get; init; }

    public string? Coverage { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime? EffectiveDate { get; init; }

    public DateTime? ExpirationDate { get; init; }

    public string? MediaUrl { get; init; }
}
