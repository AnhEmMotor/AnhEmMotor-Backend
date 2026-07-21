using System;
using System.Collections.Generic;

namespace Application.ApiContracts.Customer.Responses;

public class CustomerProfile360Response
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? AddressDetail { get; set; }

    public string? Ward { get; set; }

    public string? Province { get; set; }

    public string? Gender { get; set; }

    public DateTime? Birthday { get; set; }

    public string? IdentificationNumber { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool IsVerified { get; set; }

    public string Tier { get; set; } = string.Empty;

    public int Points { get; set; }

    public string? InterestedVehicle { get; set; }

    public Guid? AssignedToId { get; set; }

    public string? AssignedToName { get; set; }

    public List<OwnedVehicleInfo> Vehicles { get; set; } = new();

    public List<InvoiceSummary> Outputs { get; set; } = new();

    public List<MaintenanceHistorySummary> MaintenanceHistories { get; set; } = new();

    public List<WarrantyClaimSummary> WarrantyClaims { get; set; } = new();

    public List<TimelineEventResponse> TimelineEvents { get; set; } = new();

    public List<CareReminderResponse> CareReminders { get; set; } = new();

    public Profile360SummaryResponse Summary { get; set; } = new();
}

public class OwnedVehicleInfo
{
    public int Id { get; set; }

    public string? VinNumber { get; set; }

    public string? LicensePlate { get; set; }

    public string? EngineNumber { get; set; }

    public string? VariantName { get; set; }

    public string? ColorName { get; set; }

    public DateTimeOffset? PurchaseDate { get; set; }

    public string? Status { get; set; }

    public int CurrentOdo { get; set; }
}

public class InvoiceSummary
{
    public int Id { get; set; }

    public string? StatusId { get; set; }

    public string? StatusDisplayName { get; set; }

    public decimal? Total { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? LastStatusChangedAt { get; set; }

    public List<InvoiceItemSummary> Items { get; set; } = new();
}

public class InvoiceItemSummary
{
    public int Id { get; set; }

    public string? ProductName { get; set; }

    public int? Count { get; set; }

    public decimal? Price { get; set; }

    public string? CoverImageUrl { get; set; }
}

public class MaintenanceHistorySummary
{
    public int Id { get; set; }

    public string MaintenanceNumber { get; set; } = string.Empty;

    public int VehicleId { get; set; }

    public string? LicensePlate { get; set; }

    public string? VariantName { get; set; }

    public DateTimeOffset MaintenanceDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public int Mileage { get; set; }

    public decimal PartsCost { get; set; }

    public decimal LaborCost { get; set; }

    public decimal TotalCost { get; set; }

    public DateTimeOffset? NextMaintenanceDate { get; set; }
}

public class WarrantyClaimSummary
{
    public int Id { get; set; }

    public string ClaimNumber { get; set; } = string.Empty;

    public string StatusText { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}

public class TimelineEventResponse
{
    public string Date { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Status { get; set; }

    public int? RelatedId { get; set; }
}

public class CareReminderResponse
{
    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset? DueDate { get; set; }

    public string Priority { get; set; } = string.Empty;
}

public class Profile360SummaryResponse
{
    public int ActiveOutputsCount { get; set; }

    public int OwnedVehiclesCount { get; set; }

    public int OverdueRemindersCount { get; set; }
}
