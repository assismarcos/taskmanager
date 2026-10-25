namespace TaskManager.Application.Abstractions;

public interface ICurrentUser
{
    Guid UserId { get; }
}
