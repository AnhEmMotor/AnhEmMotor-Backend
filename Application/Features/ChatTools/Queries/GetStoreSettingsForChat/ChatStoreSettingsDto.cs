namespace Application.Features.ChatTools.Queries.GetStoreSettingsForChat;

public record ChatStoreSettingsDto
{
    public string? OrderValueExceeds { get; init; }

    public string? DepositRatio { get; init; }
}
