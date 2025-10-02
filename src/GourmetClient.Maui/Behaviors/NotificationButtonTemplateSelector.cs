using System;
using Microsoft.Maui.Controls;

namespace GourmetClient.Maui.Behaviors;

public class NotificationButtonTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UpdateNotificationTemplate { get; set; }

    public DataTemplate? ExceptionNotificationTemplate { get; set; }

    public DataTemplate? EmptyTemplate { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        // For now, return a simple template based on object type name
        // This will be expanded when ViewModels are properly integrated
        return item?.GetType().Name switch
        {
            "UpdateNotification" => UpdateNotificationTemplate,
            "ExceptionNotification" => ExceptionNotificationTemplate,
            _ => EmptyTemplate
        };
    }
}