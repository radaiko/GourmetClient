using System.Globalization;
using System.Windows.Data;

namespace GourmetClient.Behaviors;

public class CultureAwareBinding : Binding
{
    public CultureAwareBinding()
    {
        ConverterCulture = CultureInfo.CurrentCulture;
    }

    public CultureAwareBinding(string path)
        : base(path)
    {
        ConverterCulture = CultureInfo.CurrentCulture;
    }
}