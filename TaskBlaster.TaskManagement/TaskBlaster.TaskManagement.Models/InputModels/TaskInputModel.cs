using System.ComponentModel.DataAnnotations;

namespace TaskBlaster.TaskManagement.Models.InputModels;

public class TaskInputModel
{
    [Required]
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    [Required]
    public int StatusId { get; set; }
    [Required]
    public int PriorityId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssignedToUser { get; set; }
}