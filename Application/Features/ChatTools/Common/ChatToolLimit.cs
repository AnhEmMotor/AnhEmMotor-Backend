namespace Application.Features.ChatTools.Common;

public static class ChatToolLimit
{
    public const int Default = 10;
    public const int Max = 25;

    public static int Clamp(int requested) => requested <= 0 ? Default : Math.Min(requested, Max);
}
