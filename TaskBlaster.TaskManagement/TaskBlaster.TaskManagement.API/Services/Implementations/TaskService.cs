using System.Security.Claims;
using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.Exceptions;
using TaskBlaster.TaskManagement.Models.InputModels;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class TaskService : ITaskService
{

    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStatusRepository _statusRepository;
    private readonly IPriorityRepository _priorityRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly INotificationService _notificationService;

    public TaskService(
    ITaskRepository taskRepository,
    IUserRepository userRepository,
    IStatusRepository statusRepository,
    IPriorityRepository priorityRepository,
    IHttpContextAccessor httpContextAccessor,
    INotificationService notificationService
    )
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _statusRepository = statusRepository;
        _priorityRepository = priorityRepository;
        _httpContextAccessor = httpContextAccessor;
        _notificationService = notificationService;
    }

    public async Task<Envelope<TaskDto>> GetPaginatedTasksByCriteriaAsync(TaskCriteriaQueryParams query) => await _taskRepository.GetPaginatedTasksByCriteriaAsync(query);

    public async Task<TaskDetailsDto?> GetTaskByIdAsync(int taskId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        var task = await _taskRepository.GetTaskByIdAsync(taskId) ?? throw new ResourceNotFoundException($"No task with id {taskId} found");
        return task;
    }

    public async Task<int> CreateNewTaskAsync(TaskInputModel task)
    {
        var assignedToUser = task.AssignedToUser;
        if (assignedToUser != null)
        {
            var exists = await _userRepository.DoesUserExistAsync(assignedToUser);
            if (!exists) throw new BadRequestException($"No user found with name {assignedToUser} found");
        }

        var statusExist = await _statusRepository.DoesStatusExistAsync(task.StatusId ?? 0);
        if (!statusExist) throw new BadRequestException($"No status found with id {task.StatusId} found");

        var priorityExist = await _priorityRepository.DoesPriorityExistAsync(task.PriorityId ?? 0);
        if (!priorityExist) throw new BadRequestException($"No priority found with id {task.PriorityId} found");

        var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst("email_address")?.Value ?? "";

        return await _taskRepository.CreateNewTaskAsync(task, userEmail);
    }

    public async Task ArchiveTaskByIdAsync(int taskId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        var taskExist = await _taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExist)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found");
        }
        await _taskRepository.ArchiveTaskByIdAsync(taskId);
    }

    public async Task AssignUserToTaskAsync(int taskId, int userId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 1);

        var taskExist = await _taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExist)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found");
        }

        var userExist = await _userRepository.DoesUserExistAsync(userId);
        if (!userExist)
        {
            throw new ResourceNotFoundException($"No user with id {userId} found");
        }

        await _taskRepository.AssignUserToTaskAsync(taskId, userId);
        await _notificationService.SendAssignedNotification(userId, taskId);
    }

    public async Task UnassignUserFromTaskAsync(int taskId, int userId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 1);

        var taskExist = await _taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExist)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found");
        }

        var userExist = await _userRepository.DoesUserExistAsync(userId);
        if (!userExist)
        {
            throw new ResourceNotFoundException($"No user with id {userId} found");
        }

        await _taskRepository.UnassignUserFromTaskAsync(taskId, userId);
        await _notificationService.SendUnassignedNotification(userId, taskId);
    }

    public async Task UpdateTaskStatusAsync(int taskId, StatusInputModel inputModel)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        var taskExist = await _taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExist)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found");
        }

        var statusExist = await _statusRepository.DoesStatusExistAsync(inputModel.StatusId);
        if (!statusExist) throw new BadRequestException($"No status found with id {inputModel.StatusId} found");
        await _taskRepository.UpdateTaskStatusAsync(taskId, inputModel);
    }

    public async Task UpdateTaskPriorityAsync(int taskId, PriorityInputModel inputModel)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        var taskExist = await _taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExist)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found");
        }

        var priorityExist = await _priorityRepository.DoesPriorityExistAsync(inputModel.PriorityId);
        if (!priorityExist) throw new BadRequestException($"No priority found with id {inputModel.PriorityId} found");
        await _taskRepository.UpdateTaskPriorityAsync(taskId, inputModel);
    }
}