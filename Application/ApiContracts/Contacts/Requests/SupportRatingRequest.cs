using System.ComponentModel.DataAnnotations;

namespace Application.ApiContracts.Contacts.Requests;

public record SupportRatingRequest
{
    [Range(1, 5)]
    public int Rating { get; init; }

    [MaxLength(1000)]
    public string? Comment { get; init; }
}

public record CustomerSupportRatingRequest : SupportRatingRequest
{
    public Guid TrackingToken { get; init; }
}
