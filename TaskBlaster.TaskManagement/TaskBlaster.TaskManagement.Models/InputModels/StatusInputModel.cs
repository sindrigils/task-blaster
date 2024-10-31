using System.ComponentModel.DataAnnotations;

namespace TaskBlaster.TaskManagement.Models.InputModels;

public class StatusInputModel
{
    [Required]
    public int? StatusId { get; set; }
}