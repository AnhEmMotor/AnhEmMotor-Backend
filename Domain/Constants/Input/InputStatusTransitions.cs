namespace Domain.Constants;

public static class InputStatusTransitions
{
    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        {
            InputStatus.Working,
            [InputStatus.Finish, InputStatus.Cancel]
        },
        {
            InputStatus.Finish,
            []
        },
        {
            InputStatus.Cancel,
            []
        }
    };

    public static bool IsTransitionAllowed(string? currentStatus, string? newStatus)
    { throw new NotImplementedException(); }

    public static HashSet<string> GetAllowedTransitions(string? currentStatus)
    { throw new NotImplementedException(); }
}
