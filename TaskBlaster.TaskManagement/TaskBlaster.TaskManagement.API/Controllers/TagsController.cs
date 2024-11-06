using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.InputModels;

namespace TaskBlaster.TaskManagement.API.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class TagsController(ITagService tagService) : ControllerBase
{
    /// <summary>
    /// Gets all tags
    /// </summary>
    /// <returns>A list of all tags</returns>
    [HttpGet("", Name = "GetAllTags")]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAllTags()
    {
        var tags = await tagService.GetAllTagsAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    /// <param name="inputModel">An input model used to populate the new tag</param>
    [HttpPost("", Name = "CreateNewTag")]
    public async Task<ActionResult> CreateNewTag([FromBody] TagInputModel inputModel)
    {
        var newId = await tagService.CreateNewTagAsync(inputModel);
        return CreatedAtRoute("GetAllTags", null, new { id = newId });
    }
}