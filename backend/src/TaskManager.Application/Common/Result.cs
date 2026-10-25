namespace TaskManager.Application.Common;

public class Result
{
    protected Result(Error? error)
    {
    }

    public bool IsSuccess => false;

    public Error? Error => null;

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);
}
