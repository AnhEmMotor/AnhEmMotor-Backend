namespace Application.Common.Models;

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

    private Result(T value) : base(true) { _value = value; }
    private Result(Error error) : base(false, error) { _value = default; }
    private Result(List<Error> errors) : base(false, errors: errors) { _value = default; }
    public static Result<T> Success(T value) { return new Result<T>(value); }
    public new static Result<T> Failure(Error error) { return new Result<T>(error); }
    public new static Result<T> Failure(List<Error> errors) { return new Result<T>(errors); }
    public new static Result<T> Failure(string errorMessage) { return new Result<T>(Error.Failure(errorMessage)); }
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