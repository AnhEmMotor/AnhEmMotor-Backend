
namespace Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public List<Error> Errors { get; }

    public Error? Error => Errors.FirstOrDefault();

    protected Result(bool isSuccess, List<Error>? errors = null)
    {
        if (isSuccess && errors?.Count > 0)
        {
            throw new InvalidOperationException("A successful result cannot have errors.");
        }
        if (!isSuccess && (errors == null || errors.Count == 0))
        {
            throw new InvalidOperationException("A failed result must have at least one error.");
        }
        IsSuccess = isSuccess;
        Errors = errors ?? [];
    }

    public static Result Success() => new(true);

    public static Result Failure(Error error) => new(false, [error]);

    public static Result Failure(List<Error> errors) => new(false, errors);

    public static Result Failure(string errorMessage) => new(false, [Error.Failure(errorMessage)]);
}
