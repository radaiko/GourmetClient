using GourmetClient.Update;
using System.Windows;
using System.Windows.Threading;
using GourmetClient.Notifications;

namespace GourmetClient.Utils;

public static class UpdateHelper
{
    public static void StartUpdate(ReleaseDescription updateRelease)
    {
        var downloadUpdateWindow = new DownloadUpdateWindow
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        downloadUpdateWindow.StartUpdate(updateRelease).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Application.Current.Dispatcher.Invoke(
                    () => InstanceProvider.NotificationService.Send(
                        new ExceptionNotification("Aktualisieren der Version ist fehlgeschlagen", task.Exception)));
            }

            Application.Current.Dispatcher.Invoke(() => downloadUpdateWindow.Close());
        });

        downloadUpdateWindow.ShowDialog();
    }
}