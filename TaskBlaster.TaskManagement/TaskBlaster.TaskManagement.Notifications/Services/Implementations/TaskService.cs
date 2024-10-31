using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Notifications.Services.Interfaces;

namespace TaskBlaster.TaskManagement.Notifications.Services.Implementations;

public class TaskService(ITaskRepository taskRepository) : ITaskService
{
    public async Task<IEnumerable<TaskWithNotificationDto>> GetTasksForNotifications()
    {
        return await taskRepository.GetTasksForNotifications();
    }

    public async Task UpdateTaskNotifications()
    {
        await taskRepository.UpdateTaskNotifications();
    }
}