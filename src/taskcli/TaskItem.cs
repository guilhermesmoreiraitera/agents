namespace Taskcli;

/// <summary>
/// The lifecycle status of a <see cref="TaskItem"/>.
/// </summary>
public enum TaskState
{
    Pending,
    Completed
}

/// <summary>
/// A single task tracked by taskcli.
/// </summary>
public class TaskItem
{
    public int Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public TaskState Status { get; set; } = TaskState.Pending;

    public DateTime CreatedAtUtc { get; set; }
}
