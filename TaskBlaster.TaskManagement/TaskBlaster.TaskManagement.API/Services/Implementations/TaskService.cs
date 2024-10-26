using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.Exceptions;
using TaskBlaster.TaskManagement.Models.InputModels;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class TaskService(ITaskRepository taskRepository, IUserRepository userRepository) : ITaskService
{
    public async Task<Envelope<TaskDto>> GetPaginatedTasksByCriteriaAsync(TaskCriteriaQueryParams query) => await taskRepository.GetPaginatedTasksByCriteriaAsync(query);

    public async Task<TaskDetailsDto?> GetTaskByIdAsync(int taskId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        var task = await taskRepository.GetTaskByIdAsync(taskId) ?? throw new ResourceNotFoundException($"No task with id {taskId} found");
        return task;
    }

    public async Task<int> CreateNewTaskAsync(TaskInputModel task) => await taskRepository.CreateNewTaskAsync(task);

    public async Task ArchiveTaskByIdAsync(int taskId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        var taskExist = await taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExist)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found");
        }
        await taskRepository.ArchiveTaskByIdAsync(taskId);
    }

    public async Task AssignUserToTaskAsync(int taskId, int userId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 1);

        var taskExist = await taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExist)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found");
        }

        var userExist = await userRepository.DoesUserExistAsync(taskId);
        if (!userExist)
        {
            throw new ResourceNotFoundException($"No user with id {userId} found");
        }

        await taskRepository.AssignUserToTaskAsync(taskId, userId);
    }

    public async Task UnassignUserFromTaskAsync(int taskId, int userId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 1);

        var taskExist = await taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExist)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found");
        }

        var userExist = await userRepository.DoesUserExistAsync(taskId);
        if (!userExist)
        {
            throw new ResourceNotFoundException($"No user with id {userId} found");
        }

        await taskRepository.UnassignUserFromTaskAsync(taskId, userId);
    }

    public async Task UpdateTaskStatusAsync(int taskId, StatusInputModel inputModel)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        var taskExist = await taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExist)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found");
        }
    }

    public async Task UpdateTaskPriorityAsync(int taskId, PriorityInputModel inputModel)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        var taskExist = await taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExist)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found");
        }
        await taskRepository.UpdateTaskPriorityAsync(taskId, inputModel);
    }
}