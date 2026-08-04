namespace Application.DTOs.Chat;

/// <summary>Kết quả xử lý 1 tin nhắn chat trong lúc plan đang Drafting/Ready (Stage 10.9) —
/// Action: "approved" | "rejected" | "edited" | "unclear".</summary>
public record PlanChatResultDto(string Action, ChatPlanDto? Plan, string? Reply);
