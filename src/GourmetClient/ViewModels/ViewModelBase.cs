using GourmetClient.Utils;

namespace GourmetClient.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public abstract void Initialize();
}