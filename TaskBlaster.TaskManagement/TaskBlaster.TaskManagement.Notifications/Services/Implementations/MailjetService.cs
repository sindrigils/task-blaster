using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

using TaskBlaster.TaskManagement.Notifications.Models;
using TaskBlaster.TaskManagement.Notifications.Services.Interfaces;

namespace TaskBlaster.TaskManagement.Notifications.Services.Implementations;

public class MailjetService : IMailService
{
    private readonly MailjetClient _client;
    private readonly IConfiguration _configuration;

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

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Email sent successfully!");
        }
        else
        {
            Console.WriteLine($"Failed to send email. Status: {response.StatusCode}");
            Console.WriteLine(response.GetErrorMessage());
        }
    }

    public async Task SendTemplateEmailAsync(string to, string subject, int templateId, Dictionary<string, object> variables)
    {
        // Now this is terrible code, but for some reason TryGetValue does not work
        var templateVariables = new Dictionary<string, object>
        {
            { "name", variables.ContainsKey("name") ? variables["name"]?.ToString() ?? "Default Name" : "Default Name" },
            { "task_title", variables.ContainsKey("task_title") ? variables["task_title"]?.ToString() ?? "Default Title" : "Default Title" },
            { "task_description", variables.ContainsKey("task_description") ? variables["task_description"]?.ToString() ?? "Default Description" : "Default Description" },
            { "due_date", variables.ContainsKey("due_date") ? variables["due_date"]?.ToString() ?? "No Due Date" : "No Due Date" }
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

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Templated email sent successfully!");
        }
        else
        {
            Console.WriteLine($"Failed to send templated email. Status: {response.StatusCode}");
            Console.WriteLine(response.GetErrorMessage());
        }
    }
}
