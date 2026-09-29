using System.Text.Json;
using System.Text.Json.Serialization;

namespace Taskcli;

/// <summary>
/// Reads and writes the taskcli JSON storage file.
/// </summary>
public class TaskRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _filePath;

    public TaskRepository(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }

        _filePath = filePath;
    }

    /// <summary>
    /// Loads the current store from disk. If the file does not exist yet,
    /// an empty store is returned in memory and nothing is created on disk.
    /// </summary>
    public TaskStore Load()
    {
        if (!File.Exists(_filePath))
        {
            return new TaskStore();
        }

        string json;
        try
        {
            json = File.ReadAllText(_filePath);
        }
        catch (IOException ex)
        {
            throw new TaskStorageException($"Unable to read task storage file at '{_filePath}'.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new TaskStorageException($"Unable to read task storage file at '{_filePath}'.", ex);
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new TaskStorageException($"Task storage file at '{_filePath}' is empty or corrupt.");
        }

        try
        {
            var store = JsonSerializer.Deserialize<TaskStore>(json, SerializerOptions);
            if (store is null)
            {
                throw new TaskStorageException($"Task storage file at '{_filePath}' is corrupt.");
            }

            return store;
        }
        catch (JsonException ex)
        {
            throw new TaskStorageException($"Task storage file at '{_filePath}' is corrupt or unreadable.", ex);
        }
    }

    /// <summary>
    /// Persists the given store to disk atomically: writes to a temp file
    /// in the same directory, then replaces the real file so a process
    /// killed mid-write cannot corrupt existing data.
    /// </summary>
    public void Save(TaskStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string tempFilePath = Path.Combine(
            string.IsNullOrEmpty(directory) ? "." : directory,
            $".{Path.GetFileName(_filePath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            string json = JsonSerializer.Serialize(store, SerializerOptions);
            File.WriteAllText(tempFilePath, json);
            File.Move(tempFilePath, _filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new TaskStorageException($"Unable to write task storage file at '{_filePath}'.", ex);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}
