using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GC.Models;

namespace GC.Frontend.ViewModels;

public partial class SettingsViewModel : ObservableObject{
 [ObservableProperty] private Settings _settings = Settings.It;
}