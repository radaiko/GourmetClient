using System;
using System.Threading.Tasks;
using GourmetClient.Core.Update;
using GourmetClient.Core.Notifications;
using GourmetClient.Core.Utils;
using Microsoft.Maui.Controls;
using GourmetClient.Maui.Views;

namespace GourmetClient.Maui.Utils;

public class MauiUpdateHandler : IUpdateHandler
{
    public async Task HandleUpdateAsync(ReleaseDescription updateRelease)
    {
        var notificationService = InstanceProvider.NotificationService;

        try
        {
            // Show a modal page for update progress (replace with your actual page)
            var downloadUpdatePage = new DownloadUpdatePage(updateRelease);
            await Application.Current.MainPage.Navigation.PushModalAsync(downloadUpdatePage);

            await downloadUpdatePage.StartUpdateAsync();

            await Application.Current.MainPage.Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            notificationService.Send(
                new ExceptionNotification("Failed to update version", ex));
        }
    }
}