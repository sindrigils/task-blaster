using System.ComponentModel.DataAnnotations;

namespace TaskBlaster.TaskManagement.Models.InputModels;

public class PriorityInputModel
{
    [Required]
    public int PriorityId { get; set; }
}