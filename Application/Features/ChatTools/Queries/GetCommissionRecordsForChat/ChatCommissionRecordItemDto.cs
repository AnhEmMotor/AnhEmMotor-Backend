namespace Application.Features.ChatTools.Queries.GetCommissionRecordsForChat;

public record ChatCommissionRecordItemDto
{
    public int EmployeeId { get; init; }

    public string EmployeeName { get; init; } = string.Empty;

    public int OutputId { get; init; }

    public decimal Amount { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime DateEarned { get; init; }

    public DateTime? PaidAt { get; init; }
}
