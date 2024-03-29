using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes.Application.CommandsQueries.Tasks.Commands.CreateTask;
using Notes.Application.Interfaces;
using Notes.Domain;
using Notes.MVC.Models;
using Notes.MVC.Services;

namespace Notes.MVC.Controllers.Tasks;

public class TasksController : BaseController
{
    private readonly TaskService _taskService;
    private readonly INotesDbContext _context;

    public TasksController(TaskService taskService, INotesDbContext context)
    {
        _taskService = taskService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var listTasks = (await _taskService.GetList(UserId))
            .Task
            .Where(t => t.isDeleted == false)
            .ToList();

        return PartialView("_TasksPartial", listTasks);
    }

    [HttpGet]
    public async Task<IActionResult> EditPage(Guid id)
    {
        var task = await _taskService.Get(id);
        var subTasks = await _taskService.GetSubTasks(id);

        var vm = new CombineTaskSubTasks
        {
            TaskVm = task,
            SubTasks = subTasks.OrderBy(s => s.CreateDate).ToList()
        };

        if (task.isCompleted)
            ViewBag.Completed = "Задача выполнена";
        else
            ViewBag.Completed = "Задача не выполнена";

        return PartialView("~/Views/Tasks/_TaskEditPartial.cshtml", vm);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CombineTaskSubTasks vm)
    {
        await _taskService.Edit(vm, UserId);

        return RedirectToAction("EditPage", "Tasks", new { id = vm.TaskVm.Id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSubTask([FromForm] Guid subtaskid, [FromForm] Guid taskid)
    {
        var subtask = await _context.SubTasks
            .FirstOrDefaultAsync(s => s.Id == subtaskid);

        _context.SubTasks.Remove(subtask);
        await _context.SaveChangesAsync(cancellationToken: CancellationToken.None);

        return RedirectToAction("EditPage", "Tasks", new { id = taskid });
    }

    [HttpPost]
    public async Task<IActionResult> CheckSubtask([FromForm] Guid subtaskid, [FromForm] Guid taskid)
    {
        var subtask = await _context.SubTasks
            .FirstOrDefaultAsync(s => s.Id == subtaskid);

        if (subtask.isChecked)
            subtask.isChecked = false;
        else
            subtask.isChecked = true;

        await CompleteTask(taskid);

        _context.SubTasks.Update(subtask);
        await _context.SaveChangesAsync(cancellationToken: CancellationToken.None);
        
        ViewBag.Completed = "Выполнено";
        
        return RedirectToAction("EditPage", "Tasks", new { id = taskid });
    }

    [HttpPost]
    public async Task<IActionResult> EditSubTask([FromForm] Guid subtaskid, [FromForm] Guid taskid, string text)
    {
        var subtask = await _context.SubTasks
            .FirstOrDefaultAsync(s => s.Id == subtaskid);
        
        subtask.Text = text;

        _context.SubTasks.Update(subtask);
        await _context.SaveChangesAsync(cancellationToken: CancellationToken.None);
        
        return RedirectToAction("EditPage", "Tasks", new { id = taskid });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubTask([FromForm] Guid taskid, string text)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(s => s.Id == taskid);
        
        var subtask = new SubTask
        {
            isChecked = false,
            Tasks = task,
            CreateDate = DateTime.Now.ToUniversalTime(),
            Text = text
        };

        await _context.SubTasks.AddAsync(subtask);
        await _context.SaveChangesAsync(CancellationToken.None);

        return RedirectToAction("EditPage", "Tasks", new { id = taskid });
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(string taskName)
    {
        var query = new CreateTaskCommand()
        {
            UserId = UserId,
            Title = taskName
        };

        await Mediator.Send(query);
        
        return RedirectToAction("Index", "Tasks");
    }

    [HttpPost]
    public async Task<IActionResult> ToTrash(Guid id)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(s => s.Id == id);
        
        task.isDeleted = true;
        
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(CancellationToken.None);

        return RedirectToAction("Index", "Tasks");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(i => i.Id == id);

        _context.Tasks.Remove(task);
        
        await _context.SaveChangesAsync(CancellationToken.None);
        
        return RedirectToAction("TaskTrash", "Trash");
    }
    
    [HttpPost]
    public async Task<IActionResult> RestoreFromTrash(Guid id)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(i => i.Id == id);

        task.isDeleted = false;
        
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(CancellationToken.None);

        return RedirectToAction("TaskTrash", "Trash");
    }
    
    [HttpPost]
    public async Task CompleteTask(Guid id)
    {
        var completedSubtasks = (await _taskService.GetSubTasks(id))
            .Where(i => i.isChecked)
            .ToList();
        
        var task = await _context.Tasks
            .FirstOrDefaultAsync(i => i.Id == id);

        var taskSubtasks = await _taskService.GetSubTasks(id);

        if (completedSubtasks.Count < taskSubtasks.Count)
            task.isCompleted = false;
        else if(completedSubtasks.Count == taskSubtasks.Count)
            task.isCompleted = true;
        
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(CancellationToken.None);
    }
}