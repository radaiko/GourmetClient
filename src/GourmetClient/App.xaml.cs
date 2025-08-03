using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GourmetClient.Notifications;
using GourmetClient.Update;
using GourmetClient.Utils;

namespace GourmetClient;

public partial class App : Application
{
    private const string ReleaseNotesTokenFileName = "ReleaseNotes.token";

    public static string LocalAppDataPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GourmetClient");

    protected override void OnStartup(StartupEventArgs e)
    {
        AddExceptionHandlers();

        if (e.Args.Length > 1 && e.Args[0] == "/update")
        {
            StartUpdater(e.Args[1]);
        }
        else
        {
            bool force = e.Args.Any(arg => arg == "/force");
            bool checkForPreRelease = e.Args.Any(arg => arg == "/checkForPreRelease");

            StartApplication(force, checkForPreRelease || InstanceProvider.UpdateService.CurrentVersion.IsPrerelease);
        }
    }

    private void AddExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) => UnhandledExceptionOccurred(args.ExceptionObject as Exception);
        DispatcherUnhandledException += (_, args) => UnhandledExceptionOccurred(args.Exception);
        TaskScheduler.UnobservedTaskException += (_, args) => UnhandledExceptionOccurred(args.Exception);
    }

    private void UnhandledExceptionOccurred(Exception? exception)
    {
        if (exception is null)
        {
            MessageBox.Show(
                "Ein unerwarteter Fehler ist aufgetreten. Die Anwendung wird beendet",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        else
        {
            ShowExceptionNotification("Ein unerwarteter Fehler ist aufgetreten. Die Anwendung wird beendet", exception);
        }

        Environment.Exit(1);
    }

    private void StartApplication(bool force, bool checkForPreRelease)
    {
        if (!force)
        {
            Process? runningInstance = GetRunningInstance();
            if (runningInstance is not null)
            {
                if (runningInstance.MainWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(runningInstance.MainWindowHandle, SW_RESTORE);
                    SetForegroundWindow(runningInstance.MainWindowHandle);
                }

                Current.Shutdown();
                return;
            }
        }

        if (InstanceProvider.SettingsService.GetCurrentUpdateSettings().CheckForUpdates)
        {
            CheckForUpdates(checkForPreRelease);
        }

        var mainWindow = new MainWindow();

        if (!File.Exists(GetReleaseNotesTokenFilePath()))
        {
            mainWindow.Loaded += MainWindowOnLoaded;
        }

        mainWindow.Show();
    }

    private void MainWindowOnLoaded(object sender, EventArgs e)
    {
        var mainWindow = (MainWindow)sender;
        mainWindow.Loaded -= MainWindowOnLoaded;

        var releaseNotesWindow = new ReleaseNotesWindow
        {
            Owner = mainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        releaseNotesWindow.Show();

        try
        {
            File.Create(GetReleaseNotesTokenFilePath()).Dispose();
        }
        catch (IOException)
        {
            // Ignore the case that the file could not have been created. This only means that the release notes
            // windows is shown again at the next start of the application.
        }
    }

    private async void StartUpdater(string targetPath)
    {
        if (!Directory.Exists(targetPath))
        {
            MessageBox.Show(
                "Der Pfad zum Zielverzeichnis ist ungültig. Das Verzeichnis existiert nicht.",
                "GourmetClient Updater",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Current.Shutdown();
            return;
        }

        var maxTries = 50;
        var counter = 0;

        while (GetRunningInstance() is not null)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            counter++;

            if (counter >= maxTries)
            {
                MessageBox.Show(
                    "GourmetClient wurde nicht beendet. Update kann nicht gestartet werden.",
                    "GourmetClient Updater",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Current.Shutdown();
                return;
            }
        }

        var executeUpdateWindow = new ExecuteUpdateWindow();
        executeUpdateWindow.Closing += ExecuteUpdateWindowOnClosing;

        executeUpdateWindow.Show();

        Exception? updateException = null;
        try
        {
            await executeUpdateWindow.StartUpdate(targetPath);
        }
        catch (GourmetUpdateException exception)
        {
            updateException = exception;
        }
        finally
        {
            executeUpdateWindow.Closing -= ExecuteUpdateWindowOnClosing;
        }

        if (updateException is not null)
        {
            ShowExceptionNotification("Bei der Durchführung des Updates ist ein Fehler aufgetreten.", updateException);
        }

        executeUpdateWindow.Close();
        Current.Shutdown();
    }

    private void ShowExceptionNotification(string message, Exception exception)
    {
        new ExceptionNotificationDetailWindow
        {
            Title = "Fehler",
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Notification = new ExceptionNotification(message, exception)
        }.ShowDialog();
    }

    private void ExecuteUpdateWindowOnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
    }

    private Process? GetRunningInstance()
    {
        Process currentProcess = Process.GetCurrentProcess();
        return Process.GetProcessesByName(currentProcess.ProcessName).FirstOrDefault(process => process.Id != currentProcess.Id);
    }

    private async void CheckForUpdates(bool checkForPreRelease)
    {
        ReleaseDescription? updateRelease = await InstanceProvider.UpdateService.CheckForUpdate(checkForPreRelease);
        if (updateRelease is not null)
        {
            InstanceProvider.NotificationService.Send(
                new UpdateNotification("Es ist eine neue Version verfügbar", () => UpdateHelper.StartUpdate(updateRelease)));
        }
    }

    private static string GetReleaseNotesTokenFilePath()
    {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, ReleaseNotesTokenFileName);
    }

    private const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}