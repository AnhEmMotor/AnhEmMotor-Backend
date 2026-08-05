namespace Application.Features.ChatTools.Queries.GetLoyaltyMembersForChat;

public record ChatLoyaltyMemberDto
{
    public int LeadId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Tier { get; init; } = string.Empty;

    public int Points { get; init; }
}
