namespace TaskBlaster.TaskManagement.DAL.Entities;

public class TaskNotification
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public Task Task { get; set; } = null!;
    public bool DueDateNotificationSent { get; set; }
    public bool DaysAfterNotificationSent { get; set; }
    public DateTime? LastNotificationDate { get; set; }
}
