namespace TaskBlaster.TaskManagement.Models.Dtos;

public class TaskDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "";
    public string Priority { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string CreatedBy { get; set; } = "";
    public string? AssignedToUser { get; set; }
    // Þetta ætti örgl að vera TagDto en þetta er string utaf það er þannig í verkefnalýsingunni
    public List<string> Tags { get; set; } = [];
    public List<CommentDto> Comments { get; set; } = [];
}