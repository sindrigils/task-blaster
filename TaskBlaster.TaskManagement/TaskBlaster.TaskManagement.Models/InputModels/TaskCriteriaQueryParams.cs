using System.ComponentModel.DataAnnotations;

namespace TaskBlaster.TaskManagement.Models.InputModels;

public class TaskCriteriaQueryParams
{
    [Required]
    public int PageSize { get; set; } = 15;
    [Required]
    public int PageNumber { get; set; } = 1;
    public string? SearchValue { get; set; }
}