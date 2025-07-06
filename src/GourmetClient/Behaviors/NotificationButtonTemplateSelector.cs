using System.Windows;
using System.Windows.Controls;
using GourmetClient.Notifications;

namespace GourmetClient.Behaviors;

public class NotificationButtonTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UpdateNotificationTemplate { get; set; }

    public DataTemplate? ExceptionNotificationTemplate { get; set; }
        
    public DataTemplate? EmptyTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item switch
        {
            UpdateNotification => UpdateNotificationTemplate,
            ExceptionNotification => ExceptionNotificationTemplate,
            _ => EmptyTemplate
        };
    }
}