namespace Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }
    public List<Error>? Errors { get; }
    protected Result(bool isSuccess, Error? error = null, List<Error>? errors = null)
    {
        if (isSuccess && (error is not null || errors is not null))
        {
            throw new InvalidOperationException("A successful result cannot have errors.");
        }
        if (!isSuccess && error is null && (errors is null || errors.Count == 0))
        {
            throw new InvalidOperationException("A failed result must have at least one error.");
        }
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors;
    }
    public static Result Success()
    {
        return new Result(true);
    }
    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }
    public static Result Failure(List<Error> errors)
    {
        return new Result(false, errors: errors);
    }
    public static Result Failure(string errorMessage)
    {
        return new Result(false, Error.Failure(errorMessage));
    }
}

public class Result<T> : Result
{
    private readonly T? _value;
    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("Cannot access the value of a failed result.");
            }
            return _value!;
        }
    }
    private Result(T value) : base(true)
    {
        _value = value;
    }
    private Result(Error error) : base(false, error)
    {
        _value = default;
    }
    private Result(List<Error> errors) : base(false, errors: errors)
    {
        _value = default;
    }
    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }
    public new static Result<T> Failure(Error error)
    {
        return new Result<T>(error);
    }
    public new static Result<T> Failure(List<Error> errors)
    {
        return new Result<T>(errors);
    }
    public new static Result<T> Failure(string errorMessage)
    {
        return new Result<T>(Error.Failure(errorMessage));
    }
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }
    public static implicit operator Result<T>(Error error)
    {
        return Failure(error);
    }
    public static implicit operator Result<T>(List<Error> errors)
    {
        return Failure(errors);
    }
}