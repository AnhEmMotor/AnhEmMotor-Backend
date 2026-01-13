namespace Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public List<Error>? Errors { get; }

    protected Result(bool isSuccess, Error? error = null, List<Error>? errors = null)
    {
        if(isSuccess && (error is not null || errors is not null))
        {
            throw new InvalidOperationException("A successful result cannot have errors.");
        }
        if(!isSuccess && error is null && (errors is null || errors.Count == 0))
        {
            throw new InvalidOperationException("A failed result must have at least one error.");
        }
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors;
    }

    public static Result Success() { return new Result(true); }
    public static Result Failure(Error error) { return new Result(false, error); }
    public static Result Failure(List<Error> errors) { return new Result(false, errors: errors); }
    public static Result Failure(string errorMessage) { return new Result(false, Error.Failure(errorMessage)); }
}
