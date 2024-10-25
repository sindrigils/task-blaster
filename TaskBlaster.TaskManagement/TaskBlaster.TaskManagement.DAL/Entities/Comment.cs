namespace TaskBlaster.TaskManagement.DAL.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Author { get; set; } = null!;
    public string ContentAsMarkdown { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public int TaskId { get; set; }
    public Task Task { get; set; } = new Task();
}