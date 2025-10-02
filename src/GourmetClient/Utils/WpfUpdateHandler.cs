using System.Threading.Tasks;
using System.Windows;
using GourmetClient.Core.Update;
using GourmetClient.Core.Notifications;
using GourmetClient.Core.Utils;
using System;

namespace GourmetClient.Utils;

public class WpfUpdateHandler : IUpdateHandler
{
    public async Task HandleUpdateAsync(ReleaseDescription updateRelease)
    {
        var downloadUpdateWindow = new DownloadUpdateWindow
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var updateTask = downloadUpdateWindow.StartUpdate(updateRelease);
        try
        {
            downloadUpdateWindow.ShowDialog();
            await updateTask;
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Invoke(
                () => InstanceProvider.NotificationService.Send(
                    new ExceptionNotification("Aktualisieren der Version ist fehlgeschlagen", ex)));
        }
        finally
        {
            Application.Current.Dispatcher.Invoke(() => downloadUpdateWindow.Close());
        }
    }
}
