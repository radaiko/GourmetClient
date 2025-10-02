using System;
using Microsoft.Maui.Controls;

namespace GourmetClient.Maui.Behaviors;

public class ExceptionContentTemplateSelector : DataTemplateSelector
{
    public DataTemplate? GourmetRequestExceptionTemplate { get; set; }

    public DataTemplate? GourmetParseExceptionTemplate { get; set; }

    public DataTemplate? GenericExceptionTemplate { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        // For now, return a simple template based on object type name
        // This will be expanded when ViewModels are properly integrated
        return item?.GetType().Name switch
        {
            "GourmetRequestException" => GourmetRequestExceptionTemplate,
            "GourmetParseException" => GourmetParseExceptionTemplate,
            _ => GenericExceptionTemplate
        };
    }
}