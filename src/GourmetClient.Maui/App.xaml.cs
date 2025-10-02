using Microsoft.Maui;
using Microsoft.Maui.Controls;
using GourmetClient.Maui.Views;

namespace GourmetClient.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell())
		{
			Title = "Gourmet Client"
		};

		return window;
	}
}