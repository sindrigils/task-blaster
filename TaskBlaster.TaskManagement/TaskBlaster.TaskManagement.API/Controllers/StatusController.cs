using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;

namespace TaskBlaster.TaskManagement.API.Controllers;

[Authorize]
[Route("[controller]")]
[ApiController]
public class StatusController(IStatusService statusService) : ControllerBase
{
    /// <summary>
    /// Returns a list of all statuses
    /// </summary>
    /// <returns>A list of all statuses</returns>
    [HttpGet("", Name = "GetAllStatuses")]
    public async Task<ActionResult<IEnumerable<StatusDto>>> GetAllStatuses()
    {
        var statuses = await statusService.GetAllStatusesAsync();
        return Ok(statuses);
    }
}