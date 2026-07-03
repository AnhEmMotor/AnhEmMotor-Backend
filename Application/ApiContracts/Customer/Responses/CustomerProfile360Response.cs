using System;
using Application.ApiContracts.Leads.Responses;

namespace Application.ApiContracts.Customer.Responses;

public class CustomerProfile360Response
{
  // Lead profile
  public int Id { get; set; }
  public string FullName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string PhoneNumber { get; set; } = string.Empty;
  public int Score { get; set; }
  public string Status { get; set; } = string.Empty;
  public string Source { get; set; } = string.Empty;
  public string InterestedVehicle { get; set; } = string.Empty;
  public string Address { get; set; } = string.Empty;
  public string AddressDetail { get; set; } = string.Empty;
  public string Ward { get; set; } = string.Empty;
  public string District { get; set; } = string.Empty;
  public string Province { get; set; } = string.Empty;
  public string Gender { get; set; } = string.Empty;
  public DateTime? Birthday { get; set; }
  public string IdentificationNumber { get; set; } = string.Empty;
  public DateTimeOffset? CreatedAt { get; set; }
  public bool IsVerified { get; set; }
  public string Tier { get; set; } = string.Empty;
  public int Points { get; set; }
  public Guid? AssignedToId { get; set; }
  public string? AssignedToName { get; set; }

  // Activities
  public List<LeadActivityResponse> Activities { get; set; } = [];

  // Related outputs (transactions)
  public List<CustomerOutputSummary> Outputs { get; set; } = [];

  // Related vehicles
  public List<CustomerVehicleSummary> Vehicles { get; set; } = [];

  // Computed care reminders
  public List<CustomerCareReminder> CareReminders { get; set; } = [];

  // Progress timeline events (combined from outputs, plate dossier, finance, service)
  public List<CustomerTimelineEvent> TimelineEvents { get; set; } = [];

  // Summary counts
  public Customer360Summary Summary { get; set; } = new();
}

public class CustomerOutputSummary
{
  public int Id { get; set; }
  public string? StatusId { get; set; }
  public string? StatusDisplayName { get; set; }
  public DateTimeOffset? CreatedAt { get; set; }
  public DateTimeOffset? LastStatusChangedAt { get; set; }
  public decimal? Total { get; set; }
  public string? PaymentMethod { get; set; }
  public string? PaymentStatus { get; set; }
  public List<CustomerOutputItemSummary> Items { get; set; } = [];
}

public class CustomerOutputItemSummary
{
  public int? Id { get; set; }
  public string? ProductName { get; set; }
  public int? Count { get; set; }
  public decimal? Price { get; set; }
  public string? CoverImageUrl { get; set; }
}

public class CustomerVehicleSummary
{
  public int Id { get; set; }
  public string? LicensePlate { get; set; }
  public string VinNumber { get; set; } = string.Empty;
  public string EngineNumber { get; set; } = string.Empty;
  public DateTimeOffset PurchaseDate { get; set; }
  public string Status { get; set; } = string.Empty;
  public DateTime? LastMaintenanceDate { get; set; }
  public DateTime? NextMaintenanceDate { get; set; }
  public double? NextMaintenanceOdo { get; set; }
  public double CurrentOdo { get; set; }
}

public class CustomerCareReminder
{
  public string Type { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public DateTime? DueDate { get; set; }
  public string Priority { get; set; } = "normal"; // low, normal, high, urgent
}

public class CustomerTimelineEvent
{
  public DateTimeOffset Date { get; set; }
  public string Type { get; set; } = string.Empty; // output_status, plate_dossier, finance, service, activity
  public string Title { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? Status { get; set; }
  public int? RelatedId { get; set; }
}

public class Customer360Summary
{
  public int ActiveOutputsCount { get; set; }
  public int OwnedVehiclesCount { get; set; }
  public int OverdueRemindersCount { get; set; }
  public DateTimeOffset? LastInteractionDate { get; set; }
}
