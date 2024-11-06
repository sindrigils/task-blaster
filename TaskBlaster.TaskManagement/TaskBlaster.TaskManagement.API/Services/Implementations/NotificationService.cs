using System.Text;
using System.Text.Json;

using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Exceptions;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IM2MAuthenticationService _m2MAuthenticationService;
    private const int AssignTemplateId = 6418991;
    private const int UnassignTemplateId = 6418994;

    public NotificationService(
        HttpClient httpClient,
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        IM2MAuthenticationService m2MAuthenticationService)
    {
        _httpClient = httpClient;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _m2MAuthenticationService = m2MAuthenticationService;
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

        var authUser = _httpContextAccessor.HttpContext?.User;
        if (authUser == null)
        {
            throw new UnauthorizedAccessException("No authenticated user found in the current context.");
        }
        var name = authUser?.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";
        var email = authUser?.Claims.FirstOrDefault(c => c.Type == "email_address")?.Value ?? "";

        var token = await _m2MAuthenticationService.RetrieveAccessToken(
            fullName: name,
            emailAddress: email
        );

        var requestBody = CreateRequestBody(user.EmailAddress, subject, templateId, BuildTemplateVariables(user.FullName, task.Title, task.Description, task.DueDate));
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.PostAsync("http://task-management-notifications-api:80/api/notifications/emails/template", content);
        // the url to the Notificiations API in Azure
        // var response = await _httpClient.PostAsync("http://task-blaster-notifications.azurewebsites.net/api/notifications/emails/template", content);

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

    private Dictionary<string, object> BuildTemplateVariables(string name, string title, string? description, DateTime? dueDate)
    {
        var templateVariables = new Dictionary<string, object>
        {
            {"name", name},
            {"task_title", title},
        };

        if (description != null)
        {
            templateVariables["task_description"] = description;
        }

        if (dueDate != null)
        {
            templateVariables["due_date"] = dueDate;
        }

        return templateVariables;
    }

}
