using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

using TaskBlaster.TaskManagement.Notifications.Models;
using TaskBlaster.TaskManagement.Notifications.Services.Interfaces;
using TaskBlaster.TaskManagement.Notifications.Exceptions;

namespace TaskBlaster.TaskManagement.Notifications.Services.Implementations;

public class MailjetService : IMailService
{
    private readonly MailjetClient _client;
    private readonly IConfiguration _configuration;
    private readonly string DefaultName = "there";
    private readonly string DefaultTaskTitle = "No task title";
    private readonly string DefaultTaskDescription = "No task description";
    // Now this does not make much sense, since there will always be a due date, but I get a warning
    private readonly string DefaultDueDate = "No due date";

    public MailjetService(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new MailjetClient(_configuration["MailJet:ApiKey"], _configuration["MailJet:SecretKey"]);
    }

    public async Task SendBasicEmailAsync(string to, string subject, string content, EmailContentType contentType)
    {
        var request = new MailjetRequest
        {
            Resource = Send.Resource,
        }
        .Property(Send.FromEmail, "sindrir22@ru.is")
        .Property(Send.FromName, "Sindri Gils Robertsson")
        .Property(Send.Subject, subject)
        .Property(Send.MjTemplateLanguage, true)
        .Property(contentType == EmailContentType.Text ? Send.TextPart : Send.HtmlPart, content)
        .Property(Send.Recipients, new JArray {
            new JObject { {"Email", to} }
        });

        MailjetResponse response = await _client.PostAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new EmailSendingException(response.GetErrorMessage(), response.StatusCode);
        }
    }

    public async Task SendTemplateEmailAsync(string to, string subject, int templateId, Dictionary<string, object> variables)
    {
        // Now this is terrible code, but for some reason TryGetValue does not work
        var templateVariables = new Dictionary<string, object>
        {
            { "name", variables.ContainsKey("name") ? variables["name"]?.ToString() ?? DefaultName : DefaultName },
            { "task_title", variables.ContainsKey("task_title") ? variables["task_title"]?.ToString() ?? DefaultTaskTitle : DefaultTaskTitle },
            { "task_description", variables.ContainsKey("task_description") ? variables["task_description"]?.ToString() ?? DefaultTaskDescription : DefaultTaskDescription },
            { "due_date", variables.ContainsKey("due_date") ? variables["due_date"]?.ToString() ?? DefaultDueDate : DefaultDueDate }
        };

        var request = new MailjetRequest
        {
            Resource = Send.Resource,
        }
        .Property(Send.FromEmail, "sindrir22@ru.is")
        .Property(Send.FromName, "Sindri Gils Robertsson")
        .Property(Send.Subject, subject)
        .Property(Send.Recipients, new JArray {
        new JObject { {"Email", to} }
        })
        .Property(Send.MjTemplateID, templateId)
        .Property(Send.MjTemplateLanguage, true)
        .Property(Send.Vars, JObject.FromObject(templateVariables));

        MailjetResponse response = await _client.PostAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new EmailSendingException(response.GetErrorMessage(), response.StatusCode);
        }
    }
}
