using Taskcli;
using Xunit;

namespace Taskcli.Tests;

public class TaskRepositoryTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _storagePath;

    public TaskRepositoryTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _storagePath = Path.Combine(_tempDirectory, "tasks.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Fact]
    public void Load_WhenFileMissing_ReturnsEmptyStoreWithoutCreatingFile()
    {
        var repository = new TaskRepository(_storagePath);

        TaskStore store = repository.Load();

        Assert.Empty(store.Tasks);
        Assert.Equal(1, store.NextId);
        Assert.False(File.Exists(_storagePath));
        Assert.False(Directory.Exists(_tempDirectory));
    }

    [Fact]
    public void SaveThenLoad_RoundTripsTasksAndNextId()
    {
        var repository = new TaskRepository(_storagePath);
        var store = new TaskStore
        {
            NextId = 3,
            Tasks =
            {
                new TaskItem { Id = 1, Description = "First", Status = TaskState.Pending, CreatedAtUtc = DateTime.UtcNow },
                new TaskItem { Id = 2, Description = "Second", Status = TaskState.Completed, CreatedAtUtc = DateTime.UtcNow }
            }
        };

        repository.Save(store);
        TaskStore loaded = repository.Load();

        Assert.True(File.Exists(_storagePath));
        Assert.Equal(3, loaded.NextId);
        Assert.Equal(2, loaded.Tasks.Count);
        Assert.Equal("First", loaded.Tasks[0].Description);
        Assert.Equal(TaskState.Pending, loaded.Tasks[0].Status);
        Assert.Equal("Second", loaded.Tasks[1].Description);
        Assert.Equal(TaskState.Completed, loaded.Tasks[1].Status);
    }

    [Fact]
    public void Load_WhenFileIsCorruptJson_ThrowsTaskStorageException()
    {
        Directory.CreateDirectory(_tempDirectory);
        File.WriteAllText(_storagePath, "{ not valid json ");

        var repository = new TaskRepository(_storagePath);

        Assert.Throws<TaskStorageException>(() => repository.Load());
    }

    [Fact]
    public void Load_WhenFileIsEmpty_ThrowsTaskStorageException()
    {
        Directory.CreateDirectory(_tempDirectory);
        File.WriteAllText(_storagePath, string.Empty);

        var repository = new TaskRepository(_storagePath);

        Assert.Throws<TaskStorageException>(() => repository.Load());
    }

    [Fact]
    public void NextId_IncrementsAcrossMultipleSaves()
    {
        var repository = new TaskRepository(_storagePath);

        TaskStore store = repository.Load();
        store.Tasks.Add(new TaskItem { Id = store.NextId, Description = "One", CreatedAtUtc = DateTime.UtcNow });
        store.NextId++;
        repository.Save(store);

        store = repository.Load();
        store.Tasks.Add(new TaskItem { Id = store.NextId, Description = "Two", CreatedAtUtc = DateTime.UtcNow });
        store.NextId++;
        repository.Save(store);

        TaskStore final = repository.Load();

        Assert.Equal(3, final.NextId);
        Assert.Equal(new[] { 1, 2 }, final.Tasks.Select(t => t.Id));
    }
}
