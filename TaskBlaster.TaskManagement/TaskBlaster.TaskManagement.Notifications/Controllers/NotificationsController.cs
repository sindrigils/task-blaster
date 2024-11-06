using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBlaster.TaskManagement.Notifications.Models;
using TaskBlaster.TaskManagement.Notifications.Services.Interfaces;

namespace TaskBlaster.TaskManagement.Notifications.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class NotificationsController(IMailService mailService) : ControllerBase
{
    /// <summary>
    /// Sends a basic email
    /// </summary>
    /// <param name="inputModel">An input model used to populate the basic email</param>
    [HttpPost("emails/basic")]
    public async Task<ActionResult> SendBasicEmail([FromBody] BasicEmailInputModel inputModel)
    {
        var contentType = inputModel.IsHtml ? EmailContentType.Html : EmailContentType.Text;
        await mailService.SendBasicEmailAsync(inputModel.To, inputModel.Subject, inputModel.Content, contentType);

        return Ok("Email sent successfully.");
    }

    /// <summary>
    /// Sends a templated email (optional)
    /// </summary>
    /// <param name="inputModel">An input model used to populate the templated email</param>
    [HttpPost("emails/template")]
    public async Task<ActionResult> SendTemplatedEmail([FromBody] TemplateEmailInputModel inputModel)
    {
        await mailService.SendTemplateEmailAsync(inputModel.To, inputModel.Subject, inputModel.TemplateId ?? 0, inputModel.Variables);
        return Ok("Templated email sent successfully.");
    }
}