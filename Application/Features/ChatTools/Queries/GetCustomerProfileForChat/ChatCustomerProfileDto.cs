namespace Application.Features.ChatTools.Queries.GetCustomerProfileForChat;

public record ChatCustomerProfileDto
{
    public int CustomerId { get; init; }

    public string? FullName { get; init; }

    public string? PhoneNumber { get; init; }

    public int TotalOrders { get; init; }

    public decimal TotalSpent { get; init; }

    public string? Tier { get; init; }
}
