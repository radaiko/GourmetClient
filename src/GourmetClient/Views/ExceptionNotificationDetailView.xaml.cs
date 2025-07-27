using System.Windows.Controls;
using GourmetClient.Notifications;
using GourmetClient.ViewModels;

namespace GourmetClient.Views;

public partial class ExceptionNotificationDetailView : UserControl
{
    public ExceptionNotificationDetailView()
    {
        InitializeComponent();
    }

    public ExceptionNotification? Notification
    {
        get => (DataContext as ExceptionNotificationDetailViewModel)?.GetNotification();
        set => DataContext = value is not null ? new ExceptionNotificationDetailViewModel(value) : null;
    }
}