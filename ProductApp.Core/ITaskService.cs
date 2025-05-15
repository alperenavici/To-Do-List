using ProductApp.Entity;
namespace ProductApp.Core;

public interface ITaskService
{
    List<TodoTask> GetAll();
    TodoTask GetById(int id);
    void Add(TodoTask task);
    void CompleteTask(int id);
}
