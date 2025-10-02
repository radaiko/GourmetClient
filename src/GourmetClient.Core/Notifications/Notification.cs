using System;

namespace GourmetClient.Core.Notifications;

public class Notification
{
    public Notification(NotificationType notificationType, string message)
    {
        NotificationType = notificationType;
        Message = message;
        Timestamp = DateTime.Now;
    }

    public NotificationType NotificationType { get; }

    public string Message { get; }

    public DateTime Timestamp { get; }
}