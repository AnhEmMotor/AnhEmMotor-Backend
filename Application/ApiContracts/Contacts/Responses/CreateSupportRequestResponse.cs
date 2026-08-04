namespace Application.ApiContracts.Contacts.Responses;

public record CreateSupportRequestResponse
{
    public int Id { get; init; }

    public Guid TrackingToken { get; init; }
}
