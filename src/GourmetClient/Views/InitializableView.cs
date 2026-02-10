using System.Windows;
using System.Windows.Controls;
using GourmetClient.ViewModels;

namespace GourmetClient.Views;

public abstract class InitializableView : UserControl
{
    protected InitializableView()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        var viewModel = DataContext as ViewModelBase;
        viewModel?.Initialize();
    }
}