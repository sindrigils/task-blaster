using System.ComponentModel.DataAnnotations;

namespace TaskBlaster.TaskManagement.Models.InputModels;

public class UserInputModel
{
    public string FullName { get; set; } = "";
    [Required]
    public string EmailAddress { get; set; } = "";
    [Url]
    public string? ProfileImageUrl { get; set; }
}