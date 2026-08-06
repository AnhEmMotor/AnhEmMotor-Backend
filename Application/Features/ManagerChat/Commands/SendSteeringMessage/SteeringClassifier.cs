using Domain.Constants;

namespace Application.Features.ManagerChat.Commands.SendSteeringMessage;

/// <summary>
/// Phân loại chế độ steering — chỉ Tầng 1 (luật, xem 09-STAGE-STEERING.md mục 9.4).
/// </summary>
public static class SteeringClassifier
{
    private static readonly string[] CorrectionMarkers = ["à nhầm", "à quên", "nhầm rồi", "sai rồi", "không phải", "ý tôi là", "ý mình là", "sửa lại", "đổi thành", "thay vì", "khoan", "dừng lại", "bỏ qua", ];

    private static readonly HashSet<string> RestartExact = ["dừng", "stop", "thôi", "huỷ"];

    public static string? Classify(string text)
    {
        var lowered = text.Trim().ToLowerInvariant();
        if (CorrectionMarkers.Any(lowered.Contains))
            return ChatSteeringMode.Interrupt;
        if (RestartExact.Contains(lowered))
            return ChatSteeringMode.Restart;
        return null;
    }
}
