using GourmetClient.Core.Utils;
using GourmetClient.Maui.Behaviors;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace GourmetClient.ViewModels;

public class AboutViewModel : ViewModelBase
{
    public AboutViewModel()
    {
        AppVersion = InstanceProvider.UpdateService.CurrentVersion.ToString();

        ShowReleaseNotesCommand = new DelegateCommand(ShowReleaseNotes);
        OpenIconAuthorWebPageCommand = new DelegateCommand(() => OpenUrlInBrowser("https://www.flaticon.com/authors/smashicons"));
        OpenIconWebPageCommand = new DelegateCommand(() => OpenUrlInBrowser("https://www.flaticon.com"));
    }

    public string AppVersion { get; }

    public ICommand ShowReleaseNotesCommand { get; }

    public ICommand OpenIconAuthorWebPageCommand { get; }

    public ICommand OpenIconWebPageCommand { get; }

    public override void Initialize()
    {
    }

    private async void ShowReleaseNotes()
    {
        // Navigate to release notes page instead of opening a window
        await Shell.Current.GoToAsync("//releasenotes");
    }

    private async void OpenUrlInBrowser(string url)
    {
        try
        {
            await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            // Handle exception if browser fails to open
            Debug.WriteLine($"Failed to open browser: {ex.Message}");
        }
    }
}