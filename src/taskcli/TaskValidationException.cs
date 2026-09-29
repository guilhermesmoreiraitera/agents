namespace Taskcli;

/// <summary>
/// Thrown when input to a <see cref="TaskService"/> operation fails
/// validation (e.g. an empty task description).
/// </summary>
public class TaskValidationException : Exception
{
    public TaskValidationException(string message) : base(message)
    {
    }
}
