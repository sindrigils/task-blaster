

using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Notifications.Services.Interfaces;

namespace TaskBlaster.TaskManagement.Notifications.Services.Jobs;

public class DueDateReminderJob
{
    private readonly ITaskService _taskService;
    private readonly IMailService _mailService;
    private readonly IUserRepository _userRepository;

    private readonly int DueDateTodayTemplateId = 6424349;
    private readonly int DueDateYesterdayTemplateId = 6424356;

    public DueDateReminderJob(ITaskService taskService, IMailService mailService, IUserRepository userRepository)
    {
        _taskService = taskService;
        _mailService = mailService;
        _userRepository = userRepository;
    }

    public async Task ExecuteAsync()
    {
        // Fetch tasks with due dates today or a day overdue that haven't received reminders
        var tasks = await _taskService.GetTasksForNotifications();

        foreach (var task in tasks)
        {
            var user = await _userRepository.GetUserByNameAsync(task.AssignedToUser ?? "");
            if (user == null) continue;

            var templateVariables = new Dictionary<string, object>
            {
                {"name", user.FullName},
                {"task_title", task.Title},
            };

            // Send due date reminder if today is the due date
            if (task.DueDate.HasValue && task.DueDate.Value.Date == DateTime.UtcNow.Date && !task.Notification.DueDateNotificationSent)
            {
                await _mailService.SendTemplateEmailAsync(user.EmailAddress, "Due today is TODAY", DueDateTodayTemplateId, templateVariables);
            }
            // Send day-after reminder if due date was yesterday
            else if (task.DueDate.HasValue && task.DueDate.Value.Date == DateTime.UtcNow.AddDays(-1).Date && !task.Notification.DayAfterNotificationSent)
            {
                await _mailService.SendTemplateEmailAsync(user.EmailAddress, "Due today was YESTERDAY", DueDateYesterdayTemplateId, templateVariables);
            }
        }
        await _taskService.UpdateTaskNotifications();
    }
}