namespace Application.ApiContracts.ManagerChat.Requests;

/// <summary>
/// 1 thay đổi trên plan — type: "edit" | "add" | "remove" | "reorder" | "comment" (xem Stage 10.4).
/// </summary>
public class UpdatePlanStepOperation
{
    public string Type { get; set; } = string.Empty;

    public string? StepId { get; set; }

    public string? Title { get; set; }

    public string? Detail { get; set; }

    public List<string>? ExpectedTools { get; set; }

    public int? Order { get; set; }

    public string? Comment { get; set; }
}
