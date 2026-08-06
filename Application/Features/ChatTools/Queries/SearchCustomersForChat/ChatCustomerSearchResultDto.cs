namespace Application.Features.ChatTools.Queries.SearchCustomersForChat;

public record ChatCustomerSearchResultDto
{
    public int CustomerId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? PhoneNumber { get; init; }
}
