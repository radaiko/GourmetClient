using System.Windows;
using System.Windows.Controls;
using GourmetClient.Network;

namespace GourmetClient.Behaviors;

public class ExceptionContentTemplateSelector : DataTemplateSelector
{
    public DataTemplate? GourmetRequestExceptionTemplate { get; set; }

    public DataTemplate? GourmetParseExceptionTemplate { get; set; }

    public DataTemplate? GenericExceptionTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item switch
        {
            GourmetRequestException => GourmetRequestExceptionTemplate,
            GourmetParseException => GourmetParseExceptionTemplate,
            _ => GenericExceptionTemplate
        };
    }
}