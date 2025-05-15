using ProductApp.Entity;

namespace ProductApp.Data;

public class FakeTaskRepository
{
    private static List<TodoTask> _tasks = new()
    {
       
    };

    public List<TodoTask> GetAll() => _tasks;

    public TodoTask GetById(int id) => _tasks.FirstOrDefault(t => t.Id == id);

    public void Add(TodoTask task)
    {
        task.Id = _tasks.Any() ? _tasks.Max(t => t.Id) + 1 : 1;
        _tasks.Add(task);
    }

    public void CompleteTask(int id)
    {
        var task = GetById(id);
        if (task != null) task.IsCompleted = true;
    }
}
