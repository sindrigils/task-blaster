using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.Models;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.InputModels;

namespace TaskBlaster.TaskManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class TasksController(ITaskService taskService, ICommentService commentService) : ControllerBase
{
    /// <summary>
    /// Returns all tasks by a provided criteria as a paginated result
    /// </summary>
    /// <param name="query">A query which is used to paginate and filter the result</param>
    /// <returns>A filtered and paginated list of tasks</returns>
    [HttpGet("", Name = "GetPaginatedTasksByCriteria")]
    public async Task<ActionResult<Envelope<TaskDto>>> GetPaginatedTasksByCriteria([FromQuery] TaskCriteriaQueryParams query)
    {
        var tasks = await taskService.GetPaginatedTasksByCriteriaAsync(query);
        return Ok(tasks);
    }

    /// <summary>
    /// Returns a single task by id
    /// </summary>
    /// <param name="taskId">The id of the task</param>
    /// <returns>A single task or null</returns>
    [HttpGet("{taskId}", Name = "GetTaskById")]
    public async Task<ActionResult<TaskDetailsDto?>> GetTaskById(int taskId)
    {
        var task = await taskService.GetTaskByIdAsync(taskId);
        return Ok(task);
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="task">Input model used to populate the new task</param>
    [HttpPost("", Name = "CreateNewTask")]
    public async Task<ActionResult> CreateNewTask([FromBody] TaskInputModel task)
    {
        var newId = await taskService.CreateNewTaskAsync(task);
        return CreatedAtRoute("GetTaskById", new { taskId = newId }, new { id = newId });
    }

    /// <summary>
    /// Archives a task by id
    /// </summary>
    /// <param name="taskId">The id of the task which should be archived</param>
    [HttpDelete("{taskId}", Name = "ArchiveTaskById")]
    public async Task<ActionResult> ArchiveTaskById(int taskId)
    {
        await taskService.ArchiveTaskByIdAsync(taskId);
        return Ok();
    }

    /// <summary>
    /// Assigns a user from a task by id
    /// </summary>
    /// <param name="taskId">The id of the task</param>
    /// <param name="userId">The id of the user which should be assigned</param>
    [HttpPatch("{taskId}/assign/{userId}", Name = "AssignUserToTask")]
    public async Task<ActionResult> AssignUserToTask(int taskId, int userId)
    {
        await taskService.AssignUserToTaskAsync(taskId, userId);
        return NoContent();
    }

    /// <summary>
    /// Unassigns a user from a task by id
    /// </summary>
    /// <param name="taskId">The id of the task</param>
    /// <param name="userId">The id of the user which should be unassigned</param>
    [HttpPatch("{taskId}/unassign/{userId}", Name = "UnassignUserFromTask")]
    public async Task<ActionResult> UnassignUserFromTask(int taskId, int userId)
    {
        await taskService.UnassignUserFromTaskAsync(taskId, userId);
        return NoContent();
    }

    /// <summary>
    /// Updates the status of a task, e.g. 'pending', 'completed'
    /// </summary>
    /// <param name="taskId">The id of the task</param>
    /// <param name="inputModel">The input model associated with the status update</param>
    [HttpPatch("{taskId}/status", Name = "UpdateTaskStatus")]
    public async Task<ActionResult> UpdateTaskStatus(int taskId, [FromBody] StatusInputModel inputModel)
    {
        await taskService.UpdateTaskStatusAsync(taskId, inputModel);
        return NoContent();
    }

    /// <summary>
    /// Updates the priority of a task, e.g. 'Critical', 'High'
    /// </summary>
    /// <param name="taskId">The id of the task</param>
    /// <param name="inputModel">The input model associated with the priority update</param>
    [HttpPatch("{taskId}/priority", Name = "UpdateTaskPriority")]
    public async Task<ActionResult> UpdateTaskPriority(int taskId, [FromBody] PriorityInputModel inputModel)
    {
        await taskService.UpdateTaskPriorityAsync(taskId, inputModel);
        return NoContent();
    }

    /// <summary>
    /// Gets all comments associated with a task
    /// </summary>
    /// <param name="taskId">The id of the task</param>
    /// <returns>A list of comments</returns>
    [HttpGet("{taskId}/comments", Name = "GetCommentsAssociatedWithTask")]
    public async Task<ActionResult> GetCommentsAssociatedWithTask(int taskId)
    {
        var comments = await commentService.GetCommentsAssociatedWithTaskAsync(taskId);
        return Ok(comments);
    }

    /// <summary>
    /// Adds a single comment to a task
    /// </summary>
    /// <param name="id">The id of the task</param>
    /// <param name="inputModel">The input model for the comment</param>
    [HttpPost("{id}/comments", Name = "AddCommentToTask")]
    public async Task<ActionResult> AddCommentToTask(int id, [FromBody] CommentInputModel inputModel)
    {
        var userName = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "";

        await commentService.AddCommentToTaskAsync(id, userName, inputModel);
        return CreatedAtRoute("GetCommentsAssociatedWithTask", new { taskId = id }, null);
    }

    /// <summary>
    /// Removes a comment from a task
    /// </summary>
    /// <param name="taskId">The id of the task</param>
    /// <param name="commentId">The id of the comment</param>
    [HttpDelete("{taskId}/comments/{commentId}", Name = "RemoveCommentFromTask")]
    public async Task<ActionResult> RemoveCommentFromTask(int taskId, int commentId)
    {
        await commentService.RemoveCommentFromTaskAsync(taskId, commentId);
        return NoContent();
    }
}
