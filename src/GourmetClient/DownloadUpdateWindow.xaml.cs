using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using GourmetClient.Update;

namespace GourmetClient;

public partial class DownloadUpdateWindow : Window
{
    public DownloadUpdateWindow()
    {
        InitializeComponent();

        Closing += OnClosing;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        Closing -= OnClosing;
        CancelButton.Command?.Execute(null);
    }

    public Task StartUpdate(ReleaseDescription updateRelease)
    {
        return DownloadUpdateView.StartUpdate(updateRelease);
    }
}