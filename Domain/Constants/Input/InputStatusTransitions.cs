namespace Domain.Constants.Input;

public static class InputStatusTransitions
{
    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        { InputStatus.Working, [ InputStatus.Finish, InputStatus.Cancel ] },
        { InputStatus.Finish, [] },
        { InputStatus.Cancel, [] }
    };

    public static bool IsTransitionAllowed(string? currentStatus, string? newStatus)
    {
        if(currentStatus == null || newStatus == null)
        {
            return false;
        }

        return AllowedTransitions.TryGetValue(currentStatus, out var allowedNext) && allowedNext.Contains(newStatus);
    }

    public static HashSet<string> GetAllowedTransitions(string? currentStatus)
    {
        if(currentStatus != null && AllowedTransitions.TryGetValue(currentStatus, out var nextStates))
        {
            return nextStates;
        }

        return [];
    }
}