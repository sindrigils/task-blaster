namespace TaskBlaster.TaskManagement.DAL.Entities;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public int PriorityId { get; set; }
    public Priority Priority { get; set; } = null!;
    public int StatusId { get; set; }
    public Status Status { get; set; } = null!;
    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;
    public bool IsArchived { get; set; } = false;

    public ICollection<TaskTag> TaskTags { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public TaskNotification? TaskNotification { get; set; }
}