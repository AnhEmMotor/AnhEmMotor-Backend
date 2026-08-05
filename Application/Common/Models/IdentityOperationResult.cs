namespace Application.Common.Models;

public sealed class IdentityOperationResult
{
    public bool Succeeded { get; private init; }

    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static IdentityOperationResult Success() => new() { Succeeded = true };

    public static IdentityOperationResult Failure(IEnumerable<string> errors) => new()
    {
        Succeeded = false,
        Errors = errors.ToList()
    };
}
