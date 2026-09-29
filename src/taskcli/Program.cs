namespace Taskcli;

/// <summary>
/// CLI entry point. Parses the verb and delegates to <see cref="TaskService"/>;
/// contains no business logic itself.
/// </summary>
public class Program
{
    public static int Main(string[] args)
    {
        string defaultStoragePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".taskcli",
            "tasks.json");

        return Run(args, defaultStoragePath);
    }

    /// <summary>
    /// Runs the CLI against the given storage path. Exposed separately from
    /// <see cref="Main"/> so tests can point it at a temp file.
    /// </summary>
    public static int Run(string[] args, string storagePath)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: taskcli <command> [arguments]");
            return 1;
        }

        var repository = new TaskRepository(storagePath);
        var service = new TaskService(repository);

        string verb = args[0];
        switch (verb)
        {
            case "add":
                return HandleAdd(service, args);
            default:
                Console.Error.WriteLine($"Unrecognized command: '{verb}'. Usage: taskcli <command> [arguments]");
                return 1;
        }
    }

    private static int HandleAdd(TaskService service, string[] args)
    {
        string description = string.Join(" ", args[1..]);

        try
        {
            TaskItem task = service.Add(description);
            Console.WriteLine($"{task.Id}: {task.Description}");
            return 0;
        }
        catch (TaskValidationException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (TaskStorageException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
