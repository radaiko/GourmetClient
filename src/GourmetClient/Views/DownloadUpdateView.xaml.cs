using System.Threading.Tasks;
using GourmetClient.Core.Update;
using GourmetClient.Update;
using GourmetClient.ViewModels;

namespace GourmetClient.Views;

public partial class DownloadUpdateView : InitializableView
{
    public DownloadUpdateView()
    {
        InitializeComponent();

        DataContext = new DownloadUpdateViewModel();
    }

    public Task StartUpdate(ReleaseDescription updateRelease)
    {
        return ((DownloadUpdateViewModel)DataContext).StartUpdate(updateRelease);
    }
}