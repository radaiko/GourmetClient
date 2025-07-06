using System.Windows.Controls;
using GourmetClient.ViewModels;

namespace GourmetClient.Views;

public partial class NotificationsView : UserControl
{
    public NotificationsView()
    {
        InitializeComponent();

        DataContext = new NotificationsViewModel();
    }
}