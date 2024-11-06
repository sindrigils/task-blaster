using System.ComponentModel.DataAnnotations;

namespace TaskBlaster.TaskManagement.Notifications.Models;

public class TemplateEmailInputModel
{
    [Required]
    public string To { get; set; } = "";
    [Required]
    public string Subject { get; set; } = "";
    [Required]
    public int? TemplateId { get; set; }
    public Dictionary<string, object> Variables { get; set; } = null!;
}