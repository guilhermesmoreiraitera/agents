namespace Taskcli;

/// <summary>
/// Thrown when the task storage file exists but cannot be read/parsed
/// (e.g. corrupt JSON), or cannot be written.
/// </summary>
public class TaskStorageException : Exception
{
    public TaskStorageException(string message) : base(message)
    {
    }

    public TaskStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
