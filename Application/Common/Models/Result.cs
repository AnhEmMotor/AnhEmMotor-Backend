
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

#pragma warning disable CRR0047
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

    private Result(T value): base(true)
    {
        _value = value;
    }

    private Result(Error error): base(false, [error])
    {
        _value = default;
    }

    private Result(List<Error> errors): base(false, errors)
    {
        _value = default;
    }

    public static Result<T> Success(T value) => new(value);

    public new static Result<T> Failure(Error error) => new(error);

    public new static Result<T> Failure(List<Error> errors) => new(errors);

    public new static Result<T> Failure(string errorMessage) => new(Error.Failure(errorMessage));

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
    public static implicit operator Result<T>(List<Error> errors) => Failure(errors);
}
#pragma warning restore CRR0047