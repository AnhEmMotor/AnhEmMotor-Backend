using Domain.Constants;
using Domain;

namespace Domain.Constants.Input;

public static class InputStatusTransitions
{
    private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new()
    {
        { Input.InputStatus.Working, [ Input.InputStatus.Finish, Input.InputStatus.Cancel ] },
        { Input.InputStatus.Finish, [] },
        { Input.InputStatus.Cancel, [] }
    };

    public static bool IsTransitionAllowed(string? currentStatus, string? newStatus)
    { throw new NotImplementedException(); }

    public static HashSet<string> GetAllowedTransitions(string? currentStatus) { throw new NotImplementedException(); }
}
