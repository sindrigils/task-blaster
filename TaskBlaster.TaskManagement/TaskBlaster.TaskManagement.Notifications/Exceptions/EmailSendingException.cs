namespace TaskBlaster.TaskManagement.Notifications.Exceptions;

public class EmailSendingException : Exception
{
    public EmailSendingException(string message, int statusCode)
        : base($"Email sending failed with status {statusCode}: {message}")
    {
    }
}
