using Microsoft.AspNetCore.Mvc;
using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;

namespace TaskBlaster.TaskManagement.API.Controllers;

[Route("[controller]")]
[ApiController]
public class PrioritiesController(IPriorityService priorityService) : ControllerBase
{
    /// <summary>
    /// Returns a list of all priorities
    /// </summary>
    /// <returns>A list of all priorities</returns>
    [HttpGet("")]
    public async Task<ActionResult<IEnumerable<PriorityDto>>> GetAllPriorities()
    {
        var priorities = await priorityService.GetAllPrioritiesAsync();
        return Ok(priorities);
    }

}