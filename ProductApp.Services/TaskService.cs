using ProductApp.Core;
using ProductApp.Data;
using ProductApp.Entity;

namespace ProductApp.Services;

public class TaskService : ITaskService
{
    private readonly FakeTaskRepository _repo = new();

    public List<TodoTask> GetAll() => _repo.GetAll();

    public TodoTask GetById(int id) => _repo.GetById(id);

    public void Add(TodoTask task) => _repo.Add(task);

    public void CompleteTask(int id) => _repo.CompleteTask(id);
}