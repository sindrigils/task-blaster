using System.ComponentModel.DataAnnotations;

namespace TaskBlaster.TaskManagement.Models.InputModels;

public class CommentInputModel
{
    [Required]
    public string ContentAsMarkdown { get; set; } = "";
}