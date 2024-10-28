using Microsoft.EntityFrameworkCore;
using TaskBlaster.TaskManagement.DAL.Data;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.Exceptions;
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

    public async Task<int> CreateNewTaskAsync(TaskInputModel task, string userEmail)
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

        var createdById = (await _dbContext.Users.FirstOrDefaultAsync(u => u.EmailAddress == userEmail))?.Id ?? -1;
        // this if statement should never be true, since the user will always be created in the db becaue of onTokenValidation
        // but the complier complains because it can possible be null so i used -1 instead
        if (createdById == -1) throw new BadRequestException("The authenticated user was not found.");

        var newTask = new Entities.Task
        {
            Title = task.Title,
            Description = task.Description,
            StatusId = task.StatusId ?? 0,
            PriorityId = task.PriorityId ?? 0,
            DueDate = task.DueDate,
            AssignedToId = userId,
            CreatedById = createdById
        };

        await _dbContext.AddAsync(newTask);
        await _dbContext.SaveChangesAsync();
        return newTask.Id;
    }

    public async Task<Envelope<TaskDto>> GetPaginatedTasksByCriteriaAsync(TaskCriteriaQueryParams query)
    {
        var taskQuery = _dbContext.Tasks.AsQueryable();

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

        return new Envelope<TaskDto>(query.PageNumber, query.PageSize, tasks);
    }


    public async Task<TaskDetailsDto?> GetTaskByIdAsync(int taskId)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .Include(t => t.AssignedTo)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == taskId);
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
            Tags = task.TaskTags.Select(tt => tt.Tag.Name).ToList() ?? [],
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