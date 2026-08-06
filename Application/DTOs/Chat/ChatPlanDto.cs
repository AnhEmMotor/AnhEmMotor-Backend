namespace Application.DTOs.Chat;

public record ChatPlanDto(
    Guid RunId,
    int Version,
    string Status,
    List<PlanStepDto> Steps,
    string LastEditedBy,
    DateTime? ApprovedAt);
