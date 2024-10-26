using Microsoft.EntityFrameworkCore;
using TaskBlaster.TaskManagement.DAL.Data;
using TaskBlaster.TaskManagement.DAL.Entities;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.InputModels;
using Task = System.Threading.Tasks.Task;

namespace TaskBlaster.TaskManagement.DAL.Implementations;

public class CommentRepository(TaskBlasterDbContext dbContext) : ICommentRepository
{

    private readonly TaskBlasterDbContext _dbContext = dbContext;

    public async Task AddCommentToTaskAsync(int taskId, string user, CommentInputModel comment)
    {

        var task = await _dbContext.Tasks.Include(t => t.Comments).FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null) return;

        var newComment = new Comment
        {
            Author = user,
            ContentAsMarkdown = comment.ContentAsMarkdown,
            CreatedDate = DateTime.UtcNow,
            TaskId = taskId
        };
        await _dbContext.AddAsync(newComment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsAssociatedWithTaskAsync(int taskId)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) return [];

        var comments = task.Comments.Select(c => new CommentDto
        {
            Id = c.Id,
            Author = c.Author,
            ContentAsMarkdown = c.ContentAsMarkdown,
            CreatedDate = c.CreatedDate
        });

        return comments;
    }

    public async Task RemoveCommentFromTaskAsync(int taskId, int commentId)
    {
        var comment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.TaskId == taskId);
        if (comment == null)
        {
            throw new NotImplementedException();
        }
        _dbContext.Remove(comment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> DoesCommentExistAsync(int commentId)
    {
        return await _dbContext.Comments.AnyAsync(c => c.Id == commentId);

    }
}