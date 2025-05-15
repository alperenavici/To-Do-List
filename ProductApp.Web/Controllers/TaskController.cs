using Microsoft.AspNetCore.Mvc;
using ProductApp.Core;
using ProductApp.Entity;

namespace ProductApp.Web.Controllers;

[ApiController]
[Route("api/tasks")]
public class TodoTasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TodoTasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_taskService.GetAll());

    [HttpPost]
    public IActionResult Add([FromBody] TodoTask task)
    {
        _taskService.Add(task);
        return Ok();
    }

    [HttpPost("{id}/Complete")]
    public IActionResult Complete(int id)
    {
        _taskService.CompleteTask(id);
        return Ok();
    }
}