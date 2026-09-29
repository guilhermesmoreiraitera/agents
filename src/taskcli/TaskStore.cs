namespace Taskcli;

/// <summary>
/// The full on-disk representation of taskcli's state: a monotonic ID
/// counter plus the list of tasks. The counter is persisted (rather than
/// derived from existing task IDs) so IDs are never reused, even after
/// tasks are removed.
/// </summary>
public class TaskStore
{
    public int NextId { get; set; } = 1;

    public List<TaskItem> Tasks { get; set; } = new();
}
