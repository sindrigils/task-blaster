using Microsoft.EntityFrameworkCore;
using TaskBlaster.TaskManagement.DAL.Data;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.InputModels;

namespace TaskBlaster.TaskManagement.DAL.Implementations;

public class TaskRepository(TaskBlasterDbContext dbContext) : ITaskRepository
{
    private readonly TaskBlasterDbContext _dbContext = dbContext;
    public async Task ArchiveTaskByIdAsync(int taskId)
    {
        var task = await GetTaskAsync(taskId);
        if (task == null) return;

        task.IsArchived = true;
        await _dbContext.SaveChangesAsync();
    }

    public async Task AssignUserToTaskAsync(int taskId, int userId)
    {
        var task = await GetTaskAsync(taskId);
        if (task == null) return;

        task.AssignedToId = userId;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> CreateNewTaskAsync(TaskInputModel task)
    {
        int? userId = null;

        if (task.AssignedToUser != null)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.FullName == task.AssignedToUser);
            if (user != null)
            {
                userId = user.Id;
            }
        }
        var newTask = new Entities.Task
        {
            Title = task.Title,
            Description = task.Description,
            StatusId = task.StatusId,
            PriorityId = task.PriorityId,
            DueDate = task.DueDate,
            AssignedToId = userId
        };
        await _dbContext.AddAsync(newTask);
        await _dbContext.SaveChangesAsync();
        return newTask.Id;
    }

    public async Task<Envelope<TaskDto>> GetPaginatedTasksByCriteriaAsync(TaskCriteriaQueryParams query)
    {
        // Validate the input parameters if necessary

        // Create a base query for tasks
        var taskQuery = _dbContext.Tasks.AsQueryable();

        // Apply search filter if SearchValue is provided
        if (!string.IsNullOrEmpty(query.SearchValue))
        {
            taskQuery = taskQuery.Where(t => t.Title.Contains(query.SearchValue) ||
                                               (t.Description ?? string.Empty).Contains(query.SearchValue));
        }

        // Get all matching tasks
        var tasks = await taskQuery
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status.Name,
                DueDate = t.DueDate,
                AssignedToUser = t.AssignedTo != null ? t.AssignedTo.FullName : ""
            })
            .ToListAsync();

        return new Envelope<TaskDto>
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            MaxCount = tasks.Count,
            Items = tasks
        };
    }


    public async Task<TaskDetailsDto?> GetTaskByIdAsync(int taskId)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null) return null;

        return new TaskDetailsDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.Name,
            Priority = task.Priority.Name,
            CreatedAt = task.CreatedAt,
            DueDate = task.DueDate,
            AssignedToUser = task.AssignedTo != null ? task.AssignedTo.FullName : "",
            Tags = task.TaskTags.Select(tt => tt.Tag.Name).ToList(),
            Comments = task.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Author = c.Author,
                ContentAsMarkdown = c.ContentAsMarkdown,
                CreatedDate = c.CreatedDate
            }).ToList()
        };
    }

    public async Task<IEnumerable<TaskWithNotificationDto>> GetTasksForNotifications()
    {
        var currentDate = DateTime.UtcNow;

        var tasks = await _dbContext.Tasks
            .Include(t => t.TaskNotification)
            .Where(t => t.DueDate <= currentDate &&
                        t.TaskNotification != null &&
                        !t.TaskNotification.DueDateNotificationSent)
            .Select(t => new TaskWithNotificationDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status.Name,
                DueDate = t.DueDate,
                AssignedToUser = t.AssignedTo != null ? t.AssignedTo.FullName : "",
                Notification = new TaskNotificationDto
                {
                    Id = t.TaskNotification.Id,  // This is safe since we filtered out nulls
                    DueDateNotificationSent = t.TaskNotification.DueDateNotificationSent,
                    LastNotificationDate = t.TaskNotification.LastNotificationDate
                }
            })
            .ToListAsync();

        return tasks;
    }


    public async Task UnassignUserFromTaskAsync(int taskId, int userId)
    {
        var task = await GetTaskAsync(taskId);
        if (task == null) return;

        task.AssignedToId = null;
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateTaskNotifications()
    {
        var notificationsToUpdate = await _dbContext.TaskNotifications
            .Where(n => !n.DueDateNotificationSent)
            .ToListAsync();

        foreach (var notification in notificationsToUpdate)
        {
            notification.DueDateNotificationSent = true;
            notification.LastNotificationDate = DateTime.UtcNow;
        }

        // Save changes to the database
        await _dbContext.SaveChangesAsync();
    }


    public async Task UpdateTaskPriorityAsync(int taskId, PriorityInputModel inputModel)
    {
        var task = await GetTaskAsync(taskId);
        if (task == null) return;

        task.PriorityId = inputModel.PriorityId;
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateTaskStatusAsync(int taskId, StatusInputModel inputModel)
    {
        var task = await GetTaskAsync(taskId);
        if (task == null) return;

        task.StatusId = inputModel.StatusId;
        await _dbContext.SaveChangesAsync();
    }

    private async Task<Entities.Task?> GetTaskAsync(int taskId)
    {
        return await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
    }

    public async Task<bool> DoesTaskExistAsync(int taskId)
    {
        return await GetTaskAsync(taskId) != null;
    }

}