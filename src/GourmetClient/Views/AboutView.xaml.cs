using System.Windows.Controls;
using GourmetClient.ViewModels;

namespace GourmetClient.Views;
	
public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();

        DataContext = new AboutViewModel();
    }
}