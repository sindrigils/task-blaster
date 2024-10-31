using Microsoft.EntityFrameworkCore;
using TaskBlaster.TaskManagement.DAL.Data;
using TaskBlaster.TaskManagement.DAL.Entities;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.Exceptions;
using TaskBlaster.TaskManagement.Models.InputModels;
using Task = System.Threading.Tasks.Task;

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
        return;
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
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };


        await _dbContext.AddAsync(newTask);
        await _dbContext.SaveChangesAsync();

        if (task.DueDate.HasValue)
        {
            var notification = new TaskNotification
            {
                TaskId = newTask.Id,
                DueDateNotificationSent = false,
                DaysAfterNotificationSent = false,
            };
            await _dbContext.AddAsync(notification);
            await _dbContext.SaveChangesAsync();
        }

        return newTask.Id;
    }

    public async Task<Envelope<TaskDto>> GetPaginatedTasksByCriteriaAsync(TaskCriteriaQueryParams query)
    {
        var taskQuery = _dbContext.Tasks.AsQueryable();

        if (!string.IsNullOrEmpty(query.SearchValue))
        {
            var searchValueLower = query.SearchValue.ToLower();
            taskQuery = taskQuery.Where(t => EF.Functions.Like(t.Title.ToLower(), $"%{searchValueLower}%"));
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
            .Include(t => t.CreatedBy)
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
            CreatedBy = task.CreatedBy.FullName,
            AssignedToUser = task.AssignedTo?.FullName ?? "",
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

        // filter out the tasks that have sent both DueDateNotification and DaysAfterNotification
        var tasks = await _dbContext.Tasks
            .Include(t => t.TaskNotification)
            .Where(t => t.DueDate <= currentDate &&
                        t.TaskNotification != null && !(t.TaskNotification.DueDateNotificationSent && t.TaskNotification.DaysAfterNotificationSent))
            .Select(t => new TaskWithNotificationDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status.Name,
                DueDate = t.DueDate,
                AssignedToUser = t.AssignedTo != null ? t.AssignedTo.FullName : "",
                Notification = new TaskNotificationDto
                {
                    // This is safe since we filtered out the nulls and since the due date is set then there must be a TaskNotification model in the db
                    Id = t.TaskNotification!.Id,
                    DueDateNotificationSent = t.TaskNotification.DueDateNotificationSent,
                    DayAfterNotificationSent = t.TaskNotification.DaysAfterNotificationSent,
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
        // so the idea here is, when this function is called it checks if there are any tasks that have DueDate today
        // and have not set DueDateNotificationSent as true then set it as true and also check if there are any tasks
        // with due date yesterday which have not set DaysAfterNotificationSent as true then set it as true
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        var dueTodayNotifications = await _dbContext.TaskNotifications
            .Where(tn => tn.Task.DueDate.HasValue &&
                         tn.Task.DueDate.Value.Date == today &&
                         !tn.DueDateNotificationSent)
            .ToListAsync();

        var dueYesterdayNotifications = await _dbContext.TaskNotifications
            .Where(tn => tn.Task.DueDate.HasValue &&
                         tn.Task.DueDate.Value.Date == yesterday &&
                         tn.DueDateNotificationSent &&
                         !tn.DaysAfterNotificationSent)
            .ToListAsync();

        foreach (var notification in dueTodayNotifications)
        {
            notification.DueDateNotificationSent = true;
        }

        foreach (var notification in dueYesterdayNotifications)
        {
            notification.DaysAfterNotificationSent = true;
        }

        await _dbContext.SaveChangesAsync();
    }



    public async Task UpdateTaskPriorityAsync(int taskId, PriorityInputModel inputModel)
    {
        var task = await GetTaskAsync(taskId);
        if (task == null) return;

        // now since the priorityId in the inputmodel is nullable (in order to not get 0 as the default value) I need to use ??
        task.PriorityId = inputModel.PriorityId ?? task.PriorityId;
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateTaskStatusAsync(int taskId, StatusInputModel inputModel)
    {
        var task = await GetTaskAsync(taskId);
        if (task == null) return;

        // now since the stausId in the inputmodel is nullable (in order to not get 0 as the default value) I need to use ??
        task.StatusId = inputModel.StatusId ?? task.StatusId;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsUserAssigned(int taskId, int userId)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        // safe to use "!" since i have confirmed that this task exists before calling this function
        return task!.AssignedToId == userId;
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