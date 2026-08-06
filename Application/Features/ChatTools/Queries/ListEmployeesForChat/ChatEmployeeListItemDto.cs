namespace Application.Features.ChatTools.Queries.ListEmployeesForChat;

public record ChatEmployeeListItemDto
{
    public int EmployeeId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string JobTitle { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}
