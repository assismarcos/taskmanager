namespace TaskManager.Application.Common;

public sealed class Result<T> : Result
{
    private Result(Error? error) : base(error)
    {
    }

    public T Value => default!;

    public static Result<T> Success(T value) => new((Error?)null);

    public static new Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
