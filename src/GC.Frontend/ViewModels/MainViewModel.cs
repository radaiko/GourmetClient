﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace GC.Frontend.ViewModels;

public partial class MainViewModel : ObservableObject {
#pragma warning disable CA1822 // Mark members as static
  public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
}