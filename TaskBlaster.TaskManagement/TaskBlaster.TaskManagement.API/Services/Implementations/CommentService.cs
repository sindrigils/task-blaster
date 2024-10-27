using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.Exceptions;
using TaskBlaster.TaskManagement.Models.InputModels;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class CommentService(ICommentRepository commentRepository, ITaskRepository taskRepository, IUserRepository userRepository) : ICommentService
{
    public async Task<IEnumerable<CommentDto>> GetCommentsAssociatedWithTaskAsync(int taskId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        var taskExists = await taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExists)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found.");
        }
        return await commentRepository.GetCommentsAssociatedWithTaskAsync(taskId);
    }

    public async Task AddCommentToTaskAsync(int taskId, string user, CommentInputModel comment)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        var taskExists = await taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExists)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found.");
        }
        var userExists = await userRepository.DoesUserExistAsync(user);
        if (!userExists)
        {
            throw new ResourceNotFoundException($"No user with name {user} found.");
        }
        await commentRepository.AddCommentToTaskAsync(taskId, user, comment);
    }

    public async Task RemoveCommentFromTaskAsync(int taskId, int commentId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(taskId, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(commentId, 1);

        var taskExists = await taskRepository.DoesTaskExistAsync(taskId);
        if (!taskExists)
        {
            throw new ResourceNotFoundException($"No task with id {taskId} found.");
        }

        var commentExists = await commentRepository.DoesCommentExistAsync(commentId, taskId);
        if (!commentExists)
        {
            throw new ResourceNotFoundException($"No comment with id {commentId} found.");
        }
        await commentRepository.RemoveCommentFromTaskAsync(taskId, commentId);
    }
}