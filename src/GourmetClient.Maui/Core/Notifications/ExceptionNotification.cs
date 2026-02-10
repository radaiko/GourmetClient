using System;

namespace GourmetClient.Maui.Core.Notifications;

public class ExceptionNotification : Notification
{
    public ExceptionNotification(string message, Exception exception)
        : base(NotificationType.Error, message)
    {
        Exception = exception;
    }

    public Exception Exception { get; }
}