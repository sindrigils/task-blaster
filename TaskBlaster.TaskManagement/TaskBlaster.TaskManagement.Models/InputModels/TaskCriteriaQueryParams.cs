using System.ComponentModel.DataAnnotations;

namespace TaskBlaster.TaskManagement.Models.InputModels;

public class TaskCriteriaQueryParams
{
    [Required]
    public int PageSize { get; set; }
    [Required]
    public int PageNumber { get; set; }
    public string? SearchValue { get; set; }
}