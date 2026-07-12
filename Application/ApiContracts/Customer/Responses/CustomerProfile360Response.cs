namespace Application.ApiContracts.Customer.Responses;

public class CustomerProfile360Response
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Cccd { get; set; }

    public string MembershipGrade { get; set; } = string.Empty;

    public int LoyaltyPoints { get; set; }

    public string? DefaultRescueAddress { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public List<OwnedVehicleInfo> OwnedVehicles { get; set; } = new();

    public List<BookingSummary> RecentBookings { get; set; } = new();

    public List<InvoiceSummary> RecentInvoices { get; set; } = new();

    public List<WarrantyClaimSummary> WarrantyClaims { get; set; } = new();

    public List<SupportTicketSummary> SupportTickets { get; set; } = new();

    public List<FeedbackSummary> Feedbacks { get; set; } = new();
}

public class OwnedVehicleInfo
{
    public int VehicleId { get; set; }

    public string? VinNumber { get; set; }

    public string? LicensePlate { get; set; }

    public string? VariantName { get; set; }

    public string? ColorName { get; set; }

    public DateTimeOffset? PurchaseDate { get; set; }

    public string? WarrantyStatus { get; set; }
}

public class BookingSummary
{
    public int BookingId { get; set; }

    public string ServiceType { get; set; } = string.Empty;

    public DateTimeOffset AppointmentDate { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class InvoiceSummary
{
    public int InvoiceId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}

public class WarrantyClaimSummary
{
    public int Id { get; set; }

    public string ClaimNumber { get; set; } = string.Empty;

    public string StatusText { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}

public class SupportTicketSummary
{
    public int Id { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}

public class FeedbackSummary
{
    public int Id { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
