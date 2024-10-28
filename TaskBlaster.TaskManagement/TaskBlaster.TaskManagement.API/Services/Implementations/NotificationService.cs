using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Exceptions;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private const int AssignTemplateId = 6418991;
    private const int UnassignTemplateId = 6418994;

    public NotificationService(HttpClient httpClient, ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _httpClient = httpClient;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task SendAssignedNotification(int userId, int taskId)
    {
        await SendNotification(userId, taskId, "You have been assigned to a new task!", AssignTemplateId);
    }

    public async Task SendUnassignedNotification(int userId, int taskId)
    {
        await SendNotification(userId, taskId, "You have been unassigned from a task", UnassignTemplateId);
    }

    private async Task SendNotification(int userId, int taskId, string subject, int templateId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(taskId) ?? throw new ResourceNotFoundException($"No task with id {taskId} found");
        var user = await _userRepository.GetUserByIdAsync(userId) ?? throw new ResourceNotFoundException($"No user with id {userId} found");

        var requestBody = CreateRequestBody(user.EmailAddress, subject, templateId, BuildTemplateVariables(user.FullName, task.Title, task.Description ?? "", task.DueDate));
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("http://localhost:5165/api/notifications/emails/template", content);
        response.EnsureSuccessStatusCode();
    }

    private object CreateRequestBody(string email, string subject, int templateId, Dictionary<string, object> variables)
    {
        return new
        {
            to = email,
            subject,
            templateId,
            variables
        };
    }

    private Dictionary<string, object> BuildTemplateVariables(string name, string title, string description, DateTime? dueDate)
    {
        return new Dictionary<string, object>
        {
            {"name", name},
            {"task_title", title},
            {"task_description", description ?? "No description"},
            {"due_date", dueDate}
        };
    }
}
