using System.ComponentModel.DataAnnotations;

namespace TaskBlaster.TaskManagement.Models.InputModels;

public class TagInputModel
{
    [Required]
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}