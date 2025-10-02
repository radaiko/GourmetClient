using GourmetClient.Core.Update;
using GourmetClient.ViewModels;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace GourmetClient.Maui.Views;

public partial class DownloadUpdatePage : ContentPage
{
    private readonly DownloadUpdateViewModel _viewModel;

    public DownloadUpdatePage(ReleaseDescription updateRelease)
    {
        _viewModel = new DownloadUpdateViewModel();
        BindingContext = _viewModel;
        UpdateRelease = updateRelease;
    }

    public ReleaseDescription UpdateRelease { get; }

    public async Task StartUpdateAsync()
    {
        await _viewModel.StartUpdate(UpdateRelease);
    }
}